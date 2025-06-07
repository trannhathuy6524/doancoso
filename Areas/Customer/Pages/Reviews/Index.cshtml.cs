using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GotoCarRental.Areas.Customer.Pages.Reviews
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(IReviewRepository reviewRepository, UserManager<ApplicationUser> userManager)
        {
            _reviewRepository = reviewRepository;
            _userManager = userManager;
        }

        public IEnumerable<Review> Reviews { get; set; } = new List<Review>();

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
            Reviews = await _reviewRepository.GetByUserIdAsync(userId);
        }
    }
}
