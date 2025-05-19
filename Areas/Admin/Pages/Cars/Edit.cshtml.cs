using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GotoCarRental.Areas.Admin.Pages.Cars
{
    [Authorize(Roles = "Admin,Manager")]
    public class EditModel : PageModel
    {
        private readonly ICarRepository _carRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<EditModel> _logger;
        private readonly IProvinceRepository _provinceRepository;


        public EditModel(
            ICarRepository carRepository,
            IBrandRepository brandRepository,
            ICategoryRepository categoryRepository,
            IWebHostEnvironment webHostEnvironment,
            ILogger<EditModel> logger,
            IProvinceRepository provinceRepository)
        {
            _carRepository = carRepository;
            _brandRepository = brandRepository;
            _categoryRepository = categoryRepository;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
            _provinceRepository = provinceRepository;
        }

        [BindProperty]
        public Car Car { get; set; }

        [BindProperty]
        public IFormFile MainImage { get; set; }

        public SelectList BrandSelectList { get; set; }
        public SelectList CategorySelectList { get; set; }
        public IEnumerable<CarImage> CarImages { get; set; }
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

            // Đảm bảo Car.UserId không null
            if (Car.UserId == null)
            {
                // Nếu không có thông tin người dùng, gán một giá trị mặc định
                Car.UserId = string.Empty;
            }

            await LoadSelectLists();
            CarImages = await _carRepository.GetCarImagesAsync(id.Value);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Loại bỏ ModelState validation cho các navigation properties và các trường không cần thiết
            ModelState.Remove("Car.User");
            ModelState.Remove("Car.Brand");
            ModelState.Remove("Car.Category");
            ModelState.Remove("Car.Rentals");
            ModelState.Remove("Car.Reviews");
            ModelState.Remove("Car.CarFeatures");
            ModelState.Remove("Car.CarModel3D");
            ModelState.Remove("Car.CarImages");
            ModelState.Remove("MainImage"); // MainImage không bắt buộc khi sửa

            if (!ModelState.IsValid)
            {
                await LoadSelectLists();
                CarImages = await _carRepository.GetCarImagesAsync(Car.Id);
                return Page();
            }

            try
            {
                // Cập nhật thông tin xe
                await _carRepository.UpdateAsync(Car);

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
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật xe ID {Id}", Car.Id);
                ModelState.AddModelError(string.Empty, $"Có lỗi xảy ra: {ex.Message}");

                await LoadSelectLists();
                CarImages = await _carRepository.GetCarImagesAsync(Car.Id);
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
    }
}
