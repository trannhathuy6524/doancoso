using GotoCarRental.Models;

namespace GotoCarRental.Repository
{
    public interface IProvinceRepository
    {
        Task<IEnumerable<Province>> GetAllAsync();
        Task<Province> GetByIdAsync(int id);
    }
}
