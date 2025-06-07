using GotoCarRental.Data;
using GotoCarRental.Models;
using GotoCarRental.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GotoCarRental.Repository
{
    /// <summary>
    /// Repository xử lý thao tác CRUD và tìm kiếm liên quan đến xe
    /// </summary
    public class EFCarRepository : ICarRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IGeoLocationService _geoLocationService;

        public EFCarRepository(ApplicationDbContext context, IGeoLocationService geoLocationService)
        {
            _context = context;
            _geoLocationService = geoLocationService;
        }

        #region Phương thức CRUD cơ bản

        /// <summary>
        ///  Lấy thông tin xe theo ID bao gồm tất cả thông tin liên quan
        /// </summary>
        public async Task<Car> GetByIdAsync(int id)
        {
            var car = await _context.Cars
                .Include(c => c.Brand)
                .Include(c => c.Category)
                .Include(c => c.User)
                .Include(c => c.CarImages)
                .Include(c => c.CarModel3D)
                .Include(c => c.Province)
                .Include(c => c.CarFeatures)
                    .ThenInclude(cf => cf.Feature)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            // Đảm bảo car.User không null
            if (car != null && car.User == null && !string.IsNullOrEmpty(car.UserId))
            {
                car.User = await _context.Users.FindAsync(car.UserId);
            }

            return car;
        }

        /// <summary>
        /// Lấy thông tin xe theo ID bao gồm cả xe đã xóa mềm
        /// </summary>
        public async Task<Car> GetByIdWithDeletedAsync(int id)
        {
            // Giống với GetByIdAsync nhưng không có điều kiện !c.IsDeleted
            return await _context.Cars
                .Include(c => c.Brand)
                .Include(c => c.Category)
                .Include(c => c.User)
                .Include(c => c.CarImages)
                .Include(c => c.CarModel3D)
                .Include(c => c.Province)
                .Include(c => c.CarFeatures)
                    .ThenInclude(cf => cf.Feature)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        /// <summary>
        /// Lấy tất cả xe không bị đánh dấu xóa
        /// </summary>
        public async Task<IEnumerable<Car>> GetAllAsync()
        {
            return await _context.Cars
                .Include(c => c.Brand)
                .Include(c => c.Category)
                .Include(c => c.User)
                .Include(c => c.CarImages)
                .Where(c => !c.IsDeleted) // Thêm điều kiện này
                .ToListAsync();
        }

        /// <summary>
        /// Thêm xe mới vào hệ thống
        /// </summary>
        public async Task<Car> AddAsync(Car car)
        {
            // Đảm bảo các collection đã được khởi tạo
            if (car.CarImages == null)
                car.CarImages = new List<CarImage>();

            if (car.CarFeatures == null)
                car.CarFeatures = new List<CarFeature>();

            if (car.Rentals == null)
                car.Rentals = new List<Rental>();

            if (car.Reviews == null)
                car.Reviews = new List<Review>();

            // THÊM DÒNG NÀY: Xử lý trường hợp UserId null
            if (string.IsNullOrEmpty(car.UserId))
            {
                throw new InvalidOperationException("UserId không thể trống khi thêm xe mới.");
            }

            car.CreatedAt = DateTime.UtcNow;
            car.UpdatedAt = DateTime.UtcNow;

            await _context.Cars.AddAsync(car);
            await _context.SaveChangesAsync();

            return car;
        }

        /// <summary>
        /// Cập nhật thông tin xe
        /// </summary>
        public async Task UpdateAsync(Car car)
        {
            // Cập nhật thời gian
            car.UpdatedAt = DateTime.UtcNow;

            // Lấy xe hiện có từ database
            var existingCar = await _context.Cars.FindAsync(car.Id);
            if (existingCar == null)
            {
                throw new InvalidOperationException($"Không tìm thấy xe với ID {car.Id}");
            }

            // Cập nhật các thuộc tính cơ bản
            existingCar.Name = car.Name;
            existingCar.Description = car.Description;
            existingCar.PricePerDay = car.PricePerDay;
            existingCar.BrandId = car.BrandId;
            existingCar.CategoryId = car.CategoryId;
            existingCar.IsAvailable = car.IsAvailable;
            existingCar.IsApproved = car.IsApproved;
            existingCar.UpdatedAt = car.UpdatedAt;
            existingCar.PricePerHour = car.PricePerHour;
            existingCar.Latitude = car.Latitude;
            existingCar.Longitude = car.Longitude;
            // Thêm 2 dòng cập nhật này
            existingCar.ProvinceId = car.ProvinceId;
            existingCar.DetailedAddress = car.DetailedAddress;

            // Thêm các thuộc tính liên quan đến dịch vụ tài xế
            existingCar.OfferDriverService = car.OfferDriverService;
            existingCar.DriverFeePerDay = car.DriverFeePerDay;
            existingCar.DriverFeePerHour = car.DriverFeePerHour;

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Xóa xe khỏi hệ thống (xóa hoàn toàn)
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car != null)
            {
                _context.Cars.Remove(car);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Xóa xe an toàn (xóa mềm hoặc xóa hoàn toàn tùy theo tình trạng xe)
        /// </summary>
        public async Task<bool> DeleteSafelyAsync(int id, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Lấy thông tin xe và kiểm tra quyền sở hữu với eager loading tất cả các mối quan hệ
                var car = await _context.Cars
                    .Include(c => c.CarImages)
                    .Include(c => c.CarFeatures)
                    .Include(c => c.Reviews)
                    .Include(c => c.CarModel3D)
                    .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

                if (car == null)
                    return false;

                // Kiểm tra xem xe có đơn thuê hay không
                var hasRentals = await _context.Rentals.AnyAsync(r => r.CarId == id);
                if (hasRentals)
                {
                    // Nếu có đơn thuê, không xóa xe mà chỉ đánh dấu là không khả dụng và đã xóa
                    car.IsAvailable = false;
                    car.IsDeleted = true;
                    car.UpdatedAt = DateTime.UtcNow;

                    // Lưu thay đổi và commit transaction
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }

                // Nếu không có đơn thuê, tiến hành xóa các liên kết trước

                // 1. Xóa model 3D nếu có
                if (car.CarModel3D != null)
                {
                    _context.CarModel3Ds.Remove(car.CarModel3D);
                    // SaveChanges sau mỗi bước để tránh lỗi
                    await _context.SaveChangesAsync();
                }

                // 2. Xóa các hình ảnh liên quan
                if (car.CarImages != null && car.CarImages.Any())
                {
                    // Xóa file hình ảnh nếu cần
                    foreach (var image in car.CarImages.ToList()) // Chuyển sang ToList để tránh lỗi collection modified
                    {
                        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.ImageUrl.TrimStart('/'));
                        if (File.Exists(imagePath))
                        {
                            try
                            {
                                File.Delete(imagePath);
                            }
                            catch (IOException)
                            {
                                // Ignore file deletion errors
                                Console.WriteLine($"Không thể xóa file: {imagePath}");
                            }
                        }

                        // Xóa từng image riêng lẻ thay vì xóa một lúc nhiều
                        _context.CarImages.Remove(image);
                    }

                    await _context.SaveChangesAsync();
                }

                // 3. Xóa các liên kết với tính năng
                if (car.CarFeatures != null && car.CarFeatures.Any())
                {
                    foreach (var feature in car.CarFeatures.ToList())
                    {
                        _context.CarFeatures.Remove(feature);
                    }

                    await _context.SaveChangesAsync();
                }

                // 4. Xóa đánh giá nếu có
                var reviews = await _context.Reviews.Where(r => r.CarId == id).ToListAsync();
                if (reviews.Any())
                {
                    foreach (var review in reviews)
                    {
                        _context.Reviews.Remove(review);
                    }

                    await _context.SaveChangesAsync();
                }

                // 5. Xóa xe
                _context.Cars.Remove(car);
                await _context.SaveChangesAsync();

                // Commit transaction khi tất cả đã xong
                await transaction.CommitAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();

                // Ghi log chi tiết
                Console.WriteLine($"DbUpdateException: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Stack trace: {ex.InnerException.StackTrace}");
                }

                // Sử dụng soft delete thay vì thông báo lỗi
                try
                {
                    var car = await _context.Cars.FindAsync(id);
                    if (car != null && car.UserId == userId)
                    {
                        car.IsDeleted = true;
                        car.IsAvailable = false;
                        car.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                        return true; // Đã soft delete thành công
                    }
                }
                catch (Exception softDeleteEx)
                {
                    Console.WriteLine($"Soft delete failed: {softDeleteEx.Message}");
                }

                return false;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                // Ghi log chi tiết
                Console.WriteLine($"Exception: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Stack trace: {ex.InnerException.StackTrace}");
                }

                return false;
            }
        }

        /// <summary>
        /// Kiểm tra xem xe có tồn tại không
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Cars.AnyAsync(c => c.Id == id);
        }

        // Phương thức helper để xóa dữ liệu liên quan đến xe
        private async Task DeleteCarRelatedData(Car car)
        {
            // 1. Xóa model 3D nếu có
            if (car.CarModel3D != null)
            {
                _context.CarModel3Ds.Remove(car.CarModel3D);
                await _context.SaveChangesAsync();
            }

            // 2. Xóa các hình ảnh liên quan
            if (car.CarImages != null && car.CarImages.Any())
            {
                // Xóa file hình ảnh nếu cần
                foreach (var image in car.CarImages.ToList())
                {
                    DeleteImageFile(image);
                    _context.CarImages.Remove(image);
                }
                await _context.SaveChangesAsync();
            }

            // 3. Xóa các liên kết với tính năng
            if (car.CarFeatures != null && car.CarFeatures.Any())
            {
                foreach (var feature in car.CarFeatures.ToList())
                {
                    _context.CarFeatures.Remove(feature);
                }
                await _context.SaveChangesAsync();
            }

            // 4. Xóa đánh giá nếu có
            var reviews = await _context.Reviews.Where(r => r.CarId == car.Id).ToListAsync();
            if (reviews.Any())
            {
                foreach (var review in reviews)
                {
                    _context.Reviews.Remove(review);
                }
                await _context.SaveChangesAsync();
            }
        }

        // Helper để xóa file hình ảnh
        private void DeleteImageFile(CarImage image)
        {
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.ImageUrl.TrimStart('/'));
            if (File.Exists(imagePath))
            {
                try
                {
                    File.Delete(imagePath);
                }
                catch (IOException)
                {
                    Console.WriteLine($"Không thể xóa file: {imagePath}");
                }
            }
        }

        #endregion

        #region Phương thức lấy danh sách xe theo điều kiện

        /// <summary>
        /// Lấy danh sách xe khả dụng (đã được duyệt và đang hoạt động)
        /// </summary>
        public async Task<IEnumerable<Car>> GetAvailableCarsAsync()
        {
            return await _context.Cars
                .Include(c => c.Brand)
                .Include(c => c.Category)
                .Include(c => c.CarImages)
                .Where(c => c.IsAvailable && c.IsApproved && !c.IsDeleted)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy danh sách xe theo hãng
        /// </summary>
        public async Task<IEnumerable<Car>> GetCarsByBrandAsync(int brandId)
        {
            return await _context.Cars
                .Include(c => c.Brand)
                .Include(c => c.Category)
                .Include(c => c.CarImages)
                .Where(c => c.BrandId == brandId && c.IsApproved && !c.IsDeleted)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy danh sách xe theo loại
        /// </summary>
        public async Task<IEnumerable<Car>> GetCarsByCategoryAsync(int categoryId)
        {
            return await _context.Cars
                .Include(c => c.Brand)
                .Include(c => c.Category)
                .Include(c => c.CarImages)
                .Where(c => c.CategoryId == categoryId && c.IsApproved)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy danh sách xe theo người dùng (chủ xe)
        /// </summary>
        public async Task<IEnumerable<Car>> GetCarsByUserAsync(string userId)
        {
            return await _context.Cars
                .Include(c => c.Brand)
                .Include(c => c.Category)
                .Include(c => c.CarImages)
                .Where(c => c.UserId == userId && !c.IsDeleted)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy danh sách xe đã được duyệt và đang khả dụng
        /// </summary>
        public async Task<IEnumerable<Car>> GetApprovedCarsAsync()
        {
            return await _context.Cars
                .Include(c => c.Brand)
                .Include(c => c.Category)
                .Include(c => c.User)
                .Include(c => c.CarImages)
                .Where(c => c.IsApproved && c.IsAvailable && !c.IsDeleted) // Thêm điều kiện !c.IsDeleted
                .ToListAsync();
        }

        /// <summary>
        /// Lấy danh sách xe chưa được duyệt
        /// </summary>
        public async Task<IEnumerable<Car>> GetUnapprovedCarsAsync()
        {
            return await _context.Cars
                .Include(c => c.Brand)
                .Include(c => c.Category)
                .Include(c => c.CarImages)
                .Where(c => !c.IsApproved && !c.IsDeleted)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy danh sách xe gần vị trí người dùng
        /// </summary>
        public async Task<IEnumerable<Car>> GetNearbyCarsByLocationAsync(double latitude, double longitude, double maxDistance, int pageNumber = 1, int pageSize = 20)
        {
            // Lấy tất cả xe đã được duyệt và đang khả dụng
            var cars = await _context.Cars
                .Include(c => c.Brand)
                .Include(c => c.Category)
                .Include(c => c.CarImages)
                .Include(c => c.Province)
                .Where(c => c.IsApproved && !c.IsDeleted && c.IsAvailable)
                .Where(c => c.Latitude != null && c.Longitude != null) // Chỉ lấy xe có tọa độ
                .ToListAsync();

            // Tính khoảng cách cho mỗi xe
            foreach (var car in cars)
            {
                car.DistanceFromUser = _geoLocationService.CalculateDistance(
                    latitude, longitude,
                    car.Latitude.Value, car.Longitude.Value);
            }

            // Lọc xe theo khoảng cách tối đa và sắp xếp theo khoảng cách gần nhất
            return cars
                .Where(c => c.DistanceFromUser <= maxDistance)
                .OrderBy(c => c.DistanceFromUser)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        /// <summary>
        /// Lấy danh sách xe tương tự dựa trên tính năng
        /// </summary>
        public async Task<IEnumerable<Car>> GetRelatedCarsAsync(int carId, int count)
        {
            var car = await _context.Cars
                .Include(c => c.CarFeatures)
                .FirstOrDefaultAsync(c => c.Id == carId && !c.IsDeleted);

            if (car == null)
            {
                return new List<Car>();
            }

            // Lấy ID các tính năng của xe hiện tại
            var featureIds = car.CarFeatures.Select(cf => cf.FeatureId).ToList();

            // Lấy xe có tính năng tương tự (bỏ qua xe hiện tại)
            var relatedCars = await _context.Cars
                .Include(c => c.Brand)
                .Include(c => c.Category)
                .Include(c => c.CarImages)
                .Include(c => c.CarFeatures)
                    .ThenInclude(cf => cf.Feature)
                .Where(c => c.Id != carId && c.IsApproved && c.IsAvailable && !c.IsDeleted)
                .Where(c => c.CarFeatures.Any(cf => featureIds.Contains(cf.FeatureId)))
                .OrderByDescending(c => c.CarFeatures.Count(cf => featureIds.Contains(cf.FeatureId)))
                .Take(count)
                .ToListAsync();

            return relatedCars;
        }



        #endregion

        #region Phương thức tìm kiếm và lọc

        /// <summary>
        /// Tìm kiếm xe theo điều kiện tùy chỉnh, loại bỏ các xe bị xóa mềm
        /// </summary>
        public async Task<IEnumerable<Car>> FindAsync(Expression<Func<Car, bool>> predicate)
        {
            // Kết hợp điều kiện !c.IsDeleted với predicate
            var parameter = Expression.Parameter(typeof(Car), "c");
            var notDeletedExpression = Expression.Equal(
                Expression.Property(parameter, "IsDeleted"),
                Expression.Constant(false));
            var combinedExpression = Expression.AndAlso(
                Expression.Invoke(predicate, parameter),
                notDeletedExpression);
            var lambda = Expression.Lambda<Func<Car, bool>>(combinedExpression, parameter);

            return await _context.Cars
                .Include(c => c.Brand)
                .Include(c => c.Category)
                .Include(c => c.CarImages)
                .Where(lambda)
                .ToListAsync();
        }

        /// <summary>
        /// Tìm kiếm xe theo từ khóa
        /// </summary>
        public async Task<IEnumerable<Car>> SearchCarsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetApprovedCarsAsync(); // GetApprovedCarsAsync đã có điều kiện !c.IsDeleted

            return await _context.Cars
                .Include(c => c.Brand)
                .Include(c => c.Category)
                .Include(c => c.CarImages)
                .Where(c => c.IsApproved && !c.IsDeleted &&
                           (c.Name.Contains(searchTerm) ||
                            c.Description.Contains(searchTerm) ||
                            c.Brand.Name.Contains(searchTerm) ||
                            c.Category.Name.Contains(searchTerm)))
                .ToListAsync();
        }

        /// <summary>
        /// Lọc xe theo các tiêu chí
        /// </summary>
        public async Task<IEnumerable<Car>> FilterCarsAsync(decimal? minPrice, decimal? maxPrice, int? brandId, int? categoryId)
        {
            var query = _context.Cars
                .Include(c => c.Brand)
                .Include(c => c.Category)
                .Include(c => c.CarImages)
                .Where(c => c.IsApproved && c.IsAvailable && !c.IsDeleted);

            if (minPrice.HasValue)
                query = query.Where(c => c.PricePerDay >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(c => c.PricePerDay <= maxPrice.Value);

            if (brandId.HasValue)
                query = query.Where(c => c.BrandId == brandId.Value);

            if (categoryId.HasValue)
                query = query.Where(c => c.CategoryId == categoryId.Value);

            return await query.ToListAsync();
        }

        /// <summary>
        /// Lấy danh sách xe với phân trang và nhiều tiêu chí lọc
        /// </summary>
        public async Task<(IEnumerable<Car> Cars, int TotalCount)> GetCarsPagedAsync(
           int pageNumber,
           int pageSize,
           string searchTerm = null,
           decimal? minPrice = null,
           decimal? maxPrice = null,
           int? brandId = null,
           int? categoryId = null,
           bool? isAvailable = null,
           string sortBy = "newest")
        {
            IQueryable<Car> query = _context.Cars
                .Include(c => c.Brand)
                .Include(c => c.Category)
                .Include(c => c.User)
                .Include(c => c.CarImages)
                .Include(c => c.Province)
                .Where(c => !c.IsDeleted); // Thêm điều kiện !c.IsDeleted

            // Áp dụng bộ lọc
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c => c.Name.Contains(searchTerm) ||
                                        c.Description.Contains(searchTerm) ||
                                        c.Brand.Name.Contains(searchTerm) ||
                                        c.Category.Name.Contains(searchTerm));
            }

            if (minPrice.HasValue)
                query = query.Where(c => c.PricePerDay >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(c => c.PricePerDay <= maxPrice.Value);

            if (brandId.HasValue)
                query = query.Where(c => c.BrandId == brandId.Value);

            if (categoryId.HasValue)
                query = query.Where(c => c.CategoryId == categoryId.Value);

            if (isAvailable.HasValue)
                query = query.Where(c => c.IsAvailable == isAvailable.Value && c.IsApproved);

            // Áp dụng sắp xếp
            switch (sortBy)
            {
                case "price_asc":
                    query = query.OrderBy(c => c.PricePerDay);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(c => c.PricePerDay);
                    break;
                case "rating":
                    query = query.OrderByDescending(c =>
                        c.Reviews.Any() ? c.Reviews.Average(r => r.Rating) : 0);
                    break;
                case "newest":
                default:
                    query = query.OrderByDescending(c => c.CreatedAt);
                    break;
            }

            // Đếm tổng số lượng
            var totalCount = await query.CountAsync();

            // Lấy kết quả phân trang
            var cars = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (cars, totalCount);
        }

        /// <summary>
        /// Lấy danh sách xe theo tính năng với phân trang
        /// </summary>
        public async Task<(IEnumerable<Car> Cars, int TotalCount)> GetCarsByFeatureAsync(
            int featureId,
            int pageNumber,
            int pageSize,
            int? brandId = null,
            string sortBy = "newest")
        {
            // Bắt đầu với truy vấn cơ bản
            var query = _context.Cars
                .Include(c => c.Brand)
                .Include(c => c.Category)
                .Include(c => c.CarImages)

                .Include(c => c.CarFeatures)
                    .ThenInclude(cf => cf.Feature)
                .Where(c => c.IsApproved && c.IsAvailable && !c.IsDeleted)
                .Where(c => c.CarFeatures.Any(cf => cf.FeatureId == featureId));

            // Lọc theo hãng xe nếu được chỉ định
            if (brandId.HasValue)
            {
                query = query.Where(c => c.BrandId == brandId);
            }

            // Sắp xếp theo tùy chọn
            switch (sortBy)
            {
                case "price_asc":
                    query = query.OrderBy(c => c.PricePerDay);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(c => c.PricePerDay);
                    break;
                case "rating":
                    query = query.OrderByDescending(c =>
                        c.Reviews.Any() ? c.Reviews.Average(r => r.Rating) : 0);
                    break;
                case "newest":
                default:
                    query = query.OrderByDescending(c => c.CreatedAt);
                    break;
            }

            // Đếm tổng số lượng
            var totalCount = await query.CountAsync();

            // Phân trang
            var cars = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (cars, totalCount);
        }


        #endregion

        #region Phương thức quản lý tính năng xe

        /// <summary>
        /// Lấy danh sách tính năng của xe
        /// </summary>
        public async Task<IEnumerable<Feature>> GetCarFeaturesAsync(int carId)
        {
            return await _context.CarFeatures
                .Where(cf => cf.CarId == carId)
                .Select(cf => cf.Feature)
                .ToListAsync();
        }

        /// <summary>
        /// Thêm tính năng cho xe
        /// </summary>
        public async Task AddFeatureToCarAsync(int carId, int featureId)
        {
            var exists = await _context.CarFeatures
                .AnyAsync(cf => cf.CarId == carId && cf.FeatureId == featureId);

            if (!exists)
            {
                _context.CarFeatures.Add(new CarFeature
                {
                    CarId = carId,
                    FeatureId = featureId
                });

                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Xóa tính năng khỏi xe
        /// </summary>
        public async Task RemoveFeatureFromCarAsync(int carId, int featureId)
        {
            var carFeature = await _context.CarFeatures
                .FirstOrDefaultAsync(cf => cf.CarId == carId && cf.FeatureId == featureId);

            if (carFeature != null)
            {
                _context.CarFeatures.Remove(carFeature);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Đếm số lượng xe có tính năng cụ thể
        /// </summary>
        public async Task<int> GetCarCountByFeatureAsync(int featureId)
        {
            return await _context.CarFeatures
                .Where(cf => cf.FeatureId == featureId)
                .CountAsync();
        }


        #endregion

        #region Phương thức quản lý hình ảnh và mô hình 3D

        /// <summary>
        /// Lấy danh sách hình ảnh của xe
        /// </summary>
        public async Task<IEnumerable<CarImage>> GetCarImagesAsync(int carId)
        {
            return await _context.CarImages
                .Where(ci => ci.CarId == carId)
                .ToListAsync();
        }


        /// <summary>
        /// Thêm hình ảnh cho xe
        /// </summary>
        public async Task AddCarImageAsync(int carId, CarImage image)
        {
            // Đảm bảo CarId được set đúng
            image.CarId = carId;

            // Nếu ảnh này được đánh dấu là ảnh chính, hãy cập nhật các ảnh khác
            if (image.IsMainImage)
            {
                var existingMainImages = await _context.CarImages
                    .Where(ci => ci.CarId == carId && ci.IsMainImage)
                    .ToListAsync();

                foreach (var existingImage in existingMainImages)
                {
                    existingImage.IsMainImage = false;
                }
            }

            await _context.CarImages.AddAsync(image);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Lấy thông tin mô hình 3D của xe
        /// </summary>
        public async Task<CarModel3D> GetCar3DModelAsync(int carId)
        {
            return await _context.CarModel3Ds
                .FirstOrDefaultAsync(cm => cm.CarId == carId);
        }

        /// <summary>
        /// Thêm mô hình 3D cho xe từ template
        /// </summary>
        public async Task AddCarModel3DAsync(int carId, int model3DTemplateId)
        {
            var template = await _context.Model3DTemplates.FindAsync(model3DTemplateId);
            if (template == null)
            {
                throw new InvalidOperationException($"Không tìm thấy mẫu mô hình 3D với ID {model3DTemplateId}");
            }

            // Kiểm tra xem xe đã có mô hình 3D chưa
            var existingModel = await _context.CarModel3Ds.FirstOrDefaultAsync(cm => cm.CarId == carId);

            if (existingModel != null)
            {
                // Cập nhật mô hình hiện có
                existingModel.Model3DTemplateId = model3DTemplateId;
                existingModel.FileUrl = template.FileUrl;
                existingModel.PreviewImageUrl = template.PreviewImageUrl;
            }
            else
            {
                // Tạo mô hình mới
                var carModel3D = new CarModel3D
                {
                    CarId = carId,
                    Model3DTemplateId = model3DTemplateId,
                    FileUrl = template.FileUrl,
                    PreviewImageUrl = template.PreviewImageUrl
                };

                await _context.CarModel3Ds.AddAsync(carModel3D);
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Lấy danh sách các mẫu mô hình 3D
        /// </summary>
        public async Task<IEnumerable<Model3DTemplate>> GetModel3DTemplatesAsync(int? brandId, int? categoryId)
        {
            IQueryable<Model3DTemplate> query = _context.Model3DTemplates;

            if (brandId.HasValue)
            {
                query = query.Where(m => m.BrandId == brandId);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(m => m.CategoryId == categoryId);
            }

            return await query.OrderBy(m => m.Name).ToListAsync();
        }

        #endregion

        #region Phương thức quản lý đánh giá

        /// <summary>
        /// Lấy danh sách đánh giá của xe
        /// </summary>
        public async Task<IEnumerable<Review>> GetCarReviewsAsync(int carId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.CarId == carId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy điểm đánh giá trung bình của xe
        /// </summary>
        public async Task<double> GetCarAverageRatingAsync(int carId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.CarId == carId)
                .ToListAsync();

            if (!reviews.Any())
                return 0;

            return reviews.Average(r => r.Rating);
        }

        #endregion

        #region Phương thức quản lý trạng thái xe


        /// <summary>
        /// Bật/tắt trạng thái khả dụng của xe
        /// </summary>
        public async Task ToggleAvailabilityAsync(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car != null)
            {
                car.IsAvailable = !car.IsAvailable;
                car.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Duyệt xe
        /// </summary>
        public async Task ApproveCarAsync(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car != null)
            {
                car.IsApproved = true;
                car.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Bật/tắt trạng thái duyệt của xe
        /// </summary>
        public async Task ToggleApprovalAsync(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car != null)
            {
                car.IsApproved = !car.IsApproved;
                car.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        #endregion
        // Thêm các phương thức sau vào class EFCarRepository
        public async Task<CarImage> AddImageAsync(CarImage image)
        {
            await _context.CarImages.AddAsync(image);
            await _context.SaveChangesAsync();
            return image;
        }

        public async Task SetMainImageAsync(int carId, int imageId)
        {
            // Đặt tất cả ảnh của xe này thành không phải ảnh chính
            var carImages = await _context.CarImages.Where(ci => ci.CarId == carId).ToListAsync();
            foreach (var image in carImages)
            {
                image.IsMainImage = false;
            }

            // Đặt ảnh được chọn làm ảnh chính
            var mainImage = carImages.FirstOrDefault(ci => ci.Id == imageId);
            if (mainImage != null)
            {
                mainImage.IsMainImage = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteImageAsync(int imageId)
        {
            var image = await _context.CarImages.FindAsync(imageId);
            if (image != null)
            {
                bool isMainImage = image.IsMainImage;
                int carId = image.CarId;

                _context.CarImages.Remove(image);
                await _context.SaveChangesAsync();

                // Nếu xóa ảnh chính, đặt ảnh đầu tiên còn lại làm ảnh chính
                if (isMainImage)
                {
                    var remainingImage = await _context.CarImages.FirstOrDefaultAsync(ci => ci.CarId == carId);
                    if (remainingImage != null)
                    {
                        remainingImage.IsMainImage = true;
                        await _context.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
