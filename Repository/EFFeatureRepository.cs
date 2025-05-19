using GotoCarRental.Data;
using GotoCarRental.Models;
using Microsoft.EntityFrameworkCore;

namespace GotoCarRental.Repository
{
    public class EFFeatureRepository : IFeatureRepository
    {
        private readonly ApplicationDbContext _context;

        public EFFeatureRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Feature> GetByIdAsync(int id)
        {
            return await _context.Features.FindAsync(id);
        }

        public async Task<IEnumerable<Feature>> GetAllAsync()
        {
            return await _context.Features.ToListAsync();
        }

        public async Task<Feature> AddAsync(Feature feature)
        {
            // Bảo vệ khỏi null
            if (feature.CarFeatures == null)
                feature.CarFeatures = new List<CarFeature>();

            await _context.Features.AddAsync(feature);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving feature: {ex.Message}", ex);
            }

            return feature;
        }


        public async Task UpdateAsync(Feature feature)
        {
            _context.Features.Update(feature);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var feature = await _context.Features.FindAsync(id);
            if (feature != null)
            {
                _context.Features.Remove(feature);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Features.AnyAsync(f => f.Id == id);
        }
    }
}
