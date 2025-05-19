using GotoCarRental.Data;
using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GotoCarRental.Areas.Customer.Pages.Features
{
    public class IndexModel : PageModel
    {
        private readonly IFeatureRepository _featureRepository;
        private readonly ICarRepository _carRepository;
        private readonly ApplicationDbContext _context;

        public IndexModel(IFeatureRepository featureRepository, ICarRepository carRepository, ApplicationDbContext context)
        {
            _featureRepository = featureRepository;
            _carRepository = carRepository;
            _context = context;
        }

        public IEnumerable<Feature> Features { get; set; }
        public Dictionary<int, int> FeatureCounts { get; set; }
        public IEnumerable<Feature> PopularFeatures { get; set; }

        public async Task OnGetAsync()
        {
            // Lấy tất cả tính năng
            Features = await _featureRepository.GetAllAsync();

            // Đếm số lượng xe cho mỗi tính năng
            FeatureCounts = new Dictionary<int, int>();
            foreach (var feature in Features)
            {
                var count = await _context.CarFeatures
                    .Where(cf => cf.FeatureId == feature.Id)
                    .CountAsync();
                FeatureCounts[feature.Id] = count;
            }

            // Lấy các tính năng phổ biến nhất (dựa trên số lượng xe có tính năng đó)
            PopularFeatures = Features
                .OrderByDescending(f => FeatureCounts.ContainsKey(f.Id) ? FeatureCounts[f.Id] : 0)
                .Take(8)
                .ToList();
        }
    }
}
