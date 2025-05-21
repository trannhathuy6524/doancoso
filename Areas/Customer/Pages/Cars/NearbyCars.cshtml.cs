using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GotoCarRental.Areas.Customer.Pages.Cars
{
    public class NearbyCarsModel : PageModel
    {
        private readonly ICarRepository _carRepository;

        public NearbyCarsModel(ICarRepository carRepository)
        {
            _carRepository = carRepository;
        }

        [BindProperty(SupportsGet = true)]
        public double? Lat { get; set; }

        [BindProperty(SupportsGet = true)]
        public double? Lng { get; set; }

        [BindProperty(SupportsGet = true)]
        public double MaxDistance { get; set; } = 10; // Mặc định 10km

        public IEnumerable<Car> NearbyCars { get; set; } = new List<Car>();

        public async Task<IActionResult> OnGetAsync()
        {
            if (Lat.HasValue && Lng.HasValue)
            {
                NearbyCars = await _carRepository.GetNearbyCarsByLocationAsync(
                    Lat.Value, Lng.Value, MaxDistance);
            }

            return Page();
        }
    }
}
