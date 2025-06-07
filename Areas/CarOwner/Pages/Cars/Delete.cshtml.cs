using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace GotoCarRental.Areas.CarOwner.Pages.Cars
{
    [Authorize(Roles = "CarOwner")]
    public class DeleteModel : PageModel
    {
        private readonly ICarRepository _carRepository;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(
            ICarRepository carRepository,
            ILogger<DeleteModel> logger)
        {
            _carRepository = carRepository;
            _logger = logger;
        }

        [BindProperty]
        public Car Car { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Car = await _carRepository.GetByIdAsync(id.Value);
            if (Car == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền sở hữu
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != Car.UserId)
            {
                return Forbid();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var car = await _carRepository.GetByIdAsync(id.Value);

            if (car == null || car.UserId != userId)
                return NotFound();

            try
            {
                var result = await _carRepository.DeleteSafelyAsync(id.Value, userId);

                if (result)
                {
                    // Kiểm tra nếu xe vẫn tồn tại nhưng đã được đánh dấu xóa (soft delete)
                    var checkCar = await _carRepository.GetByIdWithDeletedAsync(id.Value);
                    if (checkCar != null && checkCar.IsDeleted)
                    {
                        TempData["WarningMessage"] = "Xe đã được đánh dấu là đã xóa nhưng không thể xóa hoàn toàn do đã có dữ liệu liên quan.";
                    }
                    else
                    {
                        TempData["SuccessMessage"] = "Xe đã được xóa thành công!";
                    }

                    return RedirectToPage("/MyCars");
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể xóa xe. Vui lòng liên hệ quản trị viên để được hỗ trợ.";
                    return RedirectToPage(new { id });
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                return RedirectToPage(new { id });
            }
        }
    }
}
