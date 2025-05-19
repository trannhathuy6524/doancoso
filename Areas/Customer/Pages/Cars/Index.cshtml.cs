using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GotoCarRental.Areas.Customer.Pages.Cars
{
    public class IndexModel : PageModel
    {
        private readonly ICarRepository _carRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IFeatureRepository _featureRepository;

        public IndexModel(
            ICarRepository carRepository,
            IBrandRepository brandRepository,
            ICategoryRepository categoryRepository,
            IFeatureRepository featureRepository)
        {
            _carRepository = carRepository;
            _brandRepository = brandRepository;
            _categoryRepository = categoryRepository;
            _featureRepository = featureRepository;
        }

        public IEnumerable<Car> Cars { get; set; }
        public int TotalCars { get; set; }
        public int TotalPages { get; set; }
        public Dictionary<int, double> CarRatings { get; set; }


        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "newest";

        [BindProperty(SupportsGet = true)]
        public int? BrandId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string PriceRange { get; set; }

        public SelectList BrandSelectList { get; set; }
        public SelectList CategorySelectList { get; set; }
        public SelectList SortSelectList { get; set; }

        public int PageSize { get; set; } = 9;

        public async Task OnGetAsync()
        {
            await LoadDropdowns();

            decimal? minPrice = null;
            decimal? maxPrice = null;

            // Phân tích giá nếu có
            if (!string.IsNullOrEmpty(PriceRange))
            {
                var parts = PriceRange.Split('-');
                if (parts.Length == 2)
                {
                    if (decimal.TryParse(parts[0], out var min))
                        minPrice = min;

                    if (decimal.TryParse(parts[1], out var max) && max > 0)
                        maxPrice = max;
                }
            }

            // Lấy danh sách xe theo các điều kiện cơ bản (không lọc theo tính năng)
            var result = await _carRepository.GetCarsPagedAsync(
                CurrentPage,
                PageSize,
                null, // searchTerm
                minPrice,
                maxPrice,
                BrandId,
                CategoryId,
                true, // isAvailable
                SortBy);

            Cars = result.Cars;
            TotalCars = result.TotalCount;

            // Đảm bảo Cars không bị null
            if (Cars == null)
            {
                Cars = new List<Car>();
                TotalCars = 0;
            }

            // Tính toán xếp hạng trung bình cho mỗi xe
            CarRatings = new Dictionary<int, double>();
            foreach (var car in Cars)
            {
                CarRatings[car.Id] = await _carRepository.GetCarAverageRatingAsync(car.Id);
            }

            TotalPages = (int)Math.Ceiling(TotalCars / (double)PageSize);
        }

        private async Task LoadDropdowns()
        {
            var brands = await _brandRepository.GetAllAsync();
            BrandSelectList = new SelectList(brands, "Id", "Name", BrandId);

            var categories = await _categoryRepository.GetAllAsync();
            CategorySelectList = new SelectList(categories, "Id", "Name", CategoryId);

            var sortOptions = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("newest", "Mới nhất"),
                        new KeyValuePair<string, string>("price_asc", "Giá: Thấp đến cao"),
                        new KeyValuePair<string, string>("price_desc", "Giá: Cao đến thấp"),
                        new KeyValuePair<string, string>("rating", "Đánh giá cao nhất")
                    };

            SortSelectList = new SelectList(sortOptions, "Key", "Value", SortBy);
        }
    }
}
