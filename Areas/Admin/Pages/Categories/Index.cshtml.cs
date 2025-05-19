using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GotoCarRental.Areas.Admin.Pages.Categories
{
    [Authorize(Roles = "Admin,Manager")]
    public class IndexModel : PageModel
    {
        private readonly ICategoryRepository _categoryRepository;

        public IndexModel(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public IEnumerable<Category> Categories { get; set; }
        public Dictionary<int, int> CategoryStats { get; set; }

        public async Task OnGetAsync()
        {
            Categories = await _categoryRepository.GetAllAsync();

            // Get car count for each category
            CategoryStats = new Dictionary<int, int>();
            foreach (var category in Categories)
            {
                CategoryStats[category.Id] = await _categoryRepository.GetCarCountAsync(category.Id);
            }
        }
    }
}
