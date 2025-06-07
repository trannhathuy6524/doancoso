using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace GotoCarRental.Areas.Customer.Pages.Reviews
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly ICarRepository _carRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateModel(
            IReviewRepository reviewRepository,
            ICarRepository carRepository,
            UserManager<ApplicationUser> userManager)
        {
            _reviewRepository = reviewRepository;
            _carRepository = carRepository;
            _userManager = userManager;
        }

        [BindProperty]
        public int CarId { get; set; }

        public Car Car { get; set; }

        [BindProperty]
        public ReviewInputModel Review { get; set; }

        public async Task<IActionResult> OnGetAsync(int carId)
        {
            CarId = carId;
            var userId = _userManager.GetUserId(User);

            // Khởi tạo Review với Rating mặc định là 0 (không chọn)
            Review = new ReviewInputModel { CarId = carId, Rating = 0 };

            // Phần code còn lại giữ nguyên
            Car = await _carRepository.GetByIdAsync(CarId);
            if (Car == null)
            {
                return NotFound();
            }

            var hasReviewed = await _reviewRepository.HasUserReviewedCarAsync(userId, CarId);
            if (hasReviewed)
            {
                TempData["ErrorMessage"] = "Bạn đã đánh giá xe này rồi.";
                return RedirectToPage("/Cars/Details", new { area = "Customer", id = carId });
            }

            return Page();
        }


        public async Task<IActionResult> OnPostAsync(int carId)
        {
            CarId = carId;

            if (!ModelState.IsValid)
            {
                Car = await _carRepository.GetByIdAsync(CarId);
                return Page();
            }

            var userId = _userManager.GetUserId(User);

            // Kiểm tra xem người dùng đã đánh giá xe này chưa
            var hasReviewed = await _reviewRepository.HasUserReviewedCarAsync(userId, CarId);
            if (hasReviewed)
            {
                TempData["ErrorMessage"] = "Bạn đã đánh giá xe này rồi.";
                return RedirectToPage("/Cars/Details", new { area = "Customer", id = CarId });
            }

            // Tạo đánh giá mới
            var review = new Review
            {
                CarId = CarId,
                UserId = userId,
                Rating = Review.Rating,
                Comment = Review.Comment,
                CreatedAt = DateTime.UtcNow
            };

            await _reviewRepository.AddAsync(review);

            TempData["SuccessMessage"] = "Cảm ơn bạn đã đánh giá!";
            return RedirectToPage("/Cars/Details", new { area = "Customer", id = CarId });
        }
    }

    public class ReviewInputModel
    {
        public int CarId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn đánh giá")]
        [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 sao")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nhận xét")]
        [StringLength(1000, MinimumLength = 5, ErrorMessage = "Nhận xét phải từ 5 đến 1000 ký tự")]
        public string Comment { get; set; }
    }
}
