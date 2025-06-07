using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GotoCarRental.Areas.Admin.Pages.Cars
{
    [Authorize(Roles = "Admin,Manager")]
    public class DetailsModel : PageModel
    {
        private readonly ICarRepository _carRepository;

        public DetailsModel(ICarRepository carRepository)
        {
            _carRepository = carRepository;
        }

        public Car Car { get; set; }
        public IEnumerable<CarImage> CarImages { get; set; }
        public IEnumerable<Review> Reviews { get; set; }
        public IEnumerable<Feature> Features { get; set; }
        public CarModel3D Model3D { get; set; }
        public double AverageRating { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Car = await _carRepository.GetByIdAsync(id);
            if (Car == null)
            {
                return NotFound();
            }

            // Lấy các thông tin chi tiết về xe
            CarImages = await _carRepository.GetCarImagesAsync(id);
            Reviews = await _carRepository.GetCarReviewsAsync(id);
            AverageRating = await _carRepository.GetCarAverageRatingAsync(id);
            Features = await _carRepository.GetCarFeaturesAsync(id);
            Model3D = await _carRepository.GetCar3DModelAsync(id);

            return Page();
        }

        public async Task<IActionResult> OnPostToggleAvailabilityAsync(int id)
        {
            var car = await _carRepository.GetByIdAsync(id);
            if (car == null)
            {
                return NotFound();
            }

            await _carRepository.ToggleAvailabilityAsync(id);

            TempData["SuccessMessage"] = car.IsAvailable
                ? "Xe đã được ẩn thành công!"
                : "Xe đã được hiển thị thành công!";

            return RedirectToPage("./Details", new { id });
        }
        

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var car = await _carRepository.GetByIdAsync(id);
            if (car == null)
            {
                return NotFound();
            }

            // Gọi DeleteAsync thay vì DeleteSafelyAsync vì admin có quyền xóa bất kỳ xe nào
            await _carRepository.DeleteAsync(id);

            TempData["SuccessMessage"] = "Xe đã được xóa thành công!";
            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostToggleApprovalAsync(int id)
        {
            var car = await _carRepository.GetByIdAsync(id);
            if (car == null)
            {
                return NotFound();
            }

            await _carRepository.ToggleApprovalAsync(id);

            TempData["SuccessMessage"] = car.IsApproved
                ? "Xe đã bị hủy phê duyệt thành công!"
                : "Xe đã được phê duyệt thành công!";

            return RedirectToPage("./Details", new { id });
        }
    }
}
