using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GotoCarRental.Areas.Admin.Pages.Brands
{
    [Authorize(Roles = "Admin,Manager")]
    public class CarsModel : PageModel
    {
        private readonly IBrandRepository _brandRepository;

        public CarsModel(IBrandRepository brandRepository)
        {
            _brandRepository = brandRepository;
        }

        public Brand Brand { get; set; }
        public IEnumerable<Car> Cars { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Brand = await _brandRepository.GetByIdAsync(id);
            if (Brand == null)
            {
                return NotFound();
            }

            // Lấy danh sách xe thuộc hãng này
            Cars = await _brandRepository.GetCarsByBrandAsync(id);

            return Page();
        }
    }
}
