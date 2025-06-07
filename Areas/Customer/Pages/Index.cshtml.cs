using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GotoCarRental.Areas.Customer.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ICarRepository _carRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IFeatureRepository _featureRepository;
        private readonly IReviewRepository _reviewRepository;

        public IndexModel(
            ICarRepository carRepository,
            IBrandRepository brandRepository,
            ICategoryRepository categoryRepository,
            IFeatureRepository featureRepository,
            IReviewRepository reviewRepository)
        {
            _carRepository = carRepository;
            _brandRepository = brandRepository;
            _categoryRepository = categoryRepository;
            _featureRepository = featureRepository;
            _reviewRepository = reviewRepository;
        }

        public IEnumerable<Car> FeaturedCars { get; set; }
        public IEnumerable<Car> NewCars { get; set; }
        public IEnumerable<Brand> PopularBrands { get; set; }
        public IEnumerable<Category> Categories { get; set; }
        public IEnumerable<Feature> PopularFeatures { get; set; }
        public IEnumerable<Car> TopRatedCars { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Lấy xe nổi bật (có thể là xe được đánh giá cao nhất)
            var featuredCars = await _carRepository.GetCarsPagedAsync(1, 4, null, null, null, null, null, true);
            FeaturedCars = featuredCars.Cars;

            // Lấy xe mới nhất
            var newCarsResult = await _carRepository.GetCarsPagedAsync(
                1, 8,
                null, null, null, null, null, true,
                "newest");  // Thêm tham số sortBy trong repository
            NewCars = newCarsResult.Cars;

            // Lấy danh sách hãng xe phổ biến
            PopularBrands = await _brandRepository.GetAllAsync();

            // Lấy danh mục xe
            Categories = await _categoryRepository.GetAllAsync();

            // Lấy tính năng phổ biến
            PopularFeatures = await _featureRepository.GetAllAsync();

            // Lấy xe có đánh giá cao
            var allCars = await _carRepository.GetApprovedCarsAsync();
            TopRatedCars = allCars
                .OrderByDescending(c => c.Reviews.Any() ? c.Reviews.Average(r => r.Rating) : 0)
                .Take(4)
                .ToList();

            return Page();
        }
    }
}
