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
    [Authorize(Roles = "CarOwner")]
    public class EditModel : PageModel
    {
        private readonly ICarRepository _carRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IFeatureRepository _featureRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<EditModel> _logger;
        private readonly IProvinceRepository _provinceRepository;


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

        [BindProperty]
        public Car Car { get; set; }

        [BindProperty]
        public IFormFile MainImage { get; set; }

        [BindProperty]
        public List<int> SelectedFeatures { get; set; }

        public SelectList BrandSelectList { get; set; }
        public SelectList CategorySelectList { get; set; }
        public IEnumerable<CarImage> CarImages { get; set; }
        public IEnumerable<Feature> AllFeatures { get; set; }
        public IEnumerable<int> CarFeatureIds { get; set; }
        public bool IsOwner { get; set; }
        public SelectList ProvinceSelectList { get; set; }


        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Car = await _carRepository.GetByIdAsync(id.Value);
            if (Car == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền sở hữu
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            IsOwner = (userId == Car.UserId);

            // Nếu không phải chủ xe, không cho sửa
            if (!IsOwner)
            {
                return Forbid();
            }

            await LoadSelectLists();
            CarImages = await _carRepository.GetCarImagesAsync(id.Value);

            // Lấy tất cả tính năng
            AllFeatures = await _featureRepository.GetAllAsync();

            // Lấy ID các tính năng của xe
            CarFeatureIds = Car.CarFeatures?.Select(cf => cf.FeatureId) ?? new List<int>();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Kiểm tra quyền sở hữu
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existingCar = await _carRepository.GetByIdAsync(Car.Id);
            if (existingCar == null || existingCar.UserId != currentUserId)
            {
                return Forbid();
            }

            // Loại bỏ ModelState validation cho các navigation properties
            ModelState.Remove("Car.User");
            ModelState.Remove("Car.Brand");
            ModelState.Remove("Car.Category");
            ModelState.Remove("Car.Rentals");
            ModelState.Remove("Car.Reviews");
            ModelState.Remove("Car.CarFeatures");
            ModelState.Remove("Car.CarModel3D");
            ModelState.Remove("Car.CarImages");
            ModelState.Remove("MainImage"); // MainImage không bắt buộc khi sửa
            ModelState.Remove("Car.Province");

            if (!ModelState.IsValid)
            {
                await LoadSelectLists();
                CarImages = await _carRepository.GetCarImagesAsync(Car.Id);
                AllFeatures = await _featureRepository.GetAllAsync();
                CarFeatureIds = existingCar.CarFeatures?.Select(cf => cf.FeatureId) ?? new List<int>();
                return Page();
            }

            try
            {
                // Giữ nguyên các giá trị không được phép thay đổi
                Car.UserId = existingCar.UserId;
                Car.IsApproved = existingCar.IsApproved;
                Car.CreatedAt = existingCar.CreatedAt;
                Car.UpdatedAt = DateTime.Now;

                // Cập nhật thông tin xe
                await _carRepository.UpdateAsync(Car);

                // Xử lý các tính năng của xe
                if (SelectedFeatures != null)
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
                        }
                    }

                    // Xóa các tính năng không còn được chọn
                    foreach (var feature in currentFeatures)
                    {
                        if (!SelectedFeatures.Contains(feature.Id))
                        {
                            await _carRepository.RemoveFeatureFromCarAsync(Car.Id, feature.Id);
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
                }

                // Xử lý tải lên ảnh nếu có
                if (MainImage != null)
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
                }

                TempData["SuccessMessage"] = "Cập nhật thông tin xe thành công!";
                return RedirectToPage("./Details", new { id = Car.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật xe ID {Id}", Car.Id);
                ModelState.AddModelError(string.Empty, $"Có lỗi xảy ra: {ex.Message}");

                await LoadSelectLists();
                CarImages = await _carRepository.GetCarImagesAsync(Car.Id);
                AllFeatures = await _featureRepository.GetAllAsync();
                CarFeatureIds = existingCar.CarFeatures?.Select(cf => cf.FeatureId) ?? new List<int>();
                return Page();
            }
        }

        private async Task LoadSelectLists()
        {
            var brands = await _brandRepository.GetAllAsync();
            BrandSelectList = new SelectList(brands, "Id", "Name");

            var categories = await _categoryRepository.GetAllAsync();
            CategorySelectList = new SelectList(categories, "Id", "Name");

            var provinces = await _provinceRepository.GetAllAsync();
            ProvinceSelectList = new SelectList(provinces, "Id", "Name", Car?.ProvinceId);

        }

        private async Task<string> SaveImage(IFormFile image, int carId)
        {
            try
            {
                // Tạo đường dẫn thư mục lưu ảnh
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "cars", carId.ToString());
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Tạo tên file ảnh độc nhất
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Lưu file ảnh
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }

                // Trả về đường dẫn tương đối của ảnh
                return $"/images/cars/{carId}/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lưu ảnh: {ex.Message}", ex);
            }
        }

        // Handler để toggle trạng thái available
        public async Task<IActionResult> OnPostToggleAvailabilityAsync(int id)
        {
            // Kiểm tra quyền sở hữu
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var car = await _carRepository.GetByIdAsync(id);
            if (car == null || car.UserId != currentUserId)
            {
                return Forbid();
            }

            // Toggle IsAvailable
            await _carRepository.ToggleAvailabilityAsync(id);

            TempData["SuccessMessage"] = $"Đã thay đổi trạng thái sẵn sàng của xe {car.Name}";
            return RedirectToPage("./Details", new { id });
        }
    }
}
