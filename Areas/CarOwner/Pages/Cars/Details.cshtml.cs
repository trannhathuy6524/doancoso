using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace GotoCarRental.Areas.CarOwner.Pages.Cars
{
    [Authorize(Roles = "CarOwner")]
    public class DetailsModel : PageModel
    {
        private readonly ICarRepository _carRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(
            ICarRepository carRepository,
            UserManager<ApplicationUser> userManager,
            ILogger<DetailsModel> logger)
        {
            _carRepository = carRepository;
            _userManager = userManager;
            _logger = logger;
        }

        public Car Car { get; set; }
        public IEnumerable<CarImage> CarImages { get; set; }
        public IEnumerable<Feature> Features { get; set; }
        public CarModel3D Model3D { get; set; }
        public IEnumerable<Review> Reviews { get; set; }
        public double AverageRating { get; set; }
        public bool IsOwner { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Lấy thông tin xe
            Car = await _carRepository.GetByIdAsync(id.Value);
            if (Car == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền sở hữu
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            IsOwner = (userId == Car.UserId);

            // Nếu không phải chủ xe, không cho xem
            if (!IsOwner)
            {
                return Forbid();
            }

            // Lấy hình ảnh
            CarImages = await _carRepository.GetCarImagesAsync(id.Value);

            // Lấy tính năng
            Features = await _carRepository.GetCarFeaturesAsync(id.Value);

            // Lấy model 3D nếu có
            Model3D = await _carRepository.GetCar3DModelAsync(id.Value);

            // Lấy đánh giá
            Reviews = await _carRepository.GetCarReviewsAsync(id.Value);

            // Lấy điểm đánh giá trung bình
            AverageRating = await _carRepository.GetCarAverageRatingAsync(id.Value);

            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            // Người dùng CarOwner không có quyền duyệt xe
            return Forbid();
        }
    }
}
