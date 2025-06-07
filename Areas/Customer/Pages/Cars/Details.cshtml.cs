using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using static GotoCarRental.Models.Rental;

namespace GotoCarRental.Areas.Customer.Pages.Cars
{
    [AllowAnonymous]
    public class DetailsModel : PageModel
    {
        private readonly ICarRepository _carRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IReviewRepository _reviewRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DetailsModel(
            ICarRepository carRepository,
            UserManager<ApplicationUser> userManager,
            IReviewRepository reviewRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _carRepository = carRepository;
            _userManager = userManager;
            _reviewRepository = reviewRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public Car Car { get; set; }
        public IEnumerable<Feature> Features { get; set; } = new List<Feature>();
        public IEnumerable<Review> Reviews { get; set; } = new List<Review>();
        public IEnumerable<Car> RelatedCars { get; set; } = new List<Car>();
        public double AverageRating { get; set; }
        public bool IsLoggedIn { get; set; }
        public bool HasRented { get; set; }
        public bool HasUserReviewed { get; set; }
        public int TotalReviews { get; set; }

        // Thêm các thống kê đánh giá
        public Dictionary<int, int> RatingCounts { get; set; } = new Dictionary<int, int>();
        public Dictionary<int, int> RatingPercentages { get; set; } = new Dictionary<int, int>();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Đảm bảo phiên làm việc được gia hạn khi người dùng đã đăng nhập
            if (User.Identity.IsAuthenticated)
            {
                // Làm mới phiên đăng nhập
                try
                {
                    if (_httpContextAccessor.HttpContext.Request.Cookies.ContainsKey(".AspNetCore.Session"))
                    {
                        _httpContextAccessor.HttpContext.Response.Cookies.Append(
                            ".AspNetCore.Session",
                            _httpContextAccessor.HttpContext.Request.Cookies[".AspNetCore.Session"],
                            new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = _httpContextAccessor.HttpContext.Request.IsHttps,
                                IsEssential = true,
                                SameSite = SameSiteMode.Lax,
                                Expires = DateTime.Now.AddMinutes(30)
                            });
                    }
                }
                catch (Exception ex)
                {
                    // Bỏ qua lỗi khi làm mới phiên để tránh ảnh hưởng đến trải nghiệm người dùng
                    Console.WriteLine($"Lỗi khi làm mới phiên đăng nhập: {ex.Message}");
                }
            }

            Car = await _carRepository.GetByIdAsync(id.Value);
            if (Car == null || !Car.IsApproved || !Car.IsAvailable)
            {
                return NotFound();
            }

            try
            {
                // Lấy các tính năng của xe
                Features = await _carRepository.GetCarFeaturesAsync(id.Value) ?? new List<Feature>();

                // Lấy đánh giá
                Reviews = await _carRepository.GetCarReviewsAsync(id.Value) ?? new List<Review>();
                TotalReviews = Reviews.Count();
                AverageRating = await _carRepository.GetCarAverageRatingAsync(id.Value);

                // Tính toán thống kê đánh giá
                RatingCounts = new Dictionary<int, int>();
                RatingPercentages = new Dictionary<int, int>();

                for (int i = 1; i <= 5; i++)
                {
                    int count = Reviews.Count(r => r.Rating == i);
                    RatingCounts[i] = count;
                    RatingPercentages[i] = TotalReviews > 0 ? (count * 100 / TotalReviews) : 0;
                }

                // Lấy xe liên quan có chung tính năng
                RelatedCars = await _carRepository.GetRelatedCarsAsync(id.Value, 4) ?? new List<Car>();

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
            }
            catch (Exception ex)
            {
                // Log lỗi nếu có
                Console.WriteLine($"Lỗi khi lấy dữ liệu: {ex.Message}");

                // Đảm bảo các thuộc tính không bị null
                Features = Features ?? new List<Feature>();
                Reviews = Reviews ?? new List<Review>();
                RelatedCars = RelatedCars ?? new List<Car>();
            }

            return Page();
        }

        // API endpoint để giữ phiên làm việc
        public IActionResult OnGetKeepAlive()
        {
            if (User.Identity.IsAuthenticated)
            {
                // Làm mới phiên đăng nhập
                try
                {
                    if (_httpContextAccessor.HttpContext.Request.Cookies.ContainsKey(".AspNetCore.Session"))
                    {
                        _httpContextAccessor.HttpContext.Response.Cookies.Append(
                            ".AspNetCore.Session",
                            _httpContextAccessor.HttpContext.Request.Cookies[".AspNetCore.Session"],
                            new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = _httpContextAccessor.HttpContext.Request.IsHttps,
                                IsEssential = true,
                                SameSite = SameSiteMode.Lax,
                                Expires = DateTime.Now.AddMinutes(30)
                            });
                    }
                }
                catch (Exception)
                {
                    // Bỏ qua lỗi
                }

                // Tạo một response để gia hạn phiên
                return new JsonResult(new { success = true, timestamp = DateTime.Now });
            }

            return new JsonResult(new { success = false });
        }

        // Helper để tránh lỗi null khi serialize trong view
        public string SafeSerialize(object obj)
        {
            if (obj == null)
            {
                return "\"\"";
            }

            try
            {
                return System.Text.Json.JsonSerializer.Serialize(obj);
            }
            catch
            {
                return "\"\"";
            }
        }
    }
}