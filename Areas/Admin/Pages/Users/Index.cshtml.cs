using GotoCarRental.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GotoCarRental.Areas.Admin.Pages.Users
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public IndexModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public class UserViewModel
        {
            public string Id { get; set; }
            public string Email { get; set; }
            public string FullName { get; set; }
            public string PhoneNumber { get; set; }
            public string UserType { get; set; }
            public bool IsApproved { get; set; }
            public DateTime CreatedAt { get; set; }
            public List<string> Roles { get; set; }
        }

        public List<UserViewModel> Users { get; set; }
        public List<string> AvailableRoles { get; set; }

        public async Task OnGetAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            Users = new List<UserViewModel>();
            AvailableRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                Users.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    UserType = user.UserType,
                    IsApproved = user.IsApproved,
                    CreatedAt = user.CreatedAt,
                    Roles = roles.ToList()
                });
            }
        }

        public async Task<IActionResult> OnPostApproveUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsApproved = true;
            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = "Người dùng đã được phê duyệt thành công!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleRoleAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var isInRole = await _userManager.IsInRoleAsync(user, roleName);

            if (isInRole)
            {
                await _userManager.RemoveFromRoleAsync(user, roleName);
                TempData["SuccessMessage"] = $"Đã xóa quyền {roleName} của người dùng!";
            }
            else
            {
                await _userManager.AddToRoleAsync(user, roleName);
                TempData["SuccessMessage"] = $"Đã thêm quyền {roleName} cho người dùng!";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDisableUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Không cho phép vô hiệu hóa người dùng Admin hiện tại
            if (user.UserType == "Admin" && User.Identity.Name == user.UserName)
            {
                TempData["ErrorMessage"] = "Bạn không thể vô hiệu hóa tài khoản Admin của chính mình!";
                return RedirectToPage();
            }

            // Vô hiệu hóa người dùng bằng cách đặt lockoutEnabled và lockoutEnd
            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100)); // Khóa trong thời gian dài

            TempData["SuccessMessage"] = $"Người dùng {user.Email} đã bị vô hiệu hóa thành công!";
            return RedirectToPage();
        }

    }
}
