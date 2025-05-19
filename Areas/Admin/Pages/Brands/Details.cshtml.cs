using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GotoCarRental.Areas.Admin.Pages.Brands
{
    [Authorize(Roles = "Admin,Manager")]
    public class DetailsModel : PageModel
    {
        private readonly IBrandRepository _brandRepository;

        public DetailsModel(IBrandRepository brandRepository)
        {
            _brandRepository = brandRepository;
        }

        public Brand Brand { get; set; }
        public IEnumerable<Car> Cars { get; set; }
        public int CarCount { get; set; }
        public int AvailableCarsCount { get; set; }
        public int PendingCarsCount { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Brand = await _brandRepository.GetByIdAsync(id);
            if (Brand == null)
            {
                return NotFound();
            }

            // Lấy danh sách xe thuộc hãng này (đã được chấp thuận)
            Cars = await _brandRepository.GetCarsByBrandAsync(id);

            // Tính toán các thống kê
            CarCount = Cars.Count();
            AvailableCarsCount = Cars.Count(c => c.IsAvailable);
            PendingCarsCount = Cars.Count(c => !c.IsApproved);

            return Page();
        }
    }
}
