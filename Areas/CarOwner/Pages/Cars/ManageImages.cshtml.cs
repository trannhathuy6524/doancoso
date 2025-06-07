using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using GotoCarRental.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System;

namespace GotoCarRental.Areas.CarOwner.Pages.Cars
{
    [Authorize(Roles = "CarOwner")]
    public class ManageImagesModel : PageModel
    {
        private readonly ICarRepository _carRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ManageImagesModel(
            ICarRepository carRepository,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _carRepository = carRepository;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public Car Car { get; set; }
        public List<CarImage> Images { get; set; } = new List<CarImage>();
        [BindProperty]
        public List<IFormFile> NewImages { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = _userManager.GetUserId(User);
            Car = await _carRepository.GetByIdAsync(id);

            if (Car == null || Car.UserId != userId)
            {
                return NotFound();
            }

            Images = Car.CarImages?.ToList() ?? new List<CarImage>();
            return Page();
        }

        public async Task<IActionResult> OnPostUploadAsync(int id)
        {
            var userId = _userManager.GetUserId(User);
            Car = await _carRepository.GetByIdAsync(id);

            if (Car == null || Car.UserId != userId)
            {
                return NotFound();
            }

            if (NewImages != null && NewImages.Count > 0)
            {
                var uploadsFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "cars", id.ToString());
                Directory.CreateDirectory(uploadsFolderPath);

                foreach (var image in NewImages)
                {
                    if (image.Length > 0)
                    {
                        // Tạo tên file độc nhất
                        string fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                        string filePath = Path.Combine(uploadsFolderPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        // Lưu thông tin hình ảnh vào database
                        var carImage = new CarImage
                        {
                            CarId = id,
                            ImageUrl = $"/uploads/cars/{id}/{fileName}",
                            IsMainImage = Car.CarImages == null || !Car.CarImages.Any()
                        };

                        await _carRepository.AddImageAsync(carImage);
                    }
                }

                TempData["SuccessMessage"] = "Đã tải lên hình ảnh mới thành công!";
            }

            return RedirectToPage("/Cars/ManageImages", new { id = id });
        }

        public async Task<IActionResult> OnPostSetMainImageAsync(int id, int imageId)
        {
            var userId = _userManager.GetUserId(User);
            Car = await _carRepository.GetByIdAsync(id);

            if (Car == null || Car.UserId != userId)
            {
                return NotFound();
            }

            await _carRepository.SetMainImageAsync(id, imageId);
            TempData["SuccessMessage"] = "Đã đặt ảnh chính thành công!";

            return RedirectToPage("/Cars/ManageImages", new { id = id });
        }

        public async Task<IActionResult> OnPostDeleteImageAsync(int id, int imageId)
        {
            var userId = _userManager.GetUserId(User);
            Car = await _carRepository.GetByIdAsync(id);

            if (Car == null || Car.UserId != userId)
            {
                return NotFound();
            }

            var image = Car.CarImages.FirstOrDefault(i => i.Id == imageId);
            if (image != null)
            {
                // Xóa file vật lý nếu tồn tại
                if (!string.IsNullOrEmpty(image.ImageUrl))
                {
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, image.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                // Xóa từ database
                await _carRepository.DeleteImageAsync(imageId);
                TempData["SuccessMessage"] = "Đã xóa hình ảnh thành công!";
            }

            return RedirectToPage("/Cars/ManageImages", new { id = id });
        }
    }
}