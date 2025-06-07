using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace GotoCarRental.Areas.CarOwner.Pages.Cars
{
    /// <summary>
    /// PageModel cho việc chỉnh sửa thông tin xe của chủ xe
    /// </summary>
    [Authorize(Roles = "CarOwner")]
    public class EditModel : PageModel
    {
        #region Private Fields

        private readonly ICarRepository _carRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IFeatureRepository _featureRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<EditModel> _logger;
        private readonly IProvinceRepository _provinceRepository;

        #endregion

        #region Constructor

        public EditModel(
            ICarRepository carRepository,
            IBrandRepository brandRepository,
            ICategoryRepository categoryRepository,
            IFeatureRepository featureRepository,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment,
            ILogger<EditModel> logger,
            IProvinceRepository provinceRepository)
        {
            _carRepository = carRepository;
            _brandRepository = brandRepository;
            _categoryRepository = categoryRepository;
            _featureRepository = featureRepository;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
            _provinceRepository = provinceRepository;
        }

        #endregion

        #region Properties

        [BindProperty]
        public Car Car { get; set; }

        [BindProperty]
        public IFormFile MainImage { get; set; }

        [BindProperty]
        public List<int> SelectedFeatures { get; set; }

        [BindProperty]
        public bool EnableHourlyRental { get; set; }

        public SelectList BrandSelectList { get; set; }
        public SelectList CategorySelectList { get; set; }
        public IEnumerable<CarImage> CarImages { get; set; }
        public IEnumerable<Feature> AllFeatures { get; set; }
        public IEnumerable<int> CarFeatureIds { get; set; }
        public bool IsOwner { get; set; }
        public SelectList ProvinceSelectList { get; set; }

        #endregion

        #region Page Handlers

        /// <summary>
        /// Xử lý HTTP GET - Hiển thị form chỉnh sửa xe
        /// </summary>
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            try
            {
                // Kiểm tra id có tồn tại
                if (id == null)
                {
                    _logger.LogWarning("Yêu cầu chỉnh sửa xe với ID rỗng");
                    return NotFound("Không tìm thấy ID xe");
                }

                // Lấy thông tin xe
                Car = await _carRepository.GetByIdAsync(id.Value);
                if (Car == null)
                {
                    _logger.LogWarning("Không tìm thấy xe với ID: {Id}", id);
                    return NotFound($"Không tìm thấy xe với ID: {id}");
                }

                // Kiểm tra quyền sở hữu
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                IsOwner = (userId == Car.UserId);

                // Nếu không phải chủ xe, không cho sửa
                if (!IsOwner)
                {
                    _logger.LogWarning("Người dùng {UserId} không có quyền chỉnh sửa xe {CarId}", userId, id);
                    return Forbid();
                }

                // Thiết lập trạng thái ban đầu cho chức năng thuê theo giờ
                EnableHourlyRental = Car.PricePerHour > 0;

                // Tải dữ liệu cần thiết
                await LoadSelectLists();
                CarImages = await _carRepository.GetCarImagesAsync(id.Value);
                AllFeatures = await _featureRepository.GetAllAsync();
                CarFeatureIds = Car.CarFeatures?.Select(cf => cf.FeatureId) ?? new List<int>();

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải trang chỉnh sửa xe ID {Id}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin xe. Vui lòng thử lại sau.";
                return RedirectToPage("/MyCars", new { area = "CarOwner" });
            }
        }


        /// <summary>
        /// Xử lý HTTP POST - Lưu thông tin xe đã chỉnh sửa
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // Kiểm tra quyền sở hữu
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var existingCar = await _carRepository.GetByIdAsync(Car.Id);

                if (existingCar == null)
                {
                    _logger.LogWarning("Không tìm thấy xe với ID: {Id}", Car.Id);
                    return NotFound($"Không tìm thấy xe với ID: {Car.Id}");
                }

                if (existingCar.UserId != currentUserId)
                {
                    _logger.LogWarning("Người dùng {UserId} không có quyền chỉnh sửa xe {CarId}", currentUserId, Car.Id);
                    return Forbid();
                }

                // Loại bỏ ModelState validation cho tất cả các thuộc tính không cần thiết
                foreach (var key in ModelState.Keys.ToList())
                {
                    if (key != "Car.Name" && key != "Car.Description" && key != "Car.BrandId" &&
                        key != "Car.CategoryId" && key != "Car.ProvinceId" && key != "Car.DetailedAddress")
                    {
                        ModelState.Remove(key);
                    }
                }

                // QUAN TRỌNG: Giữ nguyên giá trị PricePerDay nếu không thay đổi
                if (Car.PricePerDay <= 0)
                {
                    Car.PricePerDay = existingCar.PricePerDay;
                    _logger.LogInformation("Đã sử dụng giá thuê theo ngày từ giá trị hiện có: {Price}", Car.PricePerDay);
                }

                // Kiểm tra ModelState sau khi bỏ qua các validation
                if (!ModelState.IsValid)
                {
                    _logger.LogInformation("Dữ liệu không hợp lệ khi cập nhật xe ID {Id}", Car.Id);
                    await LoadFormDataForInvalidModelState(existingCar);
                    return Page();
                }

                // Giữ nguyên các giá trị khác từ existingCar
                Car.UserId = existingCar.UserId;
                Car.IsApproved = existingCar.IsApproved;
                Car.CreatedAt = existingCar.CreatedAt;
                Car.UpdatedAt = DateTime.Now;

                // Xử lý cho tính năng thuê theo giờ
                if (!EnableHourlyRental)
                {
                    Car.PricePerHour = 0;
                }
                else if (Car.PricePerHour <= 0)
                {
                    Car.PricePerHour = existingCar.PricePerHour > 0 ? existingCar.PricePerHour : 10000;
                }

                // Xử lý cho dịch vụ tài xế
                if (!Car.OfferDriverService)
                {
                    Car.DriverFeePerDay = 0;
                    Car.DriverFeePerHour = 0;
                }
                else
                {
                    if (Car.DriverFeePerDay <= 0)
                    {
                        Car.DriverFeePerDay = existingCar.DriverFeePerDay > 0 ? existingCar.DriverFeePerDay : 200000;
                    }

                    if (EnableHourlyRental && Car.DriverFeePerHour <= 0)
                    {
                        Car.DriverFeePerHour = existingCar.DriverFeePerHour > 0 ? existingCar.DriverFeePerHour : 20000;
                    }
                }

                // Cập nhật thông tin xe
                await _carRepository.UpdateAsync(Car);
                _logger.LogInformation("Cập nhật thành công thông tin cơ bản của xe ID {Id}", Car.Id);

                // Xử lý các tính năng của xe
                await ProcessCarFeatures();

                // Xử lý tải lên ảnh nếu có
                await ProcessImageUpload();

                TempData["SuccessMessage"] = "Cập nhật thông tin xe thành công!";
                return RedirectToPage("./Details", new { id = Car.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật xe ID {Id}", Car.Id);
                ModelState.AddModelError(string.Empty, $"Có lỗi xảy ra: {ex.Message}");

                var existingCar = await _carRepository.GetByIdAsync(Car.Id);
                await LoadFormDataForInvalidModelState(existingCar);
                return Page();
            }
        }


        /// <summary>
        /// Handler để toggle trạng thái available
        /// </summary>
        public async Task<IActionResult> OnPostToggleAvailabilityAsync(int id)
        {
            try
            {
                // Kiểm tra quyền sở hữu
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var car = await _carRepository.GetByIdAsync(id);

                if (car == null)
                {
                    _logger.LogWarning("Không tìm thấy xe với ID: {Id}", id);
                    return NotFound($"Không tìm thấy xe với ID: {id}");
                }

                if (car.UserId != currentUserId)
                {
                    _logger.LogWarning("Người dùng {UserId} không có quyền chỉnh sửa xe {CarId}", currentUserId, id);
                    return Forbid();
                }

                // Toggle IsAvailable
                await _carRepository.ToggleAvailabilityAsync(id);
                _logger.LogInformation("Đã thay đổi trạng thái sẵn sàng của xe {CarId}", id);

                TempData["SuccessMessage"] = $"Đã thay đổi trạng thái sẵn sàng của xe {car.Name}";
                return RedirectToPage("./Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thay đổi trạng thái sẵn sàng của xe ID {Id}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi thay đổi trạng thái. Vui lòng thử lại sau.";
                return RedirectToPage("./Details", new { id });
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Tải các danh sách chọn (SelectList) cho form
        /// </summary>
        private async Task LoadSelectLists()
        {
            try
            {
                var brands = await _brandRepository.GetAllAsync();
                BrandSelectList = new SelectList(brands, "Id", "Name", Car?.BrandId);

                var categories = await _categoryRepository.GetAllAsync();
                CategorySelectList = new SelectList(categories, "Id", "Name", Car?.CategoryId);

                var provinces = await _provinceRepository.GetAllAsync();
                ProvinceSelectList = new SelectList(provinces, "Id", "Name", Car?.ProvinceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách chọn");
                throw new Exception("Không thể tải danh sách hãng xe, loại xe hoặc tỉnh/thành phố", ex);
            }
        }

        /// <summary>
        /// Lưu hình ảnh mới tải lên
        /// </summary>
        private async Task<string> SaveImage(IFormFile image, int carId)
        {
            try
            {
                // Kiểm tra kích thước file
                if (image.Length > 5 * 1024 * 1024) // 5MB
                {
                    throw new Exception("Kích thước file quá lớn. Tối đa 5MB được cho phép.");
                }

                // Kiểm tra định dạng file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(image.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    throw new Exception("Chỉ chấp nhận định dạng hình ảnh: JPG, PNG, GIF.");
                }

                // Tạo đường dẫn thư mục lưu ảnh
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "cars", carId.ToString());
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Tạo tên file ảnh độc nhất
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(image.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Lưu file ảnh
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }

                _logger.LogInformation("Đã lưu hình ảnh mới cho xe {CarId}: {FilePath}", carId, uniqueFileName);

                // Trả về đường dẫn tương đối của ảnh
                return $"/images/cars/{carId}/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lưu ảnh cho xe ID {CarId}", carId);
                throw new Exception($"Lỗi khi lưu ảnh: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Loại bỏ ModelState validation cho các navigation properties
        /// </summary>
        private void CleanupModelState()
        {
            ModelState.Remove("Car.User");
            ModelState.Remove("Car.Brand");
            ModelState.Remove("Car.Category");
            ModelState.Remove("Car.Rentals");
            ModelState.Remove("Car.Reviews");
            ModelState.Remove("Car.CarFeatures");
            ModelState.Remove("Car.CarModel3D");
            ModelState.Remove("Car.CarImages");
            ModelState.Remove("Car.Province");
            ModelState.Remove("MainImage"); // MainImage không bắt buộc khi sửa
        }

        /// <summary>
        /// Tải lại dữ liệu cần thiết khi ModelState không hợp lệ
        /// </summary>
        private async Task LoadFormDataForInvalidModelState(Car existingCar)
        {
            await LoadSelectLists();
            CarImages = await _carRepository.GetCarImagesAsync(Car.Id);
            AllFeatures = await _featureRepository.GetAllAsync();
            CarFeatureIds = existingCar.CarFeatures?.Select(cf => cf.FeatureId) ?? new List<int>();

            // Giữ nguyên trạng thái EnableHourlyRental
            if (EnableHourlyRental && Car.PricePerHour <= 0)
            {
                Car.PricePerHour = existingCar.PricePerHour > 0 ? existingCar.PricePerHour : 10000;
            }

            // Giữ nguyên trạng thái OfferDriverService
            if (Car.OfferDriverService)
            {
                if (Car.DriverFeePerDay <= 0)
                {
                    Car.DriverFeePerDay = existingCar.DriverFeePerDay > 0 ? existingCar.DriverFeePerDay : 200000;
                }
                if (EnableHourlyRental && Car.DriverFeePerHour <= 0)
                {
                    Car.DriverFeePerHour = existingCar.DriverFeePerHour > 0 ? existingCar.DriverFeePerHour : 20000;
                }
            }
        }


        /// <summary>
        /// Giữ nguyên các giá trị không được phép thay đổi
        /// </summary>
        private void PreserveUnchangeableProperties(Car existingCar)
        {
            Car.UserId = existingCar.UserId;
            Car.IsApproved = existingCar.IsApproved;
            Car.CreatedAt = existingCar.CreatedAt;
            Car.UpdatedAt = DateTime.Now;
        }

        /// <summary>
        /// Xử lý trạng thái tắt/bật thuê theo giờ
        /// </summary>
        private void HandleHourlyRentalOption()
        {
            // Đảm bảo giá thuê theo ngày luôn hợp lệ
            if (Car.PricePerDay <= 0)
            {
                Car.PricePerDay = 100000; // Giá mặc định nếu không nhập
                _logger.LogInformation("Đã đặt giá thuê mặc định cho xe ID {CarId}", Car.Id);
            }

            if (!EnableHourlyRental)
            {
                // Nếu tính năng thuê theo giờ bị tắt, đặt PricePerHour = 0
                Car.PricePerHour = 0;
                _logger.LogInformation("Đã tắt tính năng thuê theo giờ cho xe ID {CarId}", Car.Id);
            }
            else
            {
                // Nếu bật, đảm bảo giá trị PricePerHour là hợp lệ (> 0)
                if (Car.PricePerHour <= 0)
                {
                    Car.PricePerHour = 10000; // Đặt giá mặc định nếu người dùng không nhập
                    _logger.LogInformation("Đã đặt giá thuê theo giờ mặc định cho xe ID {CarId}", Car.Id);
                }
            }

            // Thêm đoạn code để xử lý dịch vụ tài xế
            if (!Car.OfferDriverService)
            {
                // Nếu không cung cấp dịch vụ tài xế, đặt giá = 0
                Car.DriverFeePerDay = 0;
                Car.DriverFeePerHour = 0;
                _logger.LogInformation("Đã tắt dịch vụ tài xế cho xe ID {CarId}", Car.Id);
            }
            else
            {
                // Nếu cung cấp dịch vụ tài xế, đảm bảo giá hợp lệ
                if (Car.DriverFeePerDay <= 0)
                {
                    Car.DriverFeePerDay = 200000; // Giá mặc định cho phí tài xế theo ngày
                    _logger.LogInformation("Đã đặt giá mặc định cho phí tài xế theo ngày, xe ID {CarId}", Car.Id);
                }

                // Chỉ thiết lập giá tài xế theo giờ nếu có thuê theo giờ
                if (EnableHourlyRental && Car.DriverFeePerHour <= 0)
                {
                    Car.DriverFeePerHour = 20000; // Giá mặc định cho phí tài xế theo giờ
                    _logger.LogInformation("Đã đặt giá mặc định cho phí tài xế theo giờ, xe ID {CarId}", Car.Id);
                }
            }
        }

        /// <summary>
        /// Xử lý các tính năng của xe
        /// </summary>
        private async Task ProcessCarFeatures()
        {
            try
            {
                if (SelectedFeatures != null && SelectedFeatures.Any())
                {
                    // Lấy danh sách tính năng hiện tại của xe
                    var currentFeatures = (await _carRepository.GetCarFeaturesAsync(Car.Id)).ToList();
                    var currentFeatureIds = currentFeatures.Select(f => f.Id).ToList();

                    // Thêm các tính năng mới được chọn
                    foreach (var featureId in SelectedFeatures)
                    {
                        if (!currentFeatureIds.Contains(featureId))
                        {
                            await _carRepository.AddFeatureToCarAsync(Car.Id, featureId);
                            _logger.LogInformation("Đã thêm tính năng {FeatureId} cho xe {CarId}", featureId, Car.Id);
                        }
                    }

                    // Xóa các tính năng không còn được chọn
                    foreach (var feature in currentFeatures)
                    {
                        if (!SelectedFeatures.Contains(feature.Id))
                        {
                            await _carRepository.RemoveFeatureFromCarAsync(Car.Id, feature.Id);
                            _logger.LogInformation("Đã xóa tính năng {FeatureId} khỏi xe {CarId}", feature.Id, Car.Id);
                        }
                    }
                }
                else
                {
                    // Nếu không có tính năng nào được chọn, xóa tất cả tính năng hiện có
                    var currentFeatures = await _carRepository.GetCarFeaturesAsync(Car.Id);
                    foreach (var feature in currentFeatures)
                    {
                        await _carRepository.RemoveFeatureFromCarAsync(Car.Id, feature.Id);
                    }
                    _logger.LogInformation("Đã xóa tất cả tính năng khỏi xe {CarId}", Car.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý tính năng cho xe ID {Id}", Car.Id);
                throw new Exception("Không thể cập nhật tính năng cho xe", ex);
            }
        }

        /// <summary>
        /// Xử lý tải lên ảnh mới
        /// </summary>
        private async Task ProcessImageUpload()
        {
            if (MainImage != null)
            {
                try
                {
                    var imageUrl = await SaveImage(MainImage, Car.Id);

                    // Thêm ảnh mới vào CarImage
                    var carImage = new CarImage
                    {
                        CarId = Car.Id,
                        ImageUrl = imageUrl,
                        IsMainImage = true
                    };

                    await _carRepository.AddCarImageAsync(Car.Id, carImage);
                    _logger.LogInformation("Đã thêm ảnh chính mới cho xe {CarId}", Car.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi tải lên ảnh mới cho xe ID {Id}", Car.Id);
                    throw new Exception("Không thể tải lên ảnh mới", ex);
                }
            }
        }

        #endregion
    }
}
