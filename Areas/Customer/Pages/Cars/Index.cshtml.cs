using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GotoCarRental.Areas.Customer.Pages.Cars
{
    [AllowAnonymous]
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

        // Thêm thuộc tính mới cho tìm kiếm theo loại dịch vụ
        [BindProperty(SupportsGet = true)]
        public string ServiceType { get; set; }

        // Thêm thuộc tính mới cho tìm kiếm theo hình thức thuê
        [BindProperty(SupportsGet = true)]
        public string RentalType { get; set; }

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

            // Lấy danh sách xe theo các điều kiện cơ bản
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

            // Lọc theo dịch vụ: tự lái hoặc có tài xế
            if (!string.IsNullOrEmpty(ServiceType))
            {
                if (ServiceType == "WithDriver")
                {
                    // Chỉ hiển thị xe có hỗ trợ dịch vụ tài xế
                    result.Cars = result.Cars.Where(c => c.OfferDriverService).ToList();
                }
                else if (ServiceType == "SelfDrive")
                {
                    // Hiển thị tất cả xe (cả xe có tài xế và không có tài xế đều có thể tự lái)
                    // Không cần lọc vì mặc định tất cả xe đều có thể tự lái
                }
            }

            // Lọc theo hình thức thuê: theo ngày hoặc theo giờ
            if (!string.IsNullOrEmpty(RentalType))
            {
                if (RentalType == "ByHour")
                {
                    // Chỉ hiển thị xe có hỗ trợ thuê theo giờ (PricePerHour > 0)
                    result.Cars = result.Cars.Where(c => c.PricePerHour > 0).ToList();
                }
                // Không cần lọc cho ByDay vì mặc định tất cả xe đều có thể thuê theo ngày
            }

            // Cập nhật lại tổng số xe sau khi lọc thêm
            Cars = result.Cars;
            TotalCars = Cars.Count();

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