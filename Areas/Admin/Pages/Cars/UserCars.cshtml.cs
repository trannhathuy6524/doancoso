using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GotoCarRental.Areas.Admin.Pages.Cars
{
    [Authorize(Roles = "Admin,Manager")]
    public class UserCarsModel : PageModel
    {
        private readonly ICarRepository _carRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserCarsModel(
            ICarRepository carRepository,
            UserManager<ApplicationUser> userManager)
        {
            _carRepository = carRepository;
            _userManager = userManager;
        }

        public IEnumerable<Car> Cars { get; set; }
        public ApplicationUser CarOwner { get; set; }
        public string UserId { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            // Lấy thông tin người dùng trực tiếp từ UserManager
            CarOwner = await _userManager.FindByIdAsync(userId);
            if (CarOwner == null)
            {
                return NotFound("Không tìm thấy chủ xe.");
            }

            UserId = userId;
            Cars = await _carRepository.GetCarsByUserAsync(userId);

            if (!Cars.Any())
            {
                // Không cần chuyển về trang NotFound nếu không có xe, chỉ hiển thị thông báo trống
                TempData["InfoMessage"] = "Chủ xe này chưa có xe nào.";
            }

            return Page();
        }
    }
}
