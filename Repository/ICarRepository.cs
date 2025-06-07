using GotoCarRental.Models;
using System.Drawing;
using System.Linq.Expressions;

namespace GotoCarRental.Repository
{
    /// <summary>
    /// Interface định nghĩa các thao tác repository cho thực thể Car
    /// </summary>
    public interface ICarRepository
    {
        #region Phương thức CRUD cơ bản

        /// <summary>
        /// Lấy thông tin xe theo ID bao gồm tất cả thông tin liên quan
        /// </summary>
        Task<Car> GetByIdAsync(int id);

        /// <summary>
        /// Lấy thông tin xe theo ID bao gồm cả xe đã xóa mềm
        /// </summary>
        Task<Car> GetByIdWithDeletedAsync(int id);

        /// <summary>
        /// Lấy tất cả xe không bị đánh dấu xóa
        /// </summary>
        Task<IEnumerable<Car>> GetAllAsync();

        /// <summary>
        /// Thêm xe mới vào hệ thống
        /// </summary>
        Task<Car> AddAsync(Car car);

        /// <summary>
        /// Cập nhật thông tin xe
        /// </summary>
        Task UpdateAsync(Car car);

        /// <summary>
        /// Xóa xe khỏi hệ thống (xóa hoàn toàn)
        /// </summary>
        Task DeleteAsync(int id);

        /// <summary>
        /// Xóa xe an toàn (xóa mềm hoặc xóa hoàn toàn tùy theo tình trạng xe)
        /// </summary>
        Task<bool> DeleteSafelyAsync(int id, string userId);

        /// <summary>
        /// Kiểm tra xem xe có tồn tại không
        /// </summary>
        Task<bool> ExistsAsync(int id);

        #endregion

        #region Phương thức lấy danh sách xe theo điều kiện

        /// <summary>
        /// Lấy danh sách xe khả dụng (đã được duyệt và đang hoạt động)
        /// </summary>
        Task<IEnumerable<Car>> GetAvailableCarsAsync();

        /// <summary>
        /// Lấy danh sách xe theo hãng
        /// </summary>
        Task<IEnumerable<Car>> GetCarsByBrandAsync(int brandId);

        /// <summary>
        /// Lấy danh sách xe theo loại
        /// </summary>
        Task<IEnumerable<Car>> GetCarsByCategoryAsync(int categoryId);

        /// <summary>
        /// Lấy danh sách xe theo người dùng (chủ xe)
        /// </summary>
        Task<IEnumerable<Car>> GetCarsByUserAsync(string userId);

        /// <summary>
        /// Lấy danh sách xe đã được duyệt và đang khả dụng
        /// </summary>
        Task<IEnumerable<Car>> GetApprovedCarsAsync();

        /// <summary>
        /// Lấy danh sách xe chưa được duyệt
        /// </summary>
        Task<IEnumerable<Car>> GetUnapprovedCarsAsync();

        /// <summary>
        /// Lấy danh sách xe gần vị trí người dùng
        /// </summary>
        Task<IEnumerable<Car>> GetNearbyCarsByLocationAsync(double latitude, double longitude, double maxDistance, int pageNumber = 1, int pageSize = 20);

        /// <summary>
        /// Lấy danh sách xe tương tự dựa trên tính năng
        /// </summary>
        Task<IEnumerable<Car>> GetRelatedCarsAsync(int carId, int count);

        #endregion

        #region Phương thức tìm kiếm và lọc

        /// <summary>
        /// Tìm kiếm xe theo điều kiện tùy chỉnh, loại bỏ các xe bị xóa mềm
        /// </summary>
        Task<IEnumerable<Car>> FindAsync(Expression<Func<Car, bool>> predicate);

        /// <summary>
        /// Tìm kiếm xe theo từ khóa
        /// </summary>
        Task<IEnumerable<Car>> SearchCarsAsync(string searchTerm);

        /// <summary>
        /// Lọc xe theo các tiêu chí
        /// </summary>
        Task<IEnumerable<Car>> FilterCarsAsync(decimal? minPrice, decimal? maxPrice, int? brandId, int? categoryId);

        /// <summary>
        /// Lấy danh sách xe với phân trang và nhiều tiêu chí lọc
        /// </summary>
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

        /// <summary>
        /// Lấy danh sách xe theo tính năng với phân trang
        /// </summary>
        Task<(IEnumerable<Car> Cars, int TotalCount)> GetCarsByFeatureAsync(
            int featureId,
            int pageNumber,
            int pageSize,
            int? brandId = null,
            string sortBy = "newest");



        #endregion

        #region Phương thức quản lý tính năng xe

        /// <summary>
        /// Lấy danh sách tính năng của xe
        /// </summary>
        Task<IEnumerable<Feature>> GetCarFeaturesAsync(int carId);

        /// <summary>
        /// Thêm tính năng cho xe
        /// </summary>
        Task AddFeatureToCarAsync(int carId, int featureId);

        /// <summary>
        /// Xóa tính năng khỏi xe
        /// </summary>
        Task RemoveFeatureFromCarAsync(int carId, int featureId);

        /// <summary>
        /// Đếm số lượng xe có tính năng cụ thể
        /// </summary>
        Task<int> GetCarCountByFeatureAsync(int featureId);

        #endregion

        #region Phương thức quản lý hình ảnh và mô hình 3D

        /// <summary>
        /// Lấy danh sách hình ảnh của xe
        /// </summary>
        Task<IEnumerable<CarImage>> GetCarImagesAsync(int carId);

        /// <summary>
        /// Thêm hình ảnh cho xe
        /// </summary>
        Task AddCarImageAsync(int carId, CarImage image);

        /// <summary>
        /// Lấy thông tin mô hình 3D của xe
        /// </summary>
        Task<CarModel3D> GetCar3DModelAsync(int carId);

        /// <summary>
        /// Thêm mô hình 3D cho xe từ template
        /// </summary>
        Task AddCarModel3DAsync(int carId, int model3DTemplateId);

        /// <summary>
        /// Lấy danh sách các mẫu mô hình 3D
        /// </summary>
        Task<IEnumerable<Model3DTemplate>> GetModel3DTemplatesAsync(int? brandId, int? categoryId);



        #endregion

        #region Phương thức quản lý đánh giá

        /// <summary>
        /// Lấy danh sách đánh giá của xe
        /// </summary>
        Task<IEnumerable<Review>> GetCarReviewsAsync(int carId);

        /// <summary>
        /// Lấy điểm đánh giá trung bình của xe
        /// </summary>
        Task<double> GetCarAverageRatingAsync(int carId);

        #endregion

        #region Phương thức quản lý trạng thái xe

        /// <summary>
        /// Bật/tắt trạng thái khả dụng của xe
        /// </summary>
        Task ToggleAvailabilityAsync(int id);

        /// <summary>
        /// Duyệt xe
        /// </summary>
        Task ApproveCarAsync(int id);

        /// <summary>
        /// Bật/tắt trạng thái duyệt của xe
        /// </summary>
        Task ToggleApprovalAsync(int id); // Thêm phương thức mới này


        #endregion
        // Thêm các phương thức sau vào interface ICarRepository
        Task<CarImage> AddImageAsync(CarImage image);
        Task SetMainImageAsync(int carId, int imageId);
        Task DeleteImageAsync(int imageId);
    }
}
