using GotoCarRental.Models;

namespace GotoCarRental.Repository
{
    public interface IModel3DTemplateRepository
    {
        Task<Model3DTemplate> GetByIdAsync(int id);
        Task<IEnumerable<Model3DTemplate>> GetAllAsync();
        Task<IEnumerable<Model3DTemplate>> GetByBrandAndCategoryAsync(int? brandId, int? categoryId);
        Task<Model3DTemplate> AddAsync(Model3DTemplate model3DTemplate);
        Task UpdateAsync(Model3DTemplate model3DTemplate);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<int> CountAsync();
        Task<int> CountByBrandAsync(int brandId);
        Task<int> CountByCategoryAsync(int categoryId);
    }
}
