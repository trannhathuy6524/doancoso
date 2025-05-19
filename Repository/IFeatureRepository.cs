using GotoCarRental.Models;

namespace GotoCarRental.Repository
{
    public interface IFeatureRepository
    {
        Task<Feature> GetByIdAsync(int id);
        Task<IEnumerable<Feature>> GetAllAsync();
        Task<Feature> AddAsync(Feature feature);
        Task UpdateAsync(Feature feature);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
