using GotoCarRental.Models;

namespace GotoCarRental.Repository
{
    public interface IComparisonRepository
    {
        /// <summary>
        /// Lấy danh sách xe để so sánh theo danh sách ID
        /// </summary>
        Task<IEnumerable<Car>> GetCarsForComparisonAsync(List<int> carIds);

        /// <summary>
        /// Lấy danh sách tất cả tính năng của các xe được chọn
        /// </summary>
        Task<IEnumerable<Feature>> GetAllFeaturesForCarsAsync(List<int> carIds);
    }
}
