using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GotoCarRental.Areas.Admin.Pages.Categories
{
    [Authorize(Roles = "Admin,Manager")]
    public class CarsModel : PageModel
    {
        private readonly ICategoryRepository _categoryRepository;

        public CarsModel(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public Category Category { get; set; }
        public IEnumerable<Car> Cars { get; set; }
        public int TotalCars => Cars.Count();
        public int ApprovedCars => Cars.Count(c => c.IsApproved);
        public int PendingCars => Cars.Count(c => !c.IsApproved);
        public int AvailableCars => Cars.Count(c => c.IsAvailable && c.IsApproved);

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Category = await _categoryRepository.GetByIdAsync(id);
            if (Category == null)
            {
                return NotFound();
            }

            // Lấy danh sách xe thuộc loại này
            Cars = await _categoryRepository.GetCarsByCategoryAsync(id);

            return Page();
        }
    }
}
