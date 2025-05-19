using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace GotoCarRental.Areas.Customer.Pages.Rentals
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IRentalRepository _rentalRepository;

        public IndexModel(IRentalRepository rentalRepository)
        {
            _rentalRepository = rentalRepository;
        }

        public IEnumerable<Rental> Rentals { get; set; }
        public int TotalRentals { get; set; }
        public int TotalPages { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "newest";

        public SelectList StatusSelectList { get; set; }
        public SelectList SortSelectList { get; set; }
        public decimal TotalSpent { get; set; }
        public int PageSize { get; set; } = 5;

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Lấy danh sách đơn thuê xe
            var result = await _rentalRepository.GetRentalsPagedAsync(
                CurrentPage,
                PageSize,
                userId,
                Status,
                SortBy);

            Rentals = result.Rentals;
            TotalRentals = result.TotalCount;
            TotalPages = (int)Math.Ceiling(TotalRentals / (double)PageSize);

            // Lấy tổng tiền đã chi
            TotalSpent = await _rentalRepository.GetTotalSpentAsync(userId);

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

            return Page();
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var rental = await _rentalRepository.GetByIdAsync(id);

                if (rental == null || rental.UserId != userId)
                {
                    return NotFound();
                }

                // Chỉ có thể hủy những đơn đang chờ xác nhận
                if (rental.Status != 0)
                {
                    ModelState.AddModelError("", "Chỉ có thể hủy những đơn đang chờ xác nhận.");
                    return RedirectToPage();
                }

                await _rentalRepository.CancelRentalAsync(id);
                TempData["SuccessMessage"] = "Hủy đơn thuê xe thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi hủy đơn: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}
