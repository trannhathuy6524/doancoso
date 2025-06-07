using GotoCarRental.Models;
using GotoCarRental.Repository;
using GotoCarRental.Services;
using Microsoft.AspNetCore.Mvc;

namespace GotoCarRental.ViewComponents
{
    public class CarComparisonViewComponent : ViewComponent
    {
        private readonly ICarRepository _carRepository;
        private readonly IComparisonService _comparisonService;

        public CarComparisonViewComponent(ICarRepository carRepository, IComparisonService comparisonService)
        {
            _carRepository = carRepository;
            _comparisonService = comparisonService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var carIds = _comparisonService.GetComparisonList();
            List<Car> cars = new List<Car>();

            if (carIds.Any())
            {
                foreach (var id in carIds)
                {
                    var car = await _carRepository.GetByIdAsync(id);
                    if (car != null)
                    {
                        cars.Add(car);
                    }
                }
            }

            return View(cars);
        }
    }
}
