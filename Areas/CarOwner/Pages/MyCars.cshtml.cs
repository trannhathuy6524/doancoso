using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace GotoCarRental.Areas.CarOwner.Pages
{
    [Authorize(Roles = "CarOwner")]
    public class MyCarsModel : PageModel
    {
        private readonly ICarRepository _carRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<MyCarsModel> _logger;

        public MyCarsModel(
            ICarRepository carRepository,
            UserManager<ApplicationUser> userManager,
            ILogger<MyCarsModel> logger)
        {
            _carRepository = carRepository;
            _userManager = userManager;
            _logger = logger;
        }

        public IEnumerable<Car> Cars { get; set; }
        public int PendingApprovalCount { get; set; }
        public int ApprovedCount { get; set; }
        public int AvailableCount { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Cars = await _carRepository.GetCarsByUserAsync(userId);

            // Tính toán số liệu thống kê
            PendingApprovalCount = Cars.Count(c => !c.IsApproved);
            ApprovedCount = Cars.Count(c => c.IsApproved);
            AvailableCount = Cars.Count(c => c.IsApproved && c.IsAvailable);

            return Page();
        }

        public async Task<IActionResult> OnPostToggleAvailabilityAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var car = await _carRepository.GetByIdAsync(id);

            if (car == null)
                return NotFound();

            // Kiểm tra quyền sở hữu
            if (car.UserId != userId)
                return Forbid();

            await _carRepository.ToggleAvailabilityAsync(id);

            string status = car.IsAvailable ? "không sẵn sàng" : "sẵn sàng";
            TempData["SuccessMessage"] = $"Đã chuyển trạng thái xe {car.Name} thành {status}";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var car = await _carRepository.GetByIdAsync(id);

            if (car == null)
                return NotFound();

            // Kiểm tra quyền sở hữu
            if (car.UserId != userId)
                return Forbid();

            try
            {
                // Sử dụng DeleteSafelyAsync thay vì DeleteAsync
                var result = await _carRepository.DeleteSafelyAsync(id, userId);

                if (result)
                {
                    // Kiểm tra nếu xe vẫn tồn tại nhưng đã được đánh dấu xóa (soft delete)
                    var checkCar = await _carRepository.GetByIdWithDeletedAsync(id);
                    if (checkCar != null && checkCar.IsDeleted)
                    {
                        TempData["WarningMessage"] = "Xe đã được đánh dấu là đã xóa nhưng không thể xóa hoàn toàn do đã có dữ liệu liên quan.";
                    }
                    else
                    {
                        TempData["SuccessMessage"] = $"Xe {car.Name} đã được xóa thành công";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể xóa xe. Vui lòng liên hệ quản trị viên để được hỗ trợ.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa xe ID {Id}", id);
                TempData["ErrorMessage"] = $"Không thể xóa xe: {ex.Message}";
            }

            return RedirectToPage();
        }

    }
}
