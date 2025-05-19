using GotoCarRental.Models;

namespace GotoCarRental.Repository
{
    public interface IBrandRepository
    {
        // Phương thức CRUD cơ bản
        Task<Brand> GetByIdAsync(int id);
        Task<IEnumerable<Brand>> GetAllAsync();
        Task<Brand> AddAsync(Brand brand);
        Task UpdateAsync(Brand brand);
        Task DeleteAsync(int id);

        // Phương thức tìm kiếm
        Task<IEnumerable<Brand>> SearchAsync(string searchTerm);

        // Phương thức kiểm tra tồn tại
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByNameAsync(string name);

        // Phương thức liên quan đến xe
        Task<IEnumerable<Car>> GetCarsByBrandAsync(int brandId);
        Task<int> GetCarCountAsync(int brandId);
    }
}
