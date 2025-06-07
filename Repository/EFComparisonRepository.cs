using GotoCarRental.Data;
using GotoCarRental.Models;
using Microsoft.EntityFrameworkCore;

namespace GotoCarRental.Repository
{
    public class EFComparisonRepository : IComparisonRepository
    {
        private readonly ApplicationDbContext _context;

        public EFComparisonRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Car>> GetCarsForComparisonAsync(List<int> carIds)
        {
            return await _context.Cars
                .Where(c => carIds.Contains(c.Id) && !c.IsDeleted)
                .Include(c => c.Brand)
                .Include(c => c.Category)
                .Include(c => c.CarImages)
                .Include(c => c.Province)
                .Include(c => c.CarFeatures)
                    .ThenInclude(cf => cf.Feature)
                .ToListAsync();
        }

        public async Task<IEnumerable<Feature>> GetAllFeaturesForCarsAsync(List<int> carIds)
        {
            // Lấy tất cả các tính năng có trong bất kỳ xe nào được chọn
            var featureIds = await _context.CarFeatures
                .Where(cf => carIds.Contains(cf.CarId))
                .Select(cf => cf.FeatureId)
                .Distinct()
                .ToListAsync();

            return await _context.Features
                .Where(f => featureIds.Contains(f.Id))
                .ToListAsync();
        }
    }
}
