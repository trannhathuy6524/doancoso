using GotoCarRental.Data;
using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GotoCarRental.Areas.Customer.Pages.Features
{
    public class CarsModel : PageModel
    {
        private readonly IFeatureRepository _featureRepository;
        private readonly ICarRepository _carRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly ApplicationDbContext _context;

        public CarsModel(
            IFeatureRepository featureRepository,
            ICarRepository carRepository,
            IBrandRepository brandRepository,
            ApplicationDbContext context)
        {
            _featureRepository = featureRepository;
            _carRepository = carRepository;
            _brandRepository = brandRepository;
            _context = context;
        }

        public Feature Feature { get; set; }
        public IEnumerable<Car> Cars { get; set; }
        public int TotalCars { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 9;
        public int TotalPages { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "newest";

        [BindProperty(SupportsGet = true)]
        public int? BrandId { get; set; }

        public IEnumerable<Brand> Brands { get; set; }
        public IEnumerable<Feature> RelatedFeatures { get; set; }
        public Dictionary<int, int> FeatureCounts { get; set; } = new Dictionary<int, int>();

        public async Task<IActionResult> OnGetAsync(int? id, int? page)
        {
            if (id == null)
                return NotFound();

            // Lấy thông tin tính năng
            Feature = await _featureRepository.GetByIdAsync(id.Value);
            if (Feature == null)
                return NotFound();

            // Thiết lập trang hiện tại
            CurrentPage = page ?? 1;

            // Lấy danh sách xe có tính năng này, với phân trang và lọc
            var result = await _carRepository.GetCarsByFeatureAsync(id.Value, CurrentPage, PageSize, BrandId, SortBy);
            Cars = result.Cars;
            TotalCars = result.TotalCount;
            TotalPages = (int)Math.Ceiling(TotalCars / (double)PageSize);

            // Lấy danh sách hãng xe cho bộ lọc
            Brands = await _brandRepository.GetAllAsync();

            // Lấy các tính năng liên quan
            var allFeatures = await _featureRepository.GetAllAsync();

            // Đếm số lượng xe cho từng tính năng
            foreach (var feature in allFeatures)
            {
                var count = await _context.CarFeatures
                    .Where(cf => cf.FeatureId == feature.Id)
                    .CountAsync();
                FeatureCounts[feature.Id] = count;
            }

            // Lấy top tính năng phổ biến, loại trừ tính năng hiện tại
            RelatedFeatures = allFeatures
                .Where(f => f.Id != id)
                .OrderByDescending(f => FeatureCounts.ContainsKey(f.Id) ? FeatureCounts[f.Id] : 0)
                .Take(5);

            return Page();
        }
    }
}
