using GotoCarRental.Models;

namespace GotoCarRental.Repository
{
    public interface ICategoryRepository
    {
        // Phương thức CRUD cơ bản
        Task<Category> GetByIdAsync(int id);
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category> AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id);

        // Phương thức tìm kiếm
        Task<IEnumerable<Category>> SearchAsync(string searchTerm);

        // Phương thức kiểm tra tồn tại
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByNameAsync(string name);

        // Phương thức liên quan đến xe
        Task<IEnumerable<Car>> GetCarsByCategoryAsync(int categoryId);
        Task<int> GetCarCountAsync(int categoryId);
    }
}
