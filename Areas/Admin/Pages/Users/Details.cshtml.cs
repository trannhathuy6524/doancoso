using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GotoCarRental.Areas.Admin.Pages.Users
{
    [Authorize(Roles = "Admin,Manager")]
    public class DetailsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICarRepository _carRepository;

        public DetailsModel(UserManager<ApplicationUser> userManager, ICarRepository carRepository)
        {
            _userManager = userManager;
            _carRepository = carRepository;
        }

        public ApplicationUser AppUser { get; set; }
        public List<Car> UserCars { get; set; }
        public IEnumerable<string> UserRoles { get; set; }
        public int TotalRentals { get; set; }
        public int TotalReviews { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            // Lấy thông tin người dùng bao gồm LockoutEnd
            AppUser = await _userManager.Users
                .Include(u => u.Rentals)
                .Include(u => u.Reviews)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (AppUser == null)
            {
                return NotFound();
            }

            // Lấy danh sách xe của người dùng
            UserCars = (await _carRepository.GetCarsByUserAsync(id)).ToList();

            // Lấy vai trò của người dùng
            UserRoles = await _userManager.GetRolesAsync(AppUser);

            // Đếm số lượng đơn thuê và đánh giá
            TotalRentals = AppUser.Rentals?.Count() ?? 0;
            TotalReviews = AppUser.Reviews?.Count() ?? 0;

            return Page();
        }


        public async Task<IActionResult> OnPostToggleApprovalAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsApproved = !user.IsApproved;
            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = user.IsApproved
                ? "Người dùng đã được phê duyệt thành công!"
                : "Người dùng đã bị hủy phê duyệt thành công!";

            return RedirectToPage(new { id });
        }


        public async Task<IActionResult> OnPostActivateUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Kích hoạt lại người dùng bằng cách xóa lockoutEnd
            await _userManager.SetLockoutEndDateAsync(user, null);

            TempData["SuccessMessage"] = $"Người dùng {user.Email} đã được kích hoạt lại thành công!";
            return RedirectToPage(new { id });
        }

    }
}
