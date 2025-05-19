using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace GotoCarRental.Areas.Customer.Pages.Rentals
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ICarRepository _carRepository;
        private readonly IRentalRepository _rentalRepository;

        public CreateModel(ICarRepository carRepository, IRentalRepository rentalRepository)
        {
            _carRepository = carRepository;
            _rentalRepository = rentalRepository;
        }

        public Car Car { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(1);

        [BindProperty]
        public int Days { get; set; } = 1;

        [BindProperty]
        public decimal TotalPrice { get; set; }

        [BindProperty]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = "Thanh toán khi nhận xe";

        public bool IsAvailable { get; set; }
        public IEnumerable<DateTime> UnavailableDates { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id, string startDate = null, string endDate = null)
        {
            if (id == null)
                return NotFound();

            Car = await _carRepository.GetByIdAsync(id.Value);

            if (Car == null || !Car.IsApproved)
                return NotFound();

            // Xử lý ngày bắt đầu và kết thúc từ query string
            if (DateTime.TryParse(startDate, out DateTime start) && start >= DateTime.Today)
                StartDate = start;

            if (DateTime.TryParse(endDate, out DateTime end) && end > StartDate)
                EndDate = end;
            else
                EndDate = StartDate.AddDays(1);

            // Tính số ngày thuê
            Days = (EndDate - StartDate).Days;
            if (Days < 1) Days = 1;

            // Tính tổng tiền
            TotalPrice = Car.PricePerDay * Days;

            // Kiểm tra xem xe có khả dụng trong khoảng thời gian đã chọn không
            IsAvailable = await _rentalRepository.IsCarAvailableAsync(Car.Id, StartDate, EndDate);

            // Lấy danh sách ngày không khả dụng (đã có người thuê)
            UnavailableDates = await GetUnavailableDates(Car.Id);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
                return RedirectToPage(new { id });

            Car = await _carRepository.GetByIdAsync(id);

            if (Car == null || !Car.IsApproved)
                return NotFound();

            // Kiểm tra xem xe có khả dụng trong khoảng thời gian đã chọn không
            var isAvailable = await _rentalRepository.IsCarAvailableAsync(id, StartDate, EndDate);
            if (!isAvailable)
            {
                TempData["ErrorMessage"] = "Xe đã được đặt trong khoảng thời gian bạn chọn. Vui lòng chọn khoảng thời gian khác.";
                return RedirectToPage(new { id });
            }

            // Tạo đơn thuê mới
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rental = new Rental
            {
                UserId = userId,
                CarId = id,
                StartDate = StartDate,
                EndDate = EndDate,
                TotalPrice = TotalPrice,
                Status = 0, // Chờ xác nhận
                PaymentStatus = "Pending",
                PaymentMethod = PaymentMethod,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            try
            {
                var newRental = await _rentalRepository.AddAsync(rental);
                TempData["SuccessMessage"] = "Đặt xe thành công! Vui lòng chờ xác nhận từ chủ xe.";
                return RedirectToPage("./Details", new { id = newRental.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi đặt xe: {ex.Message}";
                return RedirectToPage(new { id });
            }
        }

        public async Task<IActionResult> OnPostCalculateAsync(int id)
        {
            if (StartDate > EndDate)
            {
                TempData["ErrorMessage"] = "Ngày kết thúc phải sau ngày bắt đầu.";
                return RedirectToPage(new { id });
            }

            Days = (EndDate - StartDate).Days;
            if (Days < 1) Days = 1;

            Car = await _carRepository.GetByIdAsync(id);
            if (Car == null)
            {
                return NotFound();
            }

            TotalPrice = Car.PricePerDay * Days;
            IsAvailable = await _rentalRepository.IsCarAvailableAsync(id, StartDate, EndDate);
            UnavailableDates = await GetUnavailableDates(id);

            if (!IsAvailable)
            {
                TempData["ErrorMessage"] = "Xe đã được đặt trong khoảng thời gian bạn chọn. Vui lòng chọn khoảng thời gian khác.";
            }

            return Page();
        }

        private async Task<IEnumerable<DateTime>> GetUnavailableDates(int carId)
        {
            // Lấy tất cả các đơn thuê của xe này
            var rentals = await _rentalRepository.FindAsync(r => r.CarId == carId && r.Status != 2); // Không phải đơn đã hủy
            var unavailableDates = new List<DateTime>();

            foreach (var rental in rentals)
            {
                for (var date = rental.StartDate; date <= rental.EndDate; date = date.AddDays(1))
                {
                    unavailableDates.Add(date.Date);
                }
            }

            return unavailableDates;
        }
    }
}
