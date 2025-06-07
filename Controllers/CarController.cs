using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Logging;


namespace GotoCarRental.Controllers
{
    public class CarController : Controller
    {
        private readonly ICarRepository _carRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<CarController> _logger;


        public CarController(
            ICarRepository carRepository,
            IBrandRepository brandRepository,
            ICategoryRepository categoryRepository,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment,
            ILogger<CarController> logger)
        {
            _carRepository = carRepository;
            _brandRepository = brandRepository;
            _categoryRepository = categoryRepository;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        // GET: /Car
        public async Task<IActionResult> Index(
            string searchTerm,
            decimal? minPrice,
            decimal? maxPrice,
            int? brandId,
            int? categoryId,
            bool? isAvailable,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var result = await _carRepository.GetCarsPagedAsync(
                pageNumber,
                pageSize,
                searchTerm,
                minPrice,
                maxPrice,
                brandId,
                categoryId,
                isAvailable);

            ViewBag.SearchTerm = searchTerm;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.BrandId = brandId;
            ViewBag.CategoryId = categoryId;
            ViewBag.IsAvailable = isAvailable;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = result.TotalCount;
            ViewBag.TotalPages = (int)Math.Ceiling(result.TotalCount / (double)pageSize);

            // Thêm danh sách brands và categories vào ViewBag
            ViewBag.Brands = await _brandRepository.GetAllAsync();
            ViewBag.Categories = await _categoryRepository.GetAllAsync();

            return View(result.Cars);
        }

        // GET: /Car/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var car = await _carRepository.GetByIdAsync(id);
            if (car == null)
            {
                return NotFound();
            }

            // Lấy đánh giá của xe
            var reviews = await _carRepository.GetCarReviewsAsync(id);
            ViewBag.Reviews = reviews;

            // Lấy điểm đánh giá trung bình
            var averageRating = await _carRepository.GetCarAverageRatingAsync(id);
            ViewBag.AverageRating = averageRating;

            // Lấy hình ảnh của xe
            var carImages = await _carRepository.GetCarImagesAsync(id);
            ViewBag.CarImages = carImages;

            // Lấy model 3D của xe (nếu có)
            var model3D = await _carRepository.GetCar3DModelAsync(id);
            ViewBag.Model3D = model3D;

            // Lấy tính năng của xe
            var features = await _carRepository.GetCarFeaturesAsync(id);
            ViewBag.Features = features;

            return View(car);
        }

        // GET: /Car/Create
        // GET: /Car/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            // Lấy danh sách hãng xe và loại xe từ repository
            ViewBag.Brands = await _brandRepository.GetAllAsync();
            ViewBag.Categories = await _categoryRepository.GetAllAsync();

            return View();
        }


        // POST: /Car/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CarOwner,Admin")]
        public async Task<IActionResult> Create(Car car, List<IFormFile> carImages)
        {
            // Loại bỏ ModelState validation cho các navigation properties
            ModelState.Remove("User");
            ModelState.Remove("Brand");
            ModelState.Remove("Category");
            ModelState.Remove("Rentals");
            ModelState.Remove("Reviews");
            ModelState.Remove("CarFeatures");
            ModelState.Remove("CarModel3D");
            ModelState.Remove("CarImages");
            ModelState.Remove("UserId"); // Quan trọng: Loại bỏ ModelState cho UserId

            if (ModelState.IsValid)
            {
                try
                {
                    // Gán UserId từ User hiện tại đang đăng nhập
                    car.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                    // Đảm bảo rằng UserId không null hoặc rỗng
                    if (string.IsNullOrEmpty(car.UserId))
                    {
                        ModelState.AddModelError("", "Bạn cần đăng nhập để thêm xe mới.");
                        ViewBag.Brands = await _brandRepository.GetAllAsync();
                        ViewBag.Categories = await _categoryRepository.GetAllAsync();
                        return View(car);
                    }

                    car.IsApproved = false; // Mặc định xe chưa được duyệt

                    var newCar = await _carRepository.AddAsync(car);

                    // Code xử lý tải lên hình ảnh không thay đổi
                    if (carImages != null && carImages.Count > 0)
                    {
                        await UploadCarImagesAsync(newCar.Id, carImages);
                    }

                    TempData["SuccessMessage"] = "Xe đã được đăng ký thành công và đang chờ duyệt!";
                    return RedirectToAction(nameof(MyCars));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi thêm xe mới");
                    ModelState.AddModelError("", $"Đã xảy ra lỗi khi đăng ký xe: {ex.Message}");
                }
            }

            // Nếu có lỗi, log chi tiết
            foreach (var state in ModelState)
            {
                if (state.Value.Errors.Any())
                {
                    foreach (var error in state.Value.Errors)
                    {
                        _logger.LogError($"Field: {state.Key}, Error: {error.ErrorMessage}");
                    }
                }
            }

            // Truyền lại danh sách brands và categories vào ViewBag
            ViewBag.Brands = await _brandRepository.GetAllAsync();
            ViewBag.Categories = await _categoryRepository.GetAllAsync();

            return View(car);
        }



        // GET: /Car/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var car = await _carRepository.GetByIdAsync(id);
            if (car == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền sở hữu hoặc quyền admin
            if (!IsOwnerOrAdmin(car.UserId))
            {
                return Forbid();
            }

            ViewBag.Brands = await _brandRepository.GetAllAsync();
            ViewBag.Categories = await _categoryRepository.GetAllAsync();

            return View(car);
        }

        // POST: /Car/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, Car car, List<IFormFile> carImages)
        {
            if (id != car.Id)
            {
                return NotFound();
            }

            var existingCar = await _carRepository.GetByIdAsync(id);
            if (existingCar == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền sở hữu hoặc quyền admin
            if (!IsOwnerOrAdmin(existingCar.UserId))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Cập nhật thông tin xe
                    existingCar.Name = car.Name;
                    existingCar.Description = car.Description;
                    existingCar.PricePerDay = car.PricePerDay;
                    existingCar.BrandId = car.BrandId;
                    existingCar.CategoryId = car.CategoryId;
                    existingCar.IsAvailable = car.IsAvailable;

                    await _carRepository.UpdateAsync(existingCar);

                    // Xử lý tải lên hình ảnh mới
                    if (carImages != null && carImages.Count > 0)
                    {
                        await UploadCarImagesAsync(id, carImages);
                    }
                }
                catch (Exception)
                {
                    if (!await _carRepository.ExistsAsync(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(car);
        }

        // GET: /Car/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var car = await _carRepository.GetByIdAsync(id);
            if (car == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền sở hữu hoặc quyền admin
            if (!IsOwnerOrAdmin(car.UserId))
            {
                return Forbid();
            }

            return View(car);
        }

        // POST: /Car/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var car = await _carRepository.GetByIdAsync(id);
            if (car == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền sở hữu hoặc quyền admin
            if (!IsOwnerOrAdmin(car.UserId))
            {
                return Forbid();
            }

            await _carRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // POST: /Car/ToggleAvailability/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ToggleAvailability(int id)
        {
            var car = await _carRepository.GetByIdAsync(id);
            if (car == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền sở hữu hoặc quyền admin
            if (!IsOwnerOrAdmin(car.UserId))
            {
                return Forbid();
            }

            await _carRepository.ToggleAvailabilityAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // POST: /Car/ApproveCar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveCar(int id)
        {
            var car = await _carRepository.GetByIdAsync(id);
            if (car == null)
            {
                return NotFound();
            }

            await _carRepository.ApproveCarAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Car/MyCars
        [Authorize]
        public async Task<IActionResult> MyCars()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cars = await _carRepository.GetCarsByUserAsync(userId);
            return View(cars);
        }

        // POST: /Car/AddFeature
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> AddFeature(int carId, int featureId)
        {
            var car = await _carRepository.GetByIdAsync(carId);
            if (car == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền sở hữu hoặc quyền admin
            if (!IsOwnerOrAdmin(car.UserId))
            {
                return Forbid();
            }

            await _carRepository.AddFeatureToCarAsync(carId, featureId);
            return RedirectToAction(nameof(Edit), new { id = carId });
        }

        // POST: /Car/RemoveFeature
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> RemoveFeature(int carId, int featureId)
        {
            var car = await _carRepository.GetByIdAsync(carId);
            if (car == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền sở hữu hoặc quyền admin
            if (!IsOwnerOrAdmin(car.UserId))
            {
                return Forbid();
            }

            await _carRepository.RemoveFeatureFromCarAsync(carId, featureId);
            return RedirectToAction(nameof(Edit), new { id = carId });
        }

        // POST: /Car/DeleteImage
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteImage(int imageId, int carId)
        {
            var car = await _carRepository.GetByIdAsync(carId);
            if (car == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền sở hữu hoặc quyền admin
            if (!IsOwnerOrAdmin(car.UserId))
            {
                return Forbid();
            }

            // Xóa hình ảnh (cần thêm phương thức này vào ICarRepository)
            // await _carRepository.DeleteCarImageAsync(imageId);

            return RedirectToAction(nameof(Edit), new { id = carId });
        }

        // GET: /Car/Search
        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return RedirectToAction(nameof(Index));
            }

            var cars = await _carRepository.SearchCarsAsync(searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", cars);
        }

        // GET: /Car/Filter
        public async Task<IActionResult> Filter(decimal? minPrice, decimal? maxPrice, int? brandId, int? categoryId)
        {
            var cars = await _carRepository.FilterCarsAsync(minPrice, maxPrice, brandId, categoryId);

            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.BrandId = brandId;
            ViewBag.CategoryId = categoryId;
            ViewBag.Brands = await _brandRepository.GetAllAsync();
            ViewBag.Categories = await _categoryRepository.GetAllAsync();

            return View("Index", cars);
        }

        // Helper methods
        private bool IsOwnerOrAdmin(string ownerId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == ownerId)
            {
                return true;
            }

            // Kiểm tra quyền admin
            return User.IsInRole("Admin");
        }

        private async Task UploadCarImagesAsync(int carId, List<IFormFile> images)
        {
            // Tạo thư mục lưu ảnh nếu chưa tồn tại
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "cars", carId.ToString());
            Directory.CreateDirectory(uploadsFolder);

            foreach (var image in images)
            {
                if (image != null && image.Length > 0)
                {
                    // Tạo tên file duy nhất
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Lưu file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                    }

                    // Đường dẫn lưu vào DB
                    var relativePath = Path.Combine("images", "cars", carId.ToString(), uniqueFileName).Replace("\\", "/");

                    // Lưu thông tin ảnh vào DB
                    await _carRepository.AddCarImageAsync(carId, new CarImage
                    {
                        CarId = carId,
                        ImageUrl = relativePath
                    });
                }
            }
        }
    }
}
