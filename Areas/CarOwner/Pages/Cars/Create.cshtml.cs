using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace GotoCarRental.Areas.CarOwner.Pages.Cars
{
    [Authorize(Roles = "CarOwner")]
    public class CreateModel : PageModel
    {
        private readonly ICarRepository _carRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFeatureRepository _featureRepository;
        private readonly IProvinceRepository _provinceRepository;


        public CreateModel(
            ICarRepository carRepository,
            IBrandRepository brandRepository,
            ICategoryRepository categoryRepository,
            UserManager<ApplicationUser> userManager,
            IFeatureRepository featureRepository,
            IProvinceRepository provinceRepository)
        {
            _carRepository = carRepository;
            _brandRepository = brandRepository;
            _categoryRepository = categoryRepository;
            _userManager = userManager;
            _featureRepository = featureRepository;
            _provinceRepository = provinceRepository;
        }

        public SelectList BrandSelectList { get; set; }
        public SelectList CategorySelectList { get; set; }
        public MultiSelectList FeaturesSelectList { get; set; }
        public SelectList ProvinceSelectList { get; set; }

        [BindProperty]
        public Car Car { get; set; }

        [BindProperty]
        public IFormFile MainImage { get; set; }

        [BindProperty]
        public List<int> SelectedFeatureIds { get; set; } = new List<int>(); // Danh sách ID tính năng được chọn


        [BindProperty]
        public int? SelectedModel3DTemplateId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Lấy danh sách hãng xe, loại xe và tính năng cho dropdown
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
            ModelState.Remove("MainImage"); // Không yêu cầu ảnh chính
                                            // Bổ sung thêm dòng này vào phần loại bỏ ModelState validation
            ModelState.Remove("Car.Province");


            // Lấy user ID hiện tại
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Gán user ID cho xe mới
            Car.UserId = user.Id;
            Car.IsAvailable = true;
            Car.IsApproved = false; // Xe mới tạo cần được admin phê duyệt

            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        // Ghi log lỗi
                        Console.WriteLine($"Field: {state.Key}, Error: {error.ErrorMessage}");
                    }
                }

                await LoadSelectLists();
                return Page();
            }

            try
            {
                Console.WriteLine("Bắt đầu thêm xe mới...");
                Console.WriteLine($"Car Name: {Car.Name}, BrandId: {Car.BrandId}, CategoryId: {Car.CategoryId}");
                Console.WriteLine($"SelectedModel3DTemplateId: {SelectedModel3DTemplateId}");


                // Thêm xe mới
                var newCar = await _carRepository.AddAsync(Car);
                Console.WriteLine($"Đã thêm xe với ID: {newCar.Id}");


                // Xử lý mô hình 3D nếu có
                if (SelectedModel3DTemplateId.HasValue)
                {
                    Console.WriteLine($"Bắt đầu thêm mô hình 3D với ID: {SelectedModel3DTemplateId.Value}");
                    try
                    {
                        await _carRepository.AddCarModel3DAsync(newCar.Id, SelectedModel3DTemplateId.Value);
                        Console.WriteLine("Thêm mô hình 3D thành công");
                    }
                    catch (Exception ex)
                    {
                        // Ghi log lỗi nhưng không dừng quy trình
                        Console.WriteLine($"Lỗi khi thêm mô hình 3D: {ex.Message}");
                        TempData["WarningMessage"] = "Đã thêm xe thành công nhưng không thể thêm mô hình 3D. Bạn có thể cập nhật mô hình 3D sau.";
                        // Không throw exception ở đây, tiếp tục xử lý
                    }
                }

                // Xử lý tải lên ảnh (nếu có)
                if (MainImage != null)
                {
                    // Xử lý lưu ảnh vào thư mục và tạo đường dẫn
                    var imageUrl = await SaveImage(MainImage, newCar.Id);

                    // Thêm ảnh mới vào CarImage
                    var carImage = new CarImage
                    {
                        CarId = newCar.Id,
                        ImageUrl = imageUrl,
                        IsMainImage = true
                    };

                    // Thêm ảnh vào database
                    await _carRepository.AddCarImageAsync(newCar.Id, carImage);
                }

                // Thêm tính năng xe (nếu có)
                if (SelectedFeatureIds != null && SelectedFeatureIds.Any())
                {
                    foreach (var featureId in SelectedFeatureIds)
                    {
                        await _carRepository.AddFeatureToCarAsync(newCar.Id, featureId);
                    }
                }

                TempData["SuccessMessage"] = "Thêm xe mới thành công! Xe của bạn đang chờ quản trị viên phê duyệt.";
                return RedirectToPage("/MyCars", new { area = "CarOwner" });
            }
            catch (Exception ex)
            {
                // Cải thiện log lỗi chi tiết hơn
                Console.WriteLine($"Lỗi khi thêm xe mới: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Chi tiết lỗi: {ex.InnerException.Message}");
                }
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                // Thông báo lỗi rõ ràng hơn
                ModelState.AddModelError(string.Empty, $"Có lỗi xảy ra khi thêm xe: {ex.Message}");

                // Nếu lỗi liên quan đến thao tác cụ thể, hiển thị thông báo phù hợp
                if (ex.Message.Contains("model3d", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(string.Empty, "Có lỗi khi thêm mô hình 3D. Vui lòng chọn mô hình khác.");
                }
                else if (ex.Message.Contains("image", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(string.Empty, "Có lỗi khi tải ảnh lên. Vui lòng kiểm tra ảnh của bạn.");
                }

                await LoadSelectLists();
                return Page();
            }
        }

        private async Task LoadSelectLists()
        {
            var brands = await _brandRepository.GetAllAsync();
            BrandSelectList = new SelectList(brands, "Id", "Name");

            var categories = await _categoryRepository.GetAllAsync();
            CategorySelectList = new SelectList(categories, "Id", "Name");

            // Lấy danh sách tính năng
            var features = await _featureRepository.GetAllAsync();
            FeaturesSelectList = new MultiSelectList(features, "Id", "Name");

            var provinces = await _provinceRepository.GetAllAsync();
            ProvinceSelectList = new SelectList(provinces, "Id", "Name");
        }

        private async Task<string> SaveImage(IFormFile image, int carId)
        {
            if (image == null || image.Length == 0)
            {
                throw new ArgumentException("File ảnh không hợp lệ hoặc trống");
            }

            try
            {
                // Kiểm tra định dạng file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    throw new ArgumentException($"Chỉ chấp nhận các định dạng ảnh: {string.Join(", ", allowedExtensions)}");
                }

                // Tạo đường dẫn thư mục gốc
                var basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "cars");
                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                    Console.WriteLine($"Đã tạo thư mục gốc: {basePath}");
                }

                // Tạo đường dẫn thư mục cho xe cụ thể
                var uploadsFolder = Path.Combine(basePath, carId.ToString());
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                    Console.WriteLine($"Đã tạo thư mục cho xe: {uploadsFolder}");
                }

                // Kiểm tra quyền ghi
                try
                {
                    var testFile = Path.Combine(uploadsFolder, "test.txt");
                    using (var fs = new FileStream(testFile, FileMode.Create))
                    {
                        fs.WriteByte(1);
                    }
                    
                   System.IO.File.Delete(testFile);
                }
                catch (Exception ex)
                {
                    throw new IOException($"Không có quyền ghi vào thư mục: {uploadsFolder}. Chi tiết: {ex.Message}");
                }

                // Tạo tên file ảnh độc nhất
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(image.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Lưu file ảnh với kiểm tra
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }

                // Kiểm tra file đã được tạo thành công
                if (!System.IO.File.Exists(filePath))
                {
                    throw new IOException($"Không thể tạo file ảnh tại: {filePath}");
                }

                // Kiểm tra kích thước file để đảm bảo nó đã được ghi đầy đủ
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Length == 0)
                {
                    throw new IOException("File ảnh được tạo nhưng không có dữ liệu");
                }

                Console.WriteLine($"Đã lưu ảnh thành công: {filePath}, kích thước: {fileInfo.Length} bytes");

                // Trả về đường dẫn tương đối của ảnh
                return $"/images/cars/{carId}/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lưu ảnh: {ex.Message}");
                throw new Exception($"Lỗi khi lưu ảnh: {ex.Message}", ex);
            }
        }


        // Thêm handler để lấy danh sách mô hình 3D
        public async Task<IActionResult> OnGetModel3DsAsync(int brandId, int categoryId)
        {
            try
            {
                Console.WriteLine($"Đang tải mô hình 3D cho brandId={brandId}, categoryId={categoryId}");
                // Lấy danh sách mô hình 3D phù hợp với Brand và Category
                var templates = await _carRepository.GetModel3DTemplatesAsync(brandId, categoryId);

                if (templates == null || !templates.Any())
                {
                    Console.WriteLine("Không tìm thấy mô hình 3D phù hợp");
                    return new JsonResult(new List<object>());
                }

                Console.WriteLine($"Tìm thấy {templates.Count()} mô hình 3D");

                // Kiểm tra từng mô hình
                var result = templates.Select(t =>
                {
                    // Kiểm tra đường dẫn file
                    var fileExists = System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", t.FileUrl.TrimStart('/')));
                    var previewExists = System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", t.PreviewImageUrl.TrimStart('/')));

                    return new
                    {
                        id = t.Id,
                        name = t.Name,
                        previewUrl = t.PreviewImageUrl,
                        description = t.Description,
                        fileExists = fileExists,
                        previewExists = previewExists
                    };
                }).ToList();

                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tải mô hình 3D: {ex.Message}");
                return new JsonResult(new { error = ex.Message });
            }
        }


    }
}
