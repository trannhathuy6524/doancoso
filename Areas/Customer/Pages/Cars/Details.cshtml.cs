﻿using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace GotoCarRental.Areas.Customer.Pages.Cars
{
    public class DetailsModel : PageModel
    {
        private readonly ICarRepository _carRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IReviewRepository _reviewRepository;

        public DetailsModel(ICarRepository carRepository, UserManager<ApplicationUser> userManager, IReviewRepository reviewRepository)
        {
            _carRepository = carRepository;
            _userManager = userManager;
            _reviewRepository = reviewRepository;
        }

        public Car Car { get; set; }
        public IEnumerable<Feature> Features { get; set; }
        public IEnumerable<Review> Reviews { get; set; }
        public IEnumerable<Car> RelatedCars { get; set; }
        public double AverageRating { get; set; }
        public bool IsLoggedIn { get; set; }
        public bool HasRented { get; set; }
        public bool HasUserReviewed { get; set; }


        [BindProperty]
        public DateTime? StartDate { get; set; }

        [BindProperty]
        public DateTime? EndDate { get; set; }

        [BindProperty]
        public int Days { get; set; } = 1;

        [BindProperty]
        public decimal TotalPrice { get; set; }
        public Rental.RentalType RentalType { get; set; } = Rental.RentalType.ByDay;
        public DateTime? RentalDate { get; set; } = DateTime.Today;
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public int Hours { get; set; } = 1;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Car = await _carRepository.GetByIdAsync(id.Value);
            if (Car == null || !Car.IsApproved || !Car.IsAvailable)
            {
                return NotFound();
            }

            // Lấy các tính năng của xe
            Features = await _carRepository.GetCarFeaturesAsync(id.Value);

            // Lấy đánh giá
            Reviews = await _carRepository.GetCarReviewsAsync(id.Value);
            AverageRating = await _carRepository.GetCarAverageRatingAsync(id.Value);

            // Lấy xe liên quan có chung tính năng
            RelatedCars = await _carRepository.GetRelatedCarsAsync(id.Value, 4);

            // Kiểm tra người dùng
            IsLoggedIn = User.Identity.IsAuthenticated;
            if (IsLoggedIn)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Kiểm tra xem người dùng đã từng thuê xe này chưa
                HasRented = Car.Rentals != null && Car.Rentals.Any(r => r.UserId == userId && (r.Status == 1 || r.Status == 3));

                // Kiểm tra xem người dùng đã đánh giá xe này chưa
                HasUserReviewed = Car.Reviews != null && Car.Reviews.Any(r => r.UserId == userId);
            }

            // Các giá trị mặc định cho đặt xe
            StartDate = DateTime.Today;
            EndDate = DateTime.Today.AddDays(1);
            Days = (int)(EndDate.Value - StartDate.Value).TotalDays + 1; // Thêm +1 ở đây
            TotalPrice = Car.PricePerDay * Days;

            return Page();
        }

        public async Task<IActionResult> OnPostCalculateAsync(int id, Rental.RentalType rentalType)
        {
            RentalType = rentalType;
            Car = await _carRepository.GetByIdAsync(id);

            if (Car == null)
            {
                return NotFound();
            }

            if (RentalType == Rental.RentalType.ByDay)
            {
                if (StartDate == null || EndDate == null || StartDate > EndDate)
                {
                    ModelState.AddModelError(string.Empty, "Ngày kết thúc phải sau ngày bắt đầu.");
                    return Page();
                }

                Days = (EndDate.Value - StartDate.Value).Days + 1;
                if (Days < 1) Days = 1;

                TotalPrice = Car.PricePerDay * Days;
            }
            else // ByHour
            {
                if (RentalDate == null || !StartTime.HasValue || !EndTime.HasValue)
                {
                    ModelState.AddModelError(string.Empty, "Vui lòng chọn đầy đủ ngày và giờ thuê.");
                    return Page();
                }

                if (StartTime >= EndTime)
                {
                    ModelState.AddModelError(string.Empty, "Giờ kết thúc phải sau giờ bắt đầu.");
                    return Page();
                }

                var duration = EndTime.Value - StartTime.Value;
                Hours = (int)Math.Ceiling(duration.TotalHours);
                if (Hours < 1) Hours = 1;

                TotalPrice = Car.PricePerHour * Hours;
            }

            // Lấy các thông tin khác
            Features = await _carRepository.GetCarFeaturesAsync(id);
            Reviews = await _carRepository.GetCarReviewsAsync(id);
            AverageRating = await _carRepository.GetCarAverageRatingAsync(id);
            RelatedCars = await _carRepository.GetRelatedCarsAsync(id, 4);
            IsLoggedIn = User.Identity.IsAuthenticated;

            return Page();
        }
    }
}
