using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace GotoCarRental.Areas.CarOwner.Pages.Rentals
{
    [Authorize(Roles = "CarOwner")]
    public class IndexModel : PageModel
    {
        private readonly IRentalRepository _rentalRepository;
        private readonly ICarRepository _carRepository;

        public IndexModel(IRentalRepository rentalRepository, ICarRepository carRepository)
        {
            _rentalRepository = rentalRepository;
            _carRepository = carRepository;
        }

        public IEnumerable<Rental> Rentals { get; set; }
        public IEnumerable<Car> MyCars { get; set; }
        public int TotalRentals { get; set; }
        public int TotalPages { get; set; }
        public Dictionary<int, string> CarNames { get; set; } = new Dictionary<int, string>();

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CarId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "newest";

        public SelectList StatusSelectList { get; set; }
        public SelectList CarSelectList { get; set; }
        public SelectList SortSelectList { get; set; }

        public decimal TotalEarnings { get; set; }
        public int PageSize { get; set; } = 10;

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Lấy danh sách xe của người dùng hiện tại
            MyCars = await _carRepository.GetCarsByUserAsync(userId);

            // Tạo từ điển tên xe để hiển thị
            foreach (var car in MyCars)
            {
                CarNames[car.Id] = car.Name;
            }

            // Tạo danh sách chọn xe
            var carOptions = MyCars.Select(c => new { Id = c.Id, Name = c.Name }).ToList();
            CarSelectList = new SelectList(carOptions, "Id", "Name", CarId);

            // Tạo danh sách trạng thái
            var statusOptions = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("", "Tất cả trạng thái"),
                    new KeyValuePair<string, string>("0", "Chờ xác nhận"),
                    new KeyValuePair<string, string>("1", "Đã xác nhận"),
                    new KeyValuePair<string, string>("2", "Đã hủy"),
                    new KeyValuePair<string, string>("3", "Đã hoàn thành")
                };
            StatusSelectList = new SelectList(statusOptions, "Key", "Value", Status?.ToString() ?? "");

            // Tạo danh sách sắp xếp
            var sortOptions = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("newest", "Mới nhất"),
                    new KeyValuePair<string, string>("oldest", "Cũ nhất"),
                    new KeyValuePair<string, string>("start_date", "Ngày bắt đầu"),
                    new KeyValuePair<string, string>("price_desc", "Giá: Cao đến thấp"),
                    new KeyValuePair<string, string>("price_asc", "Giá: Thấp đến cao")
                };
            SortSelectList = new SelectList(sortOptions, "Key", "Value", SortBy);

            // Lấy danh sách đơn thuê xe
            var carIds = MyCars.Select(c => c.Id).ToList();
            var rentals = await GetRentalsForCars(carIds, CurrentPage, PageSize, Status, SortBy);

            Rentals = rentals.Rentals;
            TotalRentals = rentals.TotalCount;
            TotalPages = (int)Math.Ceiling(TotalRentals / (double)PageSize);

            // Tính tổng doanh thu từ các đơn đã hoàn thành hoặc đã xác nhận
            TotalEarnings = Rentals
                .Where(r => r.Status == 1 || r.Status == 3)
                .Sum(r => r.TotalPrice);

            return Page();
        }

        public async Task<IActionResult> OnPostConfirmAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rental = await _rentalRepository.GetByIdAsync(id);

            if (rental == null)
                return NotFound();

            // Kiểm tra xem người dùng có sở hữu xe này không
            var car = await _carRepository.GetByIdAsync(rental.CarId);
            if (car == null || car.UserId != userId)
                return Forbid();

            // Chỉ có thể xác nhận những đơn đang chờ
            if (rental.Status != 0)
            {
                TempData["ErrorMessage"] = "Chỉ có thể xác nhận những đơn đang chờ xác nhận.";
                return RedirectToPage();
            }

            await _rentalRepository.ConfirmRentalAsync(id);
            TempData["SuccessMessage"] = "Xác nhận đơn thuê xe thành công!";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rental = await _rentalRepository.GetByIdAsync(id);

            if (rental == null)
                return NotFound();

            // Kiểm tra xem người dùng có sở hữu xe này không
            var car = await _carRepository.GetByIdAsync(rental.CarId);
            if (car == null || car.UserId != userId)
                return Forbid();

            // Chỉ có thể hủy những đơn đang chờ hoặc đã xác nhận
            if (rental.Status != 0 && rental.Status != 1)
            {
                TempData["ErrorMessage"] = "Chỉ có thể hủy những đơn đang chờ xác nhận hoặc đã xác nhận.";
                return RedirectToPage();
            }

            await _rentalRepository.CancelRentalAsync(id);
            TempData["SuccessMessage"] = "Hủy đơn thuê xe thành công!";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCompleteAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rental = await _rentalRepository.GetByIdAsync(id);

            if (rental == null)
                return NotFound();

            // Kiểm tra xem người dùng có sở hữu xe này không
            var car = await _carRepository.GetByIdAsync(rental.CarId);
            if (car == null || car.UserId != userId)
                return Forbid();

            // Chỉ có thể hoàn thành những đơn đã xác nhận
            if (rental.Status != 1)
            {
                TempData["ErrorMessage"] = "Chỉ có thể hoàn thành những đơn đã xác nhận.";
                return RedirectToPage();
            }

            await _rentalRepository.CompleteRentalAsync(id);
            TempData["SuccessMessage"] = "Đơn thuê xe đã hoàn thành thành công!";

            return RedirectToPage();
        }

        private async Task<(IEnumerable<Rental> Rentals, int TotalCount)> GetRentalsForCars(List<int> carIds,
            int pageNumber, int pageSize, int? status = null, string sortBy = "newest")
        {
            // Lấy tất cả các đơn thuê cho các xe của chủ xe hiện tại
            var query = (await _rentalRepository.GetAllAsync())
                .Where(r => carIds.Contains(r.CarId));

            // Lọc theo status nếu có
            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            // Lọc theo CarId nếu có
            if (CarId.HasValue)
            {
                query = query.Where(r => r.CarId == CarId.Value);
            }

            // Sắp xếp theo các tiêu chí
            switch (sortBy)
            {
                case "oldest":
                    query = query.OrderBy(r => r.CreatedAt);
                    break;
                case "start_date":
                    query = query.OrderBy(r => r.StartDate);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(r => r.TotalPrice);
                    break;
                case "price_asc":
                    query = query.OrderBy(r => r.TotalPrice);
                    break;
                default: // newest
                    query = query.OrderByDescending(r => r.CreatedAt);
                    break;
            }

            var totalCount = query.Count();

            var rentals = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (rentals, totalCount);
        }
    }
}
