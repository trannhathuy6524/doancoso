using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Scripting;

namespace GotoCarRental.Areas.Customer.Pages.Cars
{
    public class SearchModel : PageModel
    {
        private readonly ICarRepository _carRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IFeatureRepository _featureRepository;

        public SearchModel(
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

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MinPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MaxPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? BrandId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public List<int> FeatureIds { get; set; } = new List<int>();

        // Thêm thuộc tính mới để xử lý trường hợp truyền chuỗi từ phân trang
        [BindProperty(SupportsGet = true)]
        public string FeatureIdsString { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "newest";

        public SelectList BrandSelectList { get; set; }
        public SelectList CategorySelectList { get; set; }
        public MultiSelectList FeaturesSelectList { get; set; }
        public SelectList SortSelectList { get; set; }

        public IEnumerable<Car> Cars { get; set; } = new List<Car>();
        public int TotalCars { get; set; }
        public int TotalPages { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 9;

        public async Task OnGetAsync()
        {
            // Xử lý FeatureIdsString nếu có
            if (!string.IsNullOrEmpty(FeatureIdsString))
            {
                var idStrings = FeatureIdsString.Split(',');
                foreach (var idString in idStrings)
                {
                    if (int.TryParse(idString, out int id))
                    {
                        if (!FeatureIds.Contains(id))
                        {
                            FeatureIds.Add(id);
                        }
                    }
                }
            }

            await LoadFilterOptions();

            // Cải thiện cách lấy dữ liệu để tối ưu hơn
            // Lọc theo tính năng sử dụng repository nếu có thể
            if (FeatureIds != null && FeatureIds.Any())
            {
                var allCars = new List<Car>();
                int totalCount = 0;

                // Lấy xe từ tính năng đầu tiên
                var firstFeatureId = FeatureIds[0];
                var result = await _carRepository.GetCarsByFeatureAsync(
                    firstFeatureId,
                    CurrentPage,
                    PageSize,
                    BrandId,
                    SortBy);

                allCars.AddRange(result.Cars);
                totalCount = result.TotalCount;

                // Lọc thêm theo các tính năng còn lại
                var filteredCars = allCars;
                if (FeatureIds.Count > 1)
                {
                    filteredCars = allCars.Where(car =>
                        FeatureIds.Skip(1).All(featureId =>
                            car.CarFeatures != null &&
                            car.CarFeatures.Any(cf => cf.FeatureId == featureId))
                    ).ToList();
                }

                // Tiếp tục lọc theo các tiêu chí khác
                filteredCars = filteredCars.Where(c =>
                    (string.IsNullOrEmpty(SearchTerm) ||
                     c.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                     (c.Description != null && c.Description.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                     (c.Brand != null && c.Brand.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                     (c.Category != null && c.Category.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))) &&
                    (!MinPrice.HasValue || c.PricePerDay >= MinPrice) &&
                    (!MaxPrice.HasValue || MaxPrice.Value == 0 || c.PricePerDay <= MaxPrice) &&
                    (!CategoryId.HasValue || c.CategoryId == CategoryId)
                ).ToList();

                Cars = filteredCars;
                TotalCars = filteredCars.Count;
            }
            else
            {
                // Lọc thông thường nếu không có tính năng được chọn
                var searchResult = await _carRepository.GetCarsPagedAsync(
                    CurrentPage,
                    PageSize,
                    SearchTerm,
                    MinPrice,
                    MaxPrice,
                    BrandId,
                    CategoryId,
                    true, // Chỉ lấy xe có sẵn
                    SortBy);

                Cars = searchResult.Cars;
                TotalCars = searchResult.TotalCount;
            }

            TotalPages = (int)Math.Ceiling(TotalCars / (double)PageSize);
        }


        private async Task LoadFilterOptions()
        {
            // Load brands
            var brands = await _brandRepository.GetAllAsync();
            BrandSelectList = new SelectList(brands, "Id", "Name", BrandId);

            // Load categories
            var categories = await _categoryRepository.GetAllAsync();
            CategorySelectList = new SelectList(categories, "Id", "Name", CategoryId);

            // Load features
            var features = await _featureRepository.GetAllAsync();
            FeaturesSelectList = new MultiSelectList(features, "Id", "Name", FeatureIds);

            // Sort options
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

