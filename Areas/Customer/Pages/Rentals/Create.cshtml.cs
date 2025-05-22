using GotoCarRental.Models;
using GotoCarRental.Repository;
using GotoCarRental.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using static GotoCarRental.Models.Rental;

namespace GotoCarRental.Areas.Customer.Pages.Rentals
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ICarRepository _carRepository;
        private readonly IRentalRepository _rentalRepository;
        private readonly IGeoLocationService _geoLocationService;


        public CreateModel(
            ICarRepository carRepository,
            IRentalRepository rentalRepository,
            IGeoLocationService geoLocationService)
        {
            _carRepository = carRepository;
            _rentalRepository = rentalRepository;
            _geoLocationService = geoLocationService;
        }

        public Car Car { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(1);

        [BindProperty]
        public int Days { get; set; } = 1;

        [BindProperty]
        public decimal TotalPrice { get; set; }

        [BindProperty]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = "Thanh toán khi nhận xe";

        public bool IsAvailable { get; set; }
        public IEnumerable<DateTime> UnavailableDates { get; set; }
        [BindProperty]
        public RentalType RentalType { get; set; } = RentalType.ByDay;

        [BindProperty]
        public DateTime? RentalDate { get; set; } = DateTime.Today;

        [BindProperty]
        public TimeSpan? StartTime { get; set; } = new TimeSpan(8, 0, 0); // 8:00 AM

        [BindProperty]
        public TimeSpan? EndTime { get; set; } = new TimeSpan(22, 0, 0); // 10:00 PM

        [BindProperty]
        public int Hours { get; set; } = 1;



        public async Task<IActionResult> OnGetAsync(int? id, string startDate = null, string endDate = null)
        {
            if (id == null)
                return NotFound();

            Car = await _carRepository.GetByIdAsync(id.Value);

            if (Car == null || !Car.IsApproved)
                return NotFound();

            // Xử lý ngày bắt đầu và kết thúc từ query string
            if (DateTime.TryParse(startDate, out DateTime start) && start >= DateTime.Today)
                StartDate = start;

            if (DateTime.TryParse(endDate, out DateTime end) && end > StartDate)
                EndDate = end;
            else
                EndDate = StartDate.AddDays(1);

            // Tính số ngày thuê
            Days = (EndDate - StartDate).Days + 1;
            if (Days < 1) Days = 1;

            // Tính tổng tiền
            TotalPrice = Car.PricePerDay * Days;

            // Kiểm tra xem xe có khả dụng trong khoảng thời gian đã chọn không
            IsAvailable = await _rentalRepository.IsCarAvailableAsync(Car.Id, StartDate, EndDate);

            // Lấy danh sách ngày không khả dụng (đã có người thuê)
            UnavailableDates = await GetUnavailableDates(Car.Id);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
                return RedirectToPage(new { id });

            Car = await _carRepository.GetByIdAsync(id);

            if (Car == null || !Car.IsApproved)
                return NotFound();

            bool isAvailable;

            // Tạo đơn thuê mới
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rental = new Rental
            {
                UserId = userId,
                CarId = id,
                Status = 0, // Chờ xác nhận
                PaymentStatus = "Pending",
                PaymentMethod = PaymentMethod,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Type = RentalType
            };

            if (RentalType == RentalType.ByDay)
            {
                // Kiểm tra xe có khả dụng không cho thuê theo ngày
                isAvailable = await _rentalRepository.IsCarAvailableAsync(id, StartDate, EndDate);

                if (!isAvailable)
                {
                    TempData["ErrorMessage"] = "Xe đã được đặt trong khoảng thời gian ngày bạn chọn. Vui lòng chọn khoảng thời gian khác.";
                    return RedirectToPage(new { id });
                }

                // Thiết lập thông tin cho thuê theo ngày
                rental.StartDate = StartDate;
                rental.EndDate = EndDate;
                rental.TotalPrice = Car.PricePerDay * Days;
            }
            else // RentalType.ByHour
            {
                // Tạo DateTime đầy đủ từ ngày và giờ
                var startDateTime = RentalDate.Value.Date.Add(StartTime.Value);
                var endDateTime = RentalDate.Value.Date.Add(EndTime.Value);

                // Kiểm tra xe có khả dụng không cho thuê theo giờ
                isAvailable = await _rentalRepository.IsCarAvailableByHourAsync(id, startDateTime, endDateTime);

                if (!isAvailable)
                {
                    TempData["ErrorMessage"] = "Xe đã được đặt trong khoảng thời gian giờ bạn chọn. Vui lòng chọn khoảng thời gian khác.";
                    return RedirectToPage(new { id });
                }

                // Thiết lập thông tin cho thuê theo giờ
                rental.StartDate = RentalDate.Value.Date;
                rental.EndDate = RentalDate.Value.Date;
                rental.StartTime = StartTime;
                rental.EndTime = EndTime;
                rental.Hours = Hours;
                rental.TotalPrice = Car.PricePerHour * Hours;
            }


            try
            {
                var newRental = await _rentalRepository.AddAsync(rental);
                TempData["SuccessMessage"] = "Đặt xe thành công! Vui lòng chờ xác nhận từ chủ xe.";
                return RedirectToPage("./Details", new { id = newRental.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi đặt xe: {ex.Message}";
                return RedirectToPage(new { id });
            }
        }


        public async Task<IActionResult> OnPostCalculateAsync(int id)
        {
            if (RentalType == RentalType.ByDay)
            {
                if (StartDate > EndDate)
                {
                    TempData["ErrorMessage"] = "Ngày kết thúc phải sau ngày bắt đầu.";
                    return RedirectToPage(new { id });
                }

                Days = (EndDate - StartDate).Days + 1;
                if (Days < 1) Days = 1;

                Car = await _carRepository.GetByIdAsync(id);
                if (Car == null) return NotFound();

                TotalPrice = Car.PricePerDay * Days;
                IsAvailable = await _rentalRepository.IsCarAvailableAsync(id, StartDate, EndDate);
            }
            else // RentalType.ByHour
            {
                if (StartTime >= EndTime)
                {
                    TempData["ErrorMessage"] = "Giờ kết thúc phải sau giờ bắt đầu.";
                    return RedirectToPage(new { id });
                }

                // Tính số giờ thuê
                Hours = (int)Math.Ceiling((EndTime.Value - StartTime.Value).TotalHours);
                if (Hours < 1) Hours = 1;

                Car = await _carRepository.GetByIdAsync(id);
                if (Car == null) return NotFound();

                TotalPrice = Car.PricePerHour * Hours;

                // Tạo DateTime đầy đủ từ ngày và giờ
                var startDateTime = RentalDate.Value.Date.Add(StartTime.Value);
                var endDateTime = RentalDate.Value.Date.Add(EndTime.Value);

                // Kiểm tra xem xe có khả dụng không trong khoảng thời gian đã chọn
                IsAvailable = await _rentalRepository.IsCarAvailableByHourAsync(id, startDateTime, endDateTime);
            }

            UnavailableDates = await GetUnavailableDates(id);

            if (!IsAvailable)
            {
                TempData["ErrorMessage"] = "Xe đã được đặt trong khoảng thời gian bạn chọn. Vui lòng chọn khoảng thời gian khác.";
            }

            return Page();
        }

        // Cập nhật phương thức GetUnavailableDates để xử lý thuê theo giờ
        private async Task<IEnumerable<DateTime>> GetUnavailableDates(int carId)
        {
            // Lấy tất cả các đơn thuê của xe này
            var rentals = await _rentalRepository.FindAsync(r => r.CarId == carId && r.Status != 2); // Không phải đơn đã hủy
            var unavailableDates = new List<DateTime>();

            foreach (var rental in rentals)
            {
                if (rental.Type == RentalType.ByDay)
                {
                    // Xử lý thuê theo ngày
                    for (var date = rental.StartDate; date <= rental.EndDate; date = date.AddDays(1))
                    {
                        unavailableDates.Add(date.Date);
                    }
                }
                else
                {
                    // Với thuê theo giờ, thêm ngày đó nếu thuê trong một khoảng giờ dài 
                    // (ví dụ: > 8 giờ hoặc trong giờ làm việc chính)
                    if (rental.StartTime.HasValue && rental.EndTime.HasValue)
                    {
                        var duration = rental.EndTime.Value - rental.StartTime.Value;
                        // Nếu thuê hơn 8 giờ, coi như ngày đó không khả dụng
                        if (duration.TotalHours >= 8)
                        {
                            unavailableDates.Add(rental.StartDate.Date);
                        }
                        // Hoặc nếu thuê trong giờ làm việc chính (9am-5pm)
                        else if ((rental.StartTime.Value <= TimeSpan.FromHours(9) &&
                                 rental.EndTime.Value >= TimeSpan.FromHours(17)))
                        {
                            unavailableDates.Add(rental.StartDate.Date);
                        }
                    }
                }
            }

            return unavailableDates;
        }

    }
}
