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

        [BindProperty]
        public ServiceType Service { get; set; } = ServiceType.SelfDrive;

        public decimal DriverFee { get; set; } = 0;

        [BindProperty]
        public Rental.TripType? Trip { get; set; } = null;

        [BindProperty]
        public string? PickupAddress { get; set; }

        [BindProperty]
        public double? PickupLatitude { get; set; }

        [BindProperty]
        public double? PickupLongitude { get; set; }

        [BindProperty]
        public decimal? EstimatedDistance { get; set; }

        [BindProperty]
        public decimal? DistanceFee { get; set; }
        [BindProperty]
        public bool IsDeliveryRequested { get; set; } = false;

        [BindProperty]
        public string? DeliveryAddress { get; set; }

        [BindProperty]
        public double? DeliveryLatitude { get; set; }

        [BindProperty]
        public double? DeliveryLongitude { get; set; }

        [BindProperty]
        public decimal DeliveryFee { get; set; } = 0;
        [BindProperty]
        public string? DropoffAddress { get; set; }

        [BindProperty]
        public double? DropoffLatitude { get; set; }

        [BindProperty]
        public double? DropoffLongitude { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id, string startDate = null, string endDate = null)
        {
            if (id == null)
                return NotFound();

            Car = await _carRepository.GetByIdAsync(id.Value);

            if (Car == null || !Car.IsApproved)
                return NotFound();

            // Kiểm tra người dùng có phải là chủ xe không
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Car.UserId == userId)
            {
                TempData["ErrorMessage"] = "Bạn không thể đặt xe của chính mình.";
                return RedirectToPage("/Cars/Details", new { area = "Customer", id = id });
            }

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

            // Tính tổng tiền ban đầu (không tính phí tài xế)
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

            // Kiểm tra người dùng có phải là chủ xe không
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Car.UserId == userId)
            {
                TempData["ErrorMessage"] = "Bạn không thể đặt xe của chính mình.";
                return RedirectToPage("/Cars/Details", new { area = "Customer", id = id });
            }

            bool isAvailable;
            DateTime actualStartDate, actualEndDate;
            decimal basePrice = 0;

            //// Tạo đơn thuê mới
            //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rental = new Rental
            {
                UserId = userId,
                CarId = id,
                Status = (int)RentalStatus.Pending, // Chờ xác nhận
                PaymentStatus = "Pending",
                PaymentMethod = PaymentMethod,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Type = RentalType,
                Service = Service,
                Trip = Trip, // Thêm loại hình di chuyển
                PickupAddress = PickupAddress, // Thêm địa chỉ đón
                PickupLatitude = PickupLatitude, // Thêm tọa độ điểm đón
                PickupLongitude = PickupLongitude, // Thêm tọa độ điểm đón
                EstimatedDistance = EstimatedDistance, // Thêm khoảng cách ước tính
                DriverFee = 0, // Mặc định là 0, sẽ tính sau

                // Thêm thông tin giao xe
                IsDeliveryRequested = IsDeliveryRequested,
                DeliveryAddress = DeliveryAddress,
                DeliveryLatitude = DeliveryLatitude,
                DeliveryLongitude = DeliveryLongitude,
                DeliveryFee = 0, // Mặc định là 0, sẽ tính sau

                // Thêm thông tin trả khách
                DropoffAddress = DropoffAddress, // Địa chỉ trả khách
                DropoffLatitude = DropoffLatitude, // Vĩ độ điểm trả khách 
                DropoffLongitude = DropoffLongitude // Kinh độ điểm trả khách
            };

            // Kiểm tra và xử lý nếu xe không hỗ trợ thuê theo giờ
            if (RentalType == RentalType.ByHour && Car.PricePerHour <= 0)
            {
                TempData["ErrorMessage"] = "Xe này không hỗ trợ thuê theo giờ. Vui lòng chọn thuê theo ngày.";
                return RedirectToPage(new { id });
            }

            // Xử lý dựa vào loại thuê
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
                actualStartDate = StartDate;
                actualEndDate = EndDate;
                Days = (EndDate - StartDate).Days + 1;
                basePrice = Car.PricePerDay * Days;
                rental.Days = Days;
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

                rental.StartDate = RentalDate.Value.Date;
                rental.EndDate = RentalDate.Value.Date;
                rental.StartTime = StartTime;
                rental.EndTime = EndTime;
                Hours = CalculateHours(StartTime.Value, EndTime.Value);

                // Đảm bảo giá theo giờ > 0
                decimal hourPrice = Car.PricePerHour > 0 ? Car.PricePerHour : Car.PricePerDay / 24;
                basePrice = hourPrice * Hours;
                rental.Hours = Hours;

                // Debug info
                Console.WriteLine($"Calculated hourly price: {hourPrice} VND/hour, hours: {Hours}, total: {basePrice}");

                actualStartDate = RentalDate.Value.Date.Add(StartTime.Value);
                actualEndDate = RentalDate.Value.Date.Add(EndTime.Value);
            }

            // Xử lý phí tài xế và phí khoảng cách
            if (Service == Rental.ServiceType.WithDriver)
            {
                if (RentalType == Rental.RentalType.ByDay)
                {
                    rental.DriverFee = Car.DriverFeePerDay * Days;
                }
                else // RentalType.ByHour
                {
                    // Đảm bảo phí tài xế theo giờ > 0
                    rental.DriverFee = Car.DriverFeePerHour > 0 ?
                        Car.DriverFeePerHour * Hours :
                        (Car.DriverFeePerDay / 24) * Hours;
                }

                // Tính phí khoảng cách nếu là chuyến liên tỉnh
                if ((Trip == Rental.TripType.InterCityRoundTrip || Trip == Rental.TripType.InterCityOneWay) && EstimatedDistance.HasValue)
                {
                    const decimal KmFee = 5000m;
                    decimal distance = EstimatedDistance.Value;
                    decimal multiplier = Trip == Rental.TripType.InterCityOneWay ? 2m : 1m;

                    // Tính phí khoảng cách
                    decimal distanceFee = distance * KmFee * multiplier;

                    // Hiển thị log để debug
                    Console.WriteLine($"DEBUG: Trip={Trip}, Distance={distance}km, KmFee={KmFee}, Multiplier={multiplier}, DistanceFee={distanceFee}");

                    // Gán giá trị đã tính vào rental.DistanceFee
                    rental.DistanceFee = distanceFee;

                    // Thêm phí khoảng cách vào tổng giá
                    basePrice += distanceFee;

                    // Thêm debug để đảm bảo phí được cộng vào tổng
                    Console.WriteLine($"DEBUG: basePrice after adding distance fee: {basePrice}");
                }

                // Thêm phí tài xế vào tổng giá
                basePrice += rental.DriverFee;
            }
            else // Dịch vụ tự lái
            {
                // Tính phí giao xe nếu có yêu cầu giao xe
                if (IsDeliveryRequested && DeliveryLatitude.HasValue && DeliveryLongitude.HasValue &&
                    Car.Latitude.HasValue && Car.Longitude.HasValue)
                {
                    // Tính khoảng cách từ vị trí xe đến vị trí giao xe (km)
                    double distanceInKm = _geoLocationService.CalculateDistance(
                        Car.Latitude.Value, Car.Longitude.Value,
                        DeliveryLatitude.Value, DeliveryLongitude.Value);

                    // Tính phí giao xe: 10.000 VNĐ/km, tối thiểu 20.000 VNĐ
                    rental.DeliveryFee = Math.Max(20000, (decimal)distanceInKm * 10000);

                    // In ra thông tin debug
                    Console.WriteLine($"DEBUG: DeliveryDistance={distanceInKm}km, DeliveryFee={rental.DeliveryFee}");

                    // Thêm phí giao xe vào tổng giá
                    basePrice += rental.DeliveryFee;
                }
            }

            // Thiết lập tổng giá
            rental.TotalPrice = basePrice;

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

        // Sửa phần tính phí tài xế trong phương thức OnPostCalculateAsync()
        public async Task<IActionResult> OnPostCalculateAsync(int id)
        {
            Car = await _carRepository.GetByIdAsync(id);
            if (Car == null) return NotFound();

            // Kiểm tra người dùng có phải là chủ xe không
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Car.UserId == userId)
            {
                TempData["ErrorMessage"] = "Bạn không thể đặt xe của chính mình.";
                return RedirectToPage("/Cars/Details", new { area = "Customer", id = id });
            }

            try
            {
                // In ra thông tin Car và PricePerHour để debug
                Console.WriteLine($"Car: {Car.Name}, PricePerDay: {Car.PricePerDay}, PricePerHour: {Car.PricePerHour}");

                // Kiểm tra xem xe có giá thuê theo giờ > 0 không
                if (RentalType == RentalType.ByHour && Car.PricePerHour <= 0)
                {
                    TempData["ErrorMessage"] = "Xe này không hỗ trợ thuê theo giờ. Vui lòng chọn thuê theo ngày.";
                    RentalType = RentalType.ByDay;
                    // Chuyển sang thuê theo ngày
                }

                if (RentalType == RentalType.ByDay)
                {
                    if (StartDate > EndDate)
                    {
                        TempData["ErrorMessage"] = "Ngày kết thúc phải sau ngày bắt đầu.";
                        return RedirectToPage(new { id });
                    }

                    Days = (EndDate - StartDate).Days + 1;
                    if (Days < 1) Days = 1;

                    // Tính giá cơ bản theo ngày
                    TotalPrice = Car.PricePerDay * Days;

                    // Kiểm tra xe có khả dụng không
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
                    Hours = CalculateHours(StartTime.Value, EndTime.Value);
                    if (Hours < 1) Hours = 1;

                    // Debug: In ra console giá trị giờ và giá thuê
                    Console.WriteLine($"Hours: {Hours}, PricePerHour: {Car.PricePerHour}");

                    // Tính giá cơ bản theo giờ và in thông tin debug
                    decimal hourPrice = Car.PricePerHour > 0 ? Car.PricePerHour : Car.PricePerDay / 24;
                    TotalPrice = hourPrice * Hours;

                    Console.WriteLine($"Calculated TotalPrice for hourly rental: {TotalPrice} = {hourPrice} × {Hours}");

                    // Tạo DateTime đầy đủ từ ngày và giờ
                    var startDateTime = RentalDate.Value.Date.Add(StartTime.Value);
                    var endDateTime = RentalDate.Value.Date.Add(EndTime.Value);

                    // Kiểm tra xe có khả dụng không
                    IsAvailable = await _rentalRepository.IsCarAvailableByHourAsync(id, startDateTime, endDateTime);

                    // Debug: In kết quả kiểm tra
                    Console.WriteLine($"Car availability for hourly rental: {IsAvailable}");
                }

                // Tính phí tài xế nếu có
                if (Service == Rental.ServiceType.WithDriver)
                {
                    if (RentalType == Rental.RentalType.ByDay)
                    {
                        DriverFee = Car.DriverFeePerDay * Days;
                    }
                    else // RentalType.ByHour
                    {
                        // Đảm bảo phí tài xế theo giờ > 0
                        decimal hourlyDriverFee = Car.DriverFeePerHour > 0 ? Car.DriverFeePerHour : Car.DriverFeePerDay / 24;
                        DriverFee = hourlyDriverFee * Hours;
                        Console.WriteLine($"Calculated DriverFee: {DriverFee} = {hourlyDriverFee} × {Hours}");
                    }

                    // Tính phí khoảng cách nếu là chuyến liên tỉnh
                    if ((Trip == Rental.TripType.InterCityRoundTrip || Trip == Rental.TripType.InterCityOneWay) && EstimatedDistance.HasValue)
                    {
                        // Giả sử phí là 5.000 VNĐ/km, điều chỉnh theo yêu cầu thực tế
                        const decimal KmFee = 5000m;
                        decimal distance = EstimatedDistance.Value;

                        // Với chuyến đi 1 chiều, tài xế phải quay lại nên tính 2 lần khoảng cách
                        decimal multiplier = Trip == Rental.TripType.InterCityOneWay ? 2m : 1m;
                        decimal distanceFee = distance * KmFee * multiplier;

                        // In ra để debug
                        Console.WriteLine($"DEBUG: Trip={Trip}, Distance={distance}km, KmFee={KmFee}, Multiplier={multiplier}, DistanceFee={distanceFee}");

                        // Gán giá trị đã tính vào DistanceFee
                        DistanceFee = distanceFee;

                        // Thêm phí khoảng cách vào tổng phí
                        TotalPrice += distanceFee;
                    }

                    // Thêm phí tài xế vào tổng phí
                    TotalPrice += DriverFee;
                }
                else // Dịch vụ tự lái
                {
                    DriverFee = 0;
                    DistanceFee = 0;

                    // Tính phí giao xe nếu có yêu cầu giao xe
                    if (IsDeliveryRequested && DeliveryLatitude.HasValue && DeliveryLongitude.HasValue &&
                        Car.Latitude.HasValue && Car.Longitude.HasValue)
                    {
                        // Tính khoảng cách từ vị trí xe đến vị trí giao xe (km)
                        double distanceInKm = _geoLocationService.CalculateDistance(
                            Car.Latitude.Value, Car.Longitude.Value,
                            DeliveryLatitude.Value, DeliveryLongitude.Value);

                        // Tính phí giao xe: 10.000 VNĐ/km, tối thiểu 20.000 VNĐ
                        DeliveryFee = Math.Max(20000, (decimal)distanceInKm * 10000);

                        // In ra thông tin debug
                        Console.WriteLine($"DEBUG: DeliveryDistance={distanceInKm}km, DeliveryFee={DeliveryFee}");

                        // Thêm phí giao xe vào tổng phí
                        TotalPrice += DeliveryFee;
                    }
                    else
                    {
                        // Reset phí giao xe nếu không yêu cầu giao
                        DeliveryFee = 0;
                    }
                }

                UnavailableDates = await GetUnavailableDates(id);

                if (!IsAvailable)
                {
                    TempData["ErrorMessage"] = "Xe đã được đặt trong khoảng thời gian bạn chọn. Vui lòng chọn khoảng thời gian khác.";
                }

                return Page();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during price calculation: {ex.Message}");
                TempData["ErrorMessage"] = $"Có lỗi xảy ra khi tính giá: {ex.Message}";
                return Page();
            }
        }

        private int CalculateHours(TimeSpan startTime, TimeSpan endTime)
        {
            // Tính số giờ giữa hai thời điểm, làm tròn lên
            double hoursDiff = (endTime - startTime).TotalHours;
            return (int)Math.Ceiling(hoursDiff);
        }

        private async Task<IEnumerable<DateTime>> GetUnavailableDates(int carId)
        {
            // Lấy tất cả các đơn thuê của xe này đang chờ xác nhận hoặc đã xác nhận
            var rentals = await _rentalRepository.FindAsync(
                r => r.CarId == carId &&
                    (r.Status == (int)RentalStatus.Pending || r.Status == (int)RentalStatus.Confirmed || r.Status == (int)RentalStatus.InProgress)
            );

            var unavailableDates = new List<DateTime>();

            // Debug: In ra tổng số đơn thuê đã lấy
            Console.WriteLine($"Found {rentals.Count()} rentals for car {carId}");

            foreach (var rental in rentals)
            {
                // Debug: In thông tin mỗi đơn thuê
                Console.WriteLine($"Processing rental ID={rental.Id}, Type={rental.Type}, " +
                    $"StartDate={rental.StartDate:yyyy-MM-dd}, EndDate={rental.EndDate:yyyy-MM-dd}, Status={rental.Status}");

                if (rental.Type == RentalType.ByDay)
                {
                    // Với thuê theo ngày - đánh dấu tất cả các ngày từ StartDate đến EndDate là không khả dụng
                    for (var date = rental.StartDate; date <= rental.EndDate; date = date.AddDays(1))
                    {
                        unavailableDates.Add(date.Date);
                        Console.WriteLine($"Marking day as unavailable: {date:yyyy-MM-dd}");
                    }
                }
                else if (rental.Type == RentalType.ByHour && rental.StartTime.HasValue && rental.EndTime.HasValue)
                {
                    // Với thuê theo giờ, đánh dấu ngày đó là không khả dụng cho thuê theo ngày
                    // (Đơn giản hóa logic - nếu có thuê theo giờ trong ngày, không cho thuê theo ngày)
                    unavailableDates.Add(rental.StartDate.Date);
                    Console.WriteLine($"Marking hourly rental date as unavailable: {rental.StartDate.Date:yyyy-MM-dd} " +
                        $"(StartTime={rental.StartTime}, EndTime={rental.EndTime})");
                }
            }

            // Đảm bảo rằng danh sách không rỗng để tránh JSON không hoàn chỉnh
            var result = unavailableDates.Distinct().ToList(); // Loại bỏ các ngày trùng lặp

            // Ghi log để debug
            Console.WriteLine($"Total unavailable dates: {result.Count}");
            Console.WriteLine($"Final UnavailableDates JSON: {System.Text.Json.JsonSerializer.Serialize(result)}");

            foreach (var date in result)
            {
                Console.WriteLine($"Final unavailable date: {date:yyyy-MM-dd}");
            }

            return result; // Trả về danh sách (có thể rỗng nếu không có đơn thuê nào)
        }
    }
}