using GotoCarRental.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GotoCarRental.Areas.CarOwner.Pages.Cars
{
    [Authorize(Roles = "CarOwner")]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public IList<Car> Cars { get; set; } = new List<Car>();

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Lấy danh sách xe của chủ xe hiện tại
            Cars = await _userManager.Users
                .Where(u => u.Id == user.Id)
                .SelectMany(u => u.Cars)
                .Include(c => c.Brand)
                .Include(c => c.Category)
                .Include(c => c.CarImages)
                .ToListAsync();

            return Page();
        }
    }
}
