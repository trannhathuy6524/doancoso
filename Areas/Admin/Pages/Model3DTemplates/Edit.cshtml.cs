using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GotoCarRental.Areas.Admin.Pages.Model3DTemplates
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly IModel3DTemplateRepository _model3DTemplateRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EditModel(
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
        public IFormFile? Model3DFile { get; set; }

        [BindProperty]
        public IFormFile? PreviewImage { get; set; }

        public SelectList BrandSelectList { get; set; }
        public SelectList CategorySelectList { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Model3DTemplate = await _model3DTemplateRepository.GetByIdAsync(id);

            if (Model3DTemplate == null)
            {
                return NotFound();
            }

            await LoadSelectLists();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Remove các navigation properties để tránh validation error
            ModelState.Remove("Model3DTemplate.Brand");
            ModelState.Remove("Model3DTemplate.Category");

            // Kiểm tra kích thước và định dạng file nếu có
            if (Model3DFile != null)
            {
                if (Model3DFile.Length > 10 * 1024 * 1024) // 10MB
                {
                    ModelState.AddModelError("Model3DFile", "Kích thước file mô hình không được vượt quá 10MB");
                }

                var allowedModelExtensions = new[] { ".glb", ".gltf" };
                var modelExtension = Path.GetExtension(Model3DFile.FileName).ToLower();
                if (!allowedModelExtensions.Contains(modelExtension))
                {
                    ModelState.AddModelError("Model3DFile", "Chỉ chấp nhận file có định dạng GLB hoặc GLTF");
                }
            }

            if (PreviewImage != null)
            {
                if (PreviewImage.Length > 5 * 1024 * 1024) // 5MB
                {
                    ModelState.AddModelError("PreviewImage", "Kích thước hình ảnh xem trước không được vượt quá 5MB");
                }

                var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var imageExtension = Path.GetExtension(PreviewImage.FileName).ToLower();
                if (!allowedImageExtensions.Contains(imageExtension))
                {
                    ModelState.AddModelError("PreviewImage", "Chỉ chấp nhận file hình ảnh có định dạng JPG hoặc PNG");
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadSelectLists();
                return Page();
            }

            // Lấy mô hình hiện tại từ database để có thông tin không bị bind
            var existingModel = await _model3DTemplateRepository.GetByIdAsync(Model3DTemplate.Id);
            if (existingModel == null)
            {
                return NotFound();
            }

            try
            {
                // Xử lý upload files mới (nếu có)
                if (Model3DFile != null || PreviewImage != null)
                {
                    // Xác định thư mục lưu trữ từ URL hiện tại
                    var currentModelUrl = existingModel.FileUrl;
                    var folderPath = GetFolderPathFromUrl(currentModelUrl);
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, folderPath);

                    // Tạo thư mục nếu chưa tồn tại (trường hợp hiếm gặp)
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Xử lý file mô hình 3D mới
                    if (Model3DFile != null)
                    {
                        // Xóa file cũ nếu tồn tại
                        DeleteFileIfExists(_webHostEnvironment.WebRootPath + existingModel.FileUrl);

                        // Lưu file mới
                        var modelExtension = Path.GetExtension(Model3DFile.FileName).ToLower();
                        var uniqueFileName = Guid.NewGuid().ToString() + modelExtension;
                        var modelFilePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(modelFilePath, FileMode.Create))
                        {
                            await Model3DFile.CopyToAsync(fileStream);
                        }
                        Model3DTemplate.FileUrl = $"/{folderPath}/{uniqueFileName}";
                    }
                    else
                    {
                        Model3DTemplate.FileUrl = existingModel.FileUrl;
                    }

                    // Xử lý file hình ảnh xem trước mới
                    if (PreviewImage != null)
                    {
                        // Xóa file cũ nếu tồn tại
                        DeleteFileIfExists(_webHostEnvironment.WebRootPath + existingModel.PreviewImageUrl);

                        // Lưu file mới
                        var imageExtension = Path.GetExtension(PreviewImage.FileName).ToLower();
                        var uniqueImageName = Guid.NewGuid().ToString() + imageExtension;
                        var imageFilePath = Path.Combine(uploadsFolder, uniqueImageName);
                        using (var fileStream = new FileStream(imageFilePath, FileMode.Create))
                        {
                            await PreviewImage.CopyToAsync(fileStream);
                        }
                        Model3DTemplate.PreviewImageUrl = $"/{folderPath}/{uniqueImageName}";
                    }
                    else
                    {
                        Model3DTemplate.PreviewImageUrl = existingModel.PreviewImageUrl;
                    }
                }

                // Giữ nguyên thời gian tạo
                Model3DTemplate.CreatedAt = existingModel.CreatedAt;

                // Cập nhật vào database
                await _model3DTemplateRepository.UpdateAsync(Model3DTemplate);

                TempData["SuccessMessage"] = "Cập nhật mô hình 3D thành công!";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Có lỗi xảy ra: {ex.Message}");
                await LoadSelectLists();
                return Page();
            }
        }

        private async Task LoadSelectLists()
        {
            var brands = await _brandRepository.GetAllAsync();
            BrandSelectList = new SelectList(brands, "Id", "Name", Model3DTemplate?.BrandId);

            var categories = await _categoryRepository.GetAllAsync();
            CategorySelectList = new SelectList(categories, "Id", "Name", Model3DTemplate?.CategoryId);
        }

        private string GetFolderPathFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return $"models/3d/model3d_{DateTime.Now.ToString("yyyyMMddHHmmss")}";
            }

            // Loại bỏ dấu / đầu tiên nếu có
            if (url.StartsWith("/"))
            {
                url = url.Substring(1);
            }

            // Lấy đường dẫn thư mục từ URL
            return Path.GetDirectoryName(url).Replace('\\', '/');
        }

        private void DeleteFileIfExists(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    System.IO.File.Delete(filePath);
                }
                catch
                {
                    // Bỏ qua lỗi khi không thể xóa file
                }
            }
        }
    }
}
