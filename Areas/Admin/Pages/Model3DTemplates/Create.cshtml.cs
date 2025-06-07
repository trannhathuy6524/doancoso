using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GotoCarRental.Areas.Admin.Pages.Model3DTemplates
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly IModel3DTemplateRepository _model3DTemplateRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CreateModel(
            IModel3DTemplateRepository model3DTemplateRepository,
            IBrandRepository brandRepository,
            ICategoryRepository categoryRepository,
            IWebHostEnvironment webHostEnvironment)
        {
            _model3DTemplateRepository = model3DTemplateRepository;
            _brandRepository = brandRepository;
            _categoryRepository = categoryRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        [BindProperty]
        public Model3DTemplate Model3DTemplate { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng chọn file mô hình 3D")]
        public IFormFile Model3DFile { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng chọn hình ảnh xem trước")]
        public IFormFile PreviewImage { get; set; }

        public SelectList BrandSelectList { get; set; }
        public SelectList CategorySelectList { get; set; }

        // Thêm vào method OnGetAsync
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var testConnection = await _model3DTemplateRepository.CountAsync();
                Console.WriteLine($"Kết nối database thành công. Số lượng model3D: {testConnection}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi kết nối database: {ex.Message}");
                TempData["ErrorMessage"] = $"Lỗi kết nối cơ sở dữ liệu: {ex.Message}";
            }

            await LoadSelectLists();
            return Page();
        }


        public async Task<IActionResult> OnPostAsync()
        {
            // Remove các navigation properties để tránh validation error
            ModelState.Remove("Model3DTemplate.Brand");
            ModelState.Remove("Model3DTemplate.Category");

            ModelState.Remove("Model3DTemplate.FileUrl");
            ModelState.Remove("Model3DTemplate.PreviewImageUrl");


            // Kiểm tra kích thước file
            if (Model3DFile.Length > 20 * 1024 * 1024) // 20MB
            {
                ModelState.AddModelError("Model3DFile", "Kích thước file mô hình không được vượt quá 20MB");
            }

            if (PreviewImage.Length > 5 * 1024 * 1024) // 5MB
            {
                ModelState.AddModelError("PreviewImage", "Kích thước hình ảnh xem trước không được vượt quá 5MB");
            }

            // Kiểm tra định dạng file
            var allowedModelExtensions = new[] { ".glb", ".gltf" };
            var modelExtension = Path.GetExtension(Model3DFile.FileName).ToLower();
            if (!allowedModelExtensions.Contains(modelExtension))
            {
                ModelState.AddModelError("Model3DFile", "Chỉ chấp nhận file có định dạng GLB hoặc GLTF");
            }

            var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var imageExtension = Path.GetExtension(PreviewImage.FileName).ToLower();
            if (!allowedImageExtensions.Contains(imageExtension))
            {
                ModelState.AddModelError("PreviewImage", "Chỉ chấp nhận file hình ảnh có định dạng JPG hoặc PNG");
            }

            if (!ModelState.IsValid)
            {
                await LoadSelectLists();
                return Page();
            }

            try
            {
                Console.WriteLine("Bắt đầu xử lý tạo mô hình 3D");

                // Tạo tên folder dựa trên timestamp để đảm bảo duy nhất
                var folderName = "model3d_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "models", "3d", folderName);
                Console.WriteLine($"Đường dẫn thư mục: {uploadsFolder}");

                // Tạo thư mục nếu chưa tồn tại
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Lưu file mô hình 3D
                Console.WriteLine("Bắt đầu lưu file mô hình 3D");

                var uniqueFileName = Guid.NewGuid().ToString() + modelExtension;
                var modelFilePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(modelFilePath, FileMode.Create))
                {
                    await Model3DFile.CopyToAsync(fileStream);
                }


                // Kiểm tra file đã được tạo chưa
                if (!System.IO.File.Exists(modelFilePath))
                {
                    ModelState.AddModelError(string.Empty, $"Không thể tạo file mô hình 3D tại đường dẫn: {modelFilePath}");
                    await LoadSelectLists();
                    return Page();
                }

                Model3DTemplate.FileUrl = $"/models/3d/{folderName}/{uniqueFileName}";

                // Lưu file hình ảnh xem trước
                var uniqueImageName = Guid.NewGuid().ToString() + imageExtension;
                var imageFilePath = Path.Combine(uploadsFolder, uniqueImageName);
                using (var fileStream = new FileStream(imageFilePath, FileMode.Create))
                {
                    await PreviewImage.CopyToAsync(fileStream);
                }
                Model3DTemplate.PreviewImageUrl = $"/models/3d/{folderName}/{uniqueImageName}";
                Console.WriteLine($"Đã lưu file hình ảnh xem trước: {Model3DTemplate.PreviewImageUrl}");

                // Đảm bảo đặt giá trị cho các thuộc tính bắt buộc
                Model3DTemplate.CreatedAt = DateTime.UtcNow;
                Model3DTemplate.UpdatedAt = DateTime.UtcNow;

                // Kiểm tra dữ liệu trước khi lưu
                Console.WriteLine($"Kiểm tra dữ liệu Model3DTemplate: Name={Model3DTemplate.Name}, BrandId={Model3DTemplate.BrandId}, CategoryId={Model3DTemplate.CategoryId}, Description={Model3DTemplate.Description?.Substring(0, Math.Min(20, Model3DTemplate.Description?.Length ?? 0))}...");

                // Thêm vào phương thức OnPostAsync trước khi lưu vào database
                // Đảm bảo các trường bắt buộc được gán giá trị
                if (string.IsNullOrEmpty(Model3DTemplate.Name))
                {
                    Model3DTemplate.Name = Path.GetFileNameWithoutExtension(Model3DFile.FileName);
                    Console.WriteLine($"Đặt tên mặc định: {Model3DTemplate.Name}");
                }

                if (string.IsNullOrEmpty(Model3DTemplate.Description))
                {
                    Model3DTemplate.Description = $"Mô hình 3D cho {Model3DTemplate.Name}";
                    Console.WriteLine($"Đặt mô tả mặc định: {Model3DTemplate.Description}");
                }


                // Kiểm tra và tạo thư mục gốc nếu chưa tồn tại
                var basePath = Path.Combine(_webHostEnvironment.WebRootPath, "models", "3d");
                if (!Directory.Exists(basePath))
                {
                    try
                    {
                        Directory.CreateDirectory(basePath);
                        Console.WriteLine($"Đã tạo thư mục gốc: {basePath}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Không thể tạo thư mục gốc: {ex.Message}");
                        ModelState.AddModelError(string.Empty, $"Không thể tạo thư mục lưu trữ: {ex.Message}");
                        await LoadSelectLists();
                        return Page();
                    }
                }



                // Lưu vào database
                Console.WriteLine("Bắt đầu lưu vào database");

                await _model3DTemplateRepository.AddAsync(Model3DTemplate);

                TempData["SuccessMessage"] = "Thêm mô hình 3D thành công!";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                // Log chi tiết lỗi
                Console.WriteLine($"Lỗi: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner stack trace: {ex.InnerException.StackTrace}");
                }

                ModelState.AddModelError(string.Empty, $"Có lỗi xảy ra: {ex.Message}");
                if (ex.InnerException != null)
                {
                    ModelState.AddModelError(string.Empty, $"Chi tiết: {ex.InnerException.Message}");
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
        }


    }
}
