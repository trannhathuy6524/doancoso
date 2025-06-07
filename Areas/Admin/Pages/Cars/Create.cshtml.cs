using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GotoCarRental.Areas.Admin.Pages.Cars
{
    [Authorize(Roles = "Admin,Manager")]
    public class CreateModel : PageModel
    {
        private readonly ICarRepository _carRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(
            ICarRepository carRepository,
            IBrandRepository brandRepository,
            ICategoryRepository categoryRepository,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment,
            ILogger<CreateModel> logger)
        {
            _carRepository = carRepository;
            _brandRepository = brandRepository;
            _categoryRepository = categoryRepository;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        [BindProperty]
        public Car Car { get; set; }

        [BindProperty]
        public IFormFile MainImage { get; set; }

        [BindProperty]
        public string SelectedUserId { get; set; }

        public SelectList BrandSelectList { get; set; }
        public SelectList CategorySelectList { get; set; }
        public SelectList UserSelectList { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadSelectLists();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Loại bỏ ModelState validation cho các navigation properties
            ModelState.Remove("Car.User");
            ModelState.Remove("Car.Brand");
            ModelState.Remove("Car.Category");
            ModelState.Remove("Car.Rentals");
            ModelState.Remove("Car.Reviews");
            ModelState.Remove("Car.CarFeatures");
            ModelState.Remove("Car.CarModel3D");
            ModelState.Remove("Car.CarImages");
            ModelState.Remove("Car.UserId"); // Thêm dòng này để loại bỏ validation cho UserId
            ModelState.Remove("SelectedUserId");

            if (string.IsNullOrEmpty(SelectedUserId))
            {
                ModelState.AddModelError("SelectedUserId", "Vui lòng chọn chủ xe");
                await LoadSelectLists();
                return Page();
            }

            // Gán UserId từ form selection
            Car.UserId = SelectedUserId;

            if (string.IsNullOrEmpty(Car.UserId))
            {
                ModelState.AddModelError("SelectedUserId", "Vui lòng chọn chủ xe");
                await LoadSelectLists();
                return Page();
            }

            if (!ModelState.IsValid)
            {
                await LoadSelectLists();
                return Page();
            }

            try
            {
                // Xe do admin tạo sẽ mặc định được phê duyệt
                Car.IsApproved = true;

                // Thêm xe mới
                var newCar = await _carRepository.AddAsync(Car);

                // Xử lý tải lên ảnh (nếu có)
                if (MainImage != null)
                {
                    var imageUrl = await SaveImage(MainImage, newCar.Id);

                    // Thêm ảnh mới vào CarImage
                    var carImage = new CarImage
                    {
                        CarId = newCar.Id,
                        ImageUrl = imageUrl,
                        IsMainImage = true
                    };

                    await _carRepository.AddCarImageAsync(newCar.Id, carImage);
                }

                TempData["SuccessMessage"] = "Thêm xe mới thành công!";

                return RedirectToPage("/Cars/Index", new { area = "Admin" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm xe mới");
                ModelState.AddModelError(string.Empty, $"Đã xảy ra lỗi khi thêm xe: {ex.Message}");

                await LoadSelectLists();
                return Page();
            }
        }

        private async Task LoadSelectLists()
        {
            // Danh sách hãng xe
            var brands = await _brandRepository.GetAllAsync();
            BrandSelectList = new SelectList(brands, "Id", "Name");

            // Danh sách loại xe
            var categories = await _categoryRepository.GetAllAsync();
            CategorySelectList = new SelectList(categories, "Id", "Name");

            // Danh sách người dùng (chỉ lấy những người có role CarOwner)
            var users = await _userManager.GetUsersInRoleAsync("CarOwner");
            UserSelectList = new SelectList(users, "Id", "FullName", null, "Email");
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
