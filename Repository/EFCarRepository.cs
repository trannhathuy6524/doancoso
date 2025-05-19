using GotoCarRental.Data;
using GotoCarRental.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GotoCarRental.Repository
{
    public class EFCarRepository : ICarRepository
    {
        private readonly ApplicationDbContext _context;

        public EFCarRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        #region CRUD cơ bản

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

            await _context.SaveChangesAsync();
        }




        public async Task DeleteAsync(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car != null)
            {
                _context.Cars.Remove(car);
                await _context.SaveChangesAsync();
            }
        }

        #endregion

        #region Tìm kiếm nâng cao

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

        #endregion

        #region Danh sách xe theo điều kiện

        public async Task<IEnumerable<Car>> GetAvailableCarsAsync()
        {
            return await _context.Cars
                .Include(c => c.Brand)
                .Include(c => c.Category)
                .Include(c => c.CarImages)
                .Where(c => c.IsAvailable && c.IsApproved && !c.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Car>> GetCarsByBrandAsync(int brandId)
        {
            return await _context.Cars
                .Include(c => c.Brand)
                .Include(c => c.Category)
                .Include(c => c.CarImages)
                .Where(c => c.BrandId == brandId && c.IsApproved && !c.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Car>> GetCarsByCategoryAsync(int categoryId)
        {
            return await _context.Cars
                .Include(c => c.Brand)
                .Include(c => c.Category)
                .Include(c => c.CarImages)
                .Where(c => c.CategoryId == categoryId && c.IsApproved)
                .ToListAsync();
        }

        public async Task<IEnumerable<Car>> GetCarsByUserAsync(string userId)
        {
            return await _context.Cars
                .Include(c => c.Brand)
                .Include(c => c.Category)
                .Include(c => c.CarImages)
                .Where(c => c.UserId == userId && !c.IsDeleted)
                .ToListAsync();
        }

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

        public async Task<IEnumerable<Car>> GetUnapprovedCarsAsync()
        {
            return await _context.Cars
                .Include(c => c.Brand)
                .Include(c => c.Category)
                .Include(c => c.CarImages)
                .Where(c => !c.IsApproved && !c.IsDeleted)
                .ToListAsync();
        }

        #endregion

        #region Tìm kiếm và lọc

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

        // Triển khai trong EFCarRepository
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

        #region Tính năng của xe

        public async Task<IEnumerable<Feature>> GetCarFeaturesAsync(int carId)
        {
            return await _context.CarFeatures
                .Where(cf => cf.CarId == carId)
                .Select(cf => cf.Feature)
                .ToListAsync();
        }

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

        // Thêm vào lớp EFCarRepository
        public async Task<int> GetCarCountByFeatureAsync(int featureId)
        {
            return await _context.CarFeatures
                .Where(cf => cf.FeatureId == featureId)
                .CountAsync();
        }


        #endregion

        #region Hình ảnh và mô hình 3D

        public async Task<IEnumerable<CarImage>> GetCarImagesAsync(int carId)
        {
            return await _context.CarImages
                .Where(ci => ci.CarId == carId)
                .ToListAsync();
        }

        public async Task<CarModel3D> GetCar3DModelAsync(int carId)
        {
            return await _context.CarModel3Ds
                .FirstOrDefaultAsync(cm => cm.CarId == carId);
        }

        // Phương thức phù hợp với chữ ký trong interface
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

        #endregion

        #region Đánh giá

        public async Task<IEnumerable<Review>> GetCarReviewsAsync(int carId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.CarId == carId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

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

        #region Phân trang

        // Cập nhật implementation trong EFCarRepository.cs
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


        #endregion

        #region Kiểm tra và cập nhật trạng thái

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Cars.AnyAsync(c => c.Id == id);
        }

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

        #endregion

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



        #region Mô hình 3D

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

    }
}
