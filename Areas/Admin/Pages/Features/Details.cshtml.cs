using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GotoCarRental.Areas.Admin.Pages.Features
{
    [Authorize(Roles = "Admin,Manager")]
    public class DetailsModel : PageModel
    {
        private readonly IFeatureRepository _featureRepository;
        private readonly ICarRepository _carRepository;

        public DetailsModel(IFeatureRepository featureRepository, ICarRepository carRepository)
        {
            _featureRepository = featureRepository;
            _carRepository = carRepository;
        }

        public Feature Feature { get; set; }
        public int CarsCount { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Feature = await _featureRepository.GetByIdAsync(id.Value);

            if (Feature == null)
            {
                return NotFound();
            }

            // Lấy số lượng xe có tính năng này - cần thêm phương thức này vào ICarRepository
            CarsCount = await _carRepository.GetCarCountByFeatureAsync(id.Value);

            return Page();
        }
    }
}
