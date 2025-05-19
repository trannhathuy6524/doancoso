using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GotoCarRental.Areas.Admin.Pages
{
    [Authorize(Roles = "Admin")]
    public class MyCarsModel : PageModel
    {
        private readonly ICarRepository _carRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public MyCarsModel(
            ICarRepository carRepository,
            UserManager<ApplicationUser> userManager)
        {
            _carRepository = carRepository;
            _userManager = userManager;
        }

        public IEnumerable<Car> Cars { get; set; }
        public int PendingApprovalCount { get; set; }
        public int ApprovedCount { get; set; }
        public int AvailableCount { get; set; }
        public int TotalRentals { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Lấy danh sách xe của người dùng hiện tại
            Cars = await _carRepository.GetCarsByUserAsync(user.Id);

            // Tính toán số liệu thống kê
            PendingApprovalCount = Cars.Count(c => !c.IsApproved);
            ApprovedCount = Cars.Count(c => c.IsApproved);
            AvailableCount = Cars.Count(c => c.IsApproved && c.IsAvailable);
            TotalRentals = Cars.Sum(c => c.Rentals?.Count ?? 0);

            return Page();
        }

        public async Task<IActionResult> OnPostToggleAvailabilityAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var car = await _carRepository.GetByIdAsync(id);
            if (car == null || car.UserId != user.Id)
            {
                return NotFound();
            }

            // Thay đổi trạng thái sẵn sàng của xe
            await _carRepository.ToggleAvailabilityAsync(id);

            TempData["SuccessMessage"] = $"Đã {(car.IsAvailable ? "tắt" : "bật")} trạng thái sẵn sàng cho xe {car.Name}";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var car = await _carRepository.GetByIdAsync(id);
            if (car == null || car.UserId != user.Id)
            {
                return NotFound();
            }

            // Xóa xe
            await _carRepository.DeleteAsync(id);

            TempData["SuccessMessage"] = $"Xe {car.Name} đã được xóa thành công";
            return RedirectToPage();
        }
    }
}
