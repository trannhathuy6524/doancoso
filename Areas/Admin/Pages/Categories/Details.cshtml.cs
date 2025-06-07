using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GotoCarRental.Areas.Admin.Pages.Categories
{
    [Authorize(Roles = "Admin,Manager")]
    public class DetailsModel : PageModel
    {
        private readonly ICategoryRepository _categoryRepository;

        public DetailsModel(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public Category Category { get; set; }
        public IEnumerable<Car> Cars { get; set; }
        public int CarCount { get; set; }
        public int AvailableCarsCount { get; set; }
        public int PendingCarsCount { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Category = await _categoryRepository.GetByIdAsync(id);
            if (Category == null)
            {
                return NotFound();
            }

            // Lấy danh sách xe thuộc loại này
            Cars = await _categoryRepository.GetCarsByCategoryAsync(id);

            // Tính toán các thống kê
            CarCount = Cars.Count();
            AvailableCarsCount = Cars.Count(c => c.IsAvailable);
            PendingCarsCount = Cars.Count(c => !c.IsApproved);

            return Page();
        }
    }
}
