using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GotoCarRental.Areas.CarOwner.Pages.Debug
{
    [Authorize(Roles = "CarOwner")]
    public class CheckFileModel : PageModel
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CheckFileModel(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        [BindProperty(SupportsGet = true)]
        public string FilePath { get; set; }

        public string WebRootPath { get; set; }
        public string FullPath { get; set; }
        public bool FileExists { get; set; }
        public string ErrorMessage { get; set; }
        public long? FileSize { get; set; }
        public DateTime? LastModified { get; set; }
        public string ContentType { get; set; }

        public void OnGet()
        {
            if (string.IsNullOrEmpty(FilePath))
            {
                return;
            }

            try
            {
                WebRootPath = _webHostEnvironment.WebRootPath;

                // Loại bỏ dấu / đầu tiên nếu có
                if (FilePath.StartsWith("/"))
                {
                    FilePath = FilePath.Substring(1);
                }

                FullPath = Path.Combine(WebRootPath, FilePath);
                FileExists = System.IO.File.Exists(FullPath);

                if (FileExists)
                {
                    var fileInfo = new FileInfo(FullPath);
                    FileSize = fileInfo.Length;
                    LastModified = fileInfo.LastWriteTime;

                    // Xác định content type
                    var ext = Path.GetExtension(FilePath).ToLower();
                    ContentType = ext switch
                    {
                        ".glb" => "model/gltf-binary",
                        ".gltf" => "model/gltf+json",
                        ".jpg" or ".jpeg" => "image/jpeg",
                        ".png" => "image/png",
                        _ => "application/octet-stream"
                    };
                }
                else
                {
                    var directory = Path.GetDirectoryName(FullPath);
                    if (!Directory.Exists(directory))
                    {
                        ErrorMessage = $"Thư mục không tồn tại: {directory}";
                    }
                    else
                    {
                        ErrorMessage = $"File không tồn tại: {FullPath}";
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi kiểm tra file: {ex.Message}";
            }
        }
    }
}
