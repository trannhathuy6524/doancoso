using GotoCarRental.Models;
using System.Linq.Expressions;

namespace GotoCarRental.Repository
{
    public interface ICarRepository
    {
        // Phương thức cơ bản CRUD
        Task<Car> GetByIdAsync(int id);
        Task<IEnumerable<Car>> GetAllAsync();
        Task<Car> AddAsync(Car car);
        Task UpdateAsync(Car car);
        Task DeleteAsync(int id);

        // Phương thức tìm kiếm nâng cao
        Task<IEnumerable<Car>> FindAsync(Expression<Func<Car, bool>> predicate);

        // Phương thức lấy danh sách xe theo điều kiện phổ biến
        Task<IEnumerable<Car>> GetAvailableCarsAsync();
        Task<IEnumerable<Car>> GetCarsByBrandAsync(int brandId);
        Task<IEnumerable<Car>> GetCarsByCategoryAsync(int categoryId);
        Task<IEnumerable<Car>> GetCarsByUserAsync(string userId);
        Task<IEnumerable<Car>> GetApprovedCarsAsync();
        Task<IEnumerable<Car>> GetUnapprovedCarsAsync();

        // Phương thức tìm kiếm và lọc
        Task<IEnumerable<Car>> SearchCarsAsync(string searchTerm);
        Task<IEnumerable<Car>> FilterCarsAsync(decimal? minPrice, decimal? maxPrice, int? brandId, int? categoryId);

        // Phương thức liên quan đến tính năng của xe
        Task<IEnumerable<Feature>> GetCarFeaturesAsync(int carId);
        Task AddFeatureToCarAsync(int carId, int featureId);
        Task RemoveFeatureFromCarAsync(int carId, int featureId);

        // Phương thức liên quan đến hình ảnh của xe
        Task<IEnumerable<CarImage>> GetCarImagesAsync(int carId);
        Task<CarModel3D> GetCar3DModelAsync(int carId);

        // Phương thức liên quan đến đánh giá của xe
        Task<IEnumerable<Review>> GetCarReviewsAsync(int carId);
        Task<double> GetCarAverageRatingAsync(int carId);

        // Cập nhật trong ICarRepository.cs
        Task<(IEnumerable<Car> Cars, int TotalCount)> GetCarsPagedAsync(
            int pageNumber,
            int pageSize,
            string searchTerm = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            int? brandId = null,
            int? categoryId = null,
            bool? isAvailable = null,
            string sortBy = "newest");

        // Phương thức kiểm tra tồn tại
        Task<bool> ExistsAsync(int id);

        // Phương thức cập nhật trạng thái
        Task ToggleAvailabilityAsync(int id);
        Task ApproveCarAsync(int id);
        Task ToggleApprovalAsync(int id); // Thêm phương thức mới này

        // Thêm vào interface ICarRepository
        Task<int> GetCarCountByFeatureAsync(int featureId);
        Task AddCarImageAsync(int carId, CarImage image);
        // Thêm vào interface ICarRepository
        Task<(IEnumerable<Car> Cars, int TotalCount)> GetCarsByFeatureAsync(
            int featureId,
            int pageNumber,
            int pageSize,
            int? brandId = null,
            string sortBy = "newest");
        Task<IEnumerable<Car>> GetRelatedCarsAsync(int carId, int count);
        Task<bool> DeleteSafelyAsync(int id, string userId);
        Task<Car> GetByIdWithDeletedAsync(int id);

        // Phương thức thêm mô hình 3D cho xe
        Task AddCarModel3DAsync(int carId, int model3DTemplateId);
        // Phương thức lấy tất cả template 3D theo hãng và loại xe
        Task<IEnumerable<Model3DTemplate>> GetModel3DTemplatesAsync(int? brandId, int? categoryId);

        Task<IEnumerable<Car>> GetNearbyCarsByLocationAsync(double latitude, double longitude, double maxDistance, int pageNumber = 1, int pageSize = 20);

    }
}
