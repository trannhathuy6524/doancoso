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
    public class EditModel : PageModel
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly ICarRepository _carRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public EditModel(
            IReviewRepository reviewRepository,
            ICarRepository carRepository,
            UserManager<ApplicationUser> userManager)
        {
            _reviewRepository = reviewRepository;
            _carRepository = carRepository;
            _userManager = userManager;
        }

        [BindProperty]
        public ReviewEditModel Review { get; set; }

        public Car Car { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = _userManager.GetUserId(User);
            var review = await _reviewRepository.GetByIdAsync(id);

            if (review == null)
            {
                return NotFound();
            }

            // Kiểm tra xem người dùng có quyền chỉnh sửa đánh giá không
            if (review.UserId != userId)
            {
                return Forbid();
            }

            Car = await _carRepository.GetByIdAsync(review.CarId);

            Review = new ReviewEditModel
            {
                Id = review.Id,
                CarId = review.CarId,
                Rating = review.Rating,
                Comment = review.Comment
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Car = await _carRepository.GetByIdAsync(Review.CarId);
                return Page();
            }

            var userId = _userManager.GetUserId(User);
            var review = await _reviewRepository.GetByIdAsync(Review.Id);

            if (review == null)
            {
                return NotFound();
            }

            // Kiểm tra xem người dùng có quyền chỉnh sửa đánh giá không
            if (review.UserId != userId)
            {
                return Forbid();
            }

            // Cập nhật đánh giá
            review.Rating = Review.Rating;
            review.Comment = Review.Comment;

            await _reviewRepository.UpdateAsync(review);

            TempData["SuccessMessage"] = "Đánh giá đã được cập nhật!";
            return RedirectToPage("/Reviews/Index");
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            var userId = _userManager.GetUserId(User);
            var review = await _reviewRepository.GetByIdAsync(Review.Id);

            if (review == null)
            {
                return NotFound();
            }

            // Kiểm tra xem người dùng có quyền xóa đánh giá không
            if (review.UserId != userId)
            {
                return Forbid();
            }

            await _reviewRepository.DeleteAsync(Review.Id);

            TempData["SuccessMessage"] = "Đánh giá đã được xóa!";
            return RedirectToPage("/Reviews/Index");
        }
    }

    public class ReviewEditModel
    {
        public int Id { get; set; }

        public int CarId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn đánh giá")]
        [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 sao")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nhận xét")]
        [StringLength(1000, MinimumLength = 5, ErrorMessage = "Nhận xét phải từ 5 đến 1000 ký tự")]
        public string Comment { get; set; }
    }
}
