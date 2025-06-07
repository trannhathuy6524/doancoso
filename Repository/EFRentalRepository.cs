using GotoCarRental.Data;
using GotoCarRental.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GotoCarRental.Repository
{
    public class EFRentalRepository : IRentalRepository
    {
        private readonly ApplicationDbContext _context;

        public EFRentalRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Rental> GetByIdAsync(int id)
        {
            return await _context.Rentals
                .Include(r => r.Car)
                    .ThenInclude(c => c.Brand)
                .Include(r => r.Car)
                    .ThenInclude(c => c.Category)
                .Include(r => r.Car)
                    .ThenInclude(c => c.CarImages)
                .Include(r => r.User)
                .Include(r => r.Payments)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Rental>> GetAllAsync()
        {
            return await _context.Rentals
                .Include(r => r.Car)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Rental>> GetAllForUserAsync(string userId)
        {
            return await _context.Rentals
                .Include(r => r.Car)
                    .ThenInclude(c => c.Brand)
                .Include(r => r.Car)
                    .ThenInclude(c => c.CarImages)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Rental> AddAsync(Rental rental)
        {
            // Đảm bảo giá trị Hours được bảo toàn nếu đơn thuê theo giờ
            if (rental.Type == Rental.RentalType.ByHour)
            {
                // Tính toán lại Hours để đảm bảo giá trị chính xác
                if (rental.StartTime.HasValue && rental.EndTime.HasValue)
                {
                    var hours = (int)Math.Ceiling((rental.EndTime.Value - rental.StartTime.Value).TotalHours);
                    if (hours < 1) hours = 1;

                    // Đảm bảo Hours là giá trị đã tính
                    rental.Hours = hours;

                    // PHẦN CẦN SỬA: Không ghi đè lên TotalPrice đã được tính trong Create.cshtml.cs
                    // Chỉ lưu lại TotalPrice đã tính toán trước đó
                    Console.WriteLine($"EFRentalRepository.AddAsync: Giữ nguyên TotalPrice={rental.TotalPrice}");

                    // Thêm log để kiểm tra
                    Console.WriteLine($"EFRentalRepository.AddAsync: Type=ByHour, StartTime={rental.StartTime}, EndTime={rental.EndTime}, Hours={rental.Hours}, TotalPrice={rental.TotalPrice}, DistanceFee={rental.DistanceFee}, DriverFee={rental.DriverFee}");
                }
            }

            await _context.Rentals.AddAsync(rental);
            await _context.SaveChangesAsync();
            return rental;
        }


        public async Task UpdateAsync(Rental rental)
        {
            rental.UpdatedAt = DateTime.UtcNow;
            _context.Rentals.Update(rental);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental != null)
            {
                _context.Rentals.Remove(rental);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Rental>> FindAsync(Expression<Func<Rental, bool>> predicate)
        {
            return await _context.Rentals
                .Include(r => r.Car)
                .Include(r => r.User)
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<Rental> ConfirmRentalAsync(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental == null)
                throw new ArgumentException("Rental not found");

            rental.Status = 1; // Confirmed
            rental.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return rental;
        }

        public async Task<Rental> CancelRentalAsync(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental == null)
                throw new ArgumentException("Rental not found");

            rental.Status = 2; // Cancelled
                               // Cập nhật PaymentStatus thành "Cancelled" khi hủy đơn
                               // Luôn cập nhật PaymentStatus thành "Cancelled" khi hủy đơn
            rental.PaymentStatus = "Cancelled";

            rental.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return rental;
        }


        public async Task<Rental> CompleteRentalAsync(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental == null)
                throw new ArgumentException("Rental not found");

            rental.Status = 3; // Completed
            rental.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return rental;
        }

        

        public async Task<Payment> AddPaymentAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();

            var rental = await _context.Rentals.FindAsync(payment.RentalId);
            if (rental != null)
            {
                rental.PaymentStatus = payment.Status == "Pending" ? "Pending" : "Paid";
                rental.PaymentMethod = payment.Method;
                rental.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return payment;
        }

        public async Task<IEnumerable<Payment>> GetPaymentsForRentalAsync(int rentalId)
        {
            var payments = await _context.Payments
                .Where(p => p.RentalId == rentalId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            // Debug: Log thông tin về các thanh toán đã lấy
            System.Diagnostics.Debug.WriteLine($"Found {payments.Count} payments for rental {rentalId}");
            foreach (var payment in payments)
            {
                System.Diagnostics.Debug.WriteLine($"Payment ID: {payment.PaymentId}, Status: {payment.Status}, Method: {payment.Method}");
            }

            return payments;
        }


        public async Task<bool> IsCarAvailableAsync(int carId, DateTime startDate, DateTime endDate)
        {
            // Kiểm tra xem xe có tồn tại và khả dụng không
            var car = await _context.Cars.FindAsync(carId);
            if (car == null || !car.IsAvailable || !car.IsApproved)
                return false;

            // Debug để xem khoảng thời gian đang kiểm tra
            Console.WriteLine($"Checking availability for car {carId} from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

            // Lấy danh sách tất cả các đơn thuê chưa hủy của xe trong khoảng thời gian gần với yêu cầu
            var overlappingDayRentals = await _context.Rentals
                .Where(r => r.CarId == carId && r.Status != 2) // Không phải là đã hủy
                .Where(r => r.Type == Rental.RentalType.ByDay) // Chỉ xét các đơn thuê theo ngày
                .Where(r => (startDate.Date <= r.EndDate && endDate.Date >= r.StartDate))
                .ToListAsync();

            // In thông tin các đơn thuê theo ngày trùng lặp
            foreach (var rental in overlappingDayRentals)
            {
                Console.WriteLine($"Found overlapping day rental: ID={rental.Id}, StartDate={rental.StartDate:yyyy-MM-dd}, EndDate={rental.EndDate:yyyy-MM-dd}, Status={rental.Status}");
            }

            // Nếu có bất kỳ đơn thuê theo ngày nào trùng lặp, xe không khả dụng
            if (overlappingDayRentals.Any())
                return false;

            // Lấy danh sách các đơn thuê theo giờ trong các ngày này
            var potentialHourRentals = await _context.Rentals
                .Where(r => r.CarId == carId && r.Status != 2) // Không phải là đã hủy
                .Where(r => r.Type == Rental.RentalType.ByHour) // Chỉ xét các đơn thuê theo giờ
                .Where(r => (r.StartDate.Date >= startDate.Date && r.StartDate.Date <= endDate.Date)) // Trong khoảng ngày đang xét
                .ToListAsync();

            // Xét từng ngày trong khoảng thời gian yêu cầu
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                // Với thuê theo ngày, nếu có đơn thuê theo giờ nào đó trên ngày này, xe không khả dụng cho ngày đó
                var hourRentalsOnDay = potentialHourRentals.Where(r => r.StartDate.Date == date).ToList();
                if (hourRentalsOnDay.Any())
                {
                    // In thông tin các đơn thuê theo giờ trên ngày này
                    Console.WriteLine($"Found hour rentals on date {date:yyyy-MM-dd}:");
                    foreach (var rental in hourRentalsOnDay)
                    {
                        Console.WriteLine($"- ID={rental.Id}, StartTime={rental.StartTime}, EndTime={rental.EndTime}");
                    }

                    return false;
                }
            }

            // Nếu không có đơn thuê nào trùng lặp, xe khả dụng
            return true;
        }

        public async Task<bool> HasUserRentedCarAsync(string userId, int carId)
        {
            return await _context.Rentals
                .AnyAsync(r => r.UserId == userId && r.CarId == carId &&
                         (r.Status == 1 || r.Status == 3)); // Đã xác nhận hoặc hoàn thành
        }

        public async Task<int> GetRentalCountAsync(string userId)
        {
            return await _context.Rentals
                .CountAsync(r => r.UserId == userId);
        }

        public async Task<decimal> GetTotalSpentAsync(string userId)
        {
            return await _context.Rentals
                .Where(r => r.UserId == userId && (r.Status == 1 || r.Status == 3)) // Đã xác nhận hoặc hoàn thành
                .SumAsync(r => r.TotalPrice);
        }

        public async Task<(IEnumerable<Rental> Rentals, int TotalCount)> GetRentalsPagedAsync(
            int pageNumber,
            int pageSize,
            string userId,
            int? status = null,
            string sortBy = "newest")
        {
            IQueryable<Rental> query = _context.Rentals
                .Include(r => r.Car)
                    .ThenInclude(c => c.Brand)
                .Include(r => r.Car)
                    .ThenInclude(c => c.CarImages)
                .Where(r => r.UserId == userId);

            // Lọc theo status nếu có
            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            // Sắp xếp theo các tiêu chí
            switch (sortBy)
            {
                case "oldest":
                    query = query.OrderBy(r => r.CreatedAt);
                    break;
                case "start_date":
                    query = query.OrderBy(r => r.StartDate);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(r => r.TotalPrice);
                    break;
                case "price_asc":
                    query = query.OrderBy(r => r.TotalPrice);
                    break;
                default: // newest
                    query = query.OrderByDescending(r => r.CreatedAt);
                    break;
            }

            var totalCount = await query.CountAsync();

            var rentals = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (rentals, totalCount);
        }

        public async Task<Payment> AddPendingPaymentAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();

            var rental = await _context.Rentals.FindAsync(payment.RentalId);
            if (rental != null)
            {
                System.Diagnostics.Debug.WriteLine($"Updating rental {rental.Id} payment status to Pending");
                rental.PaymentStatus = "Pending";
                rental.PaymentMethod = payment.Method;
                rental.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return payment;
        }


        public async Task<Payment> ConfirmPaymentAsync(int paymentId)
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null)
                throw new ArgumentException("Không tìm thấy thông tin thanh toán");

            payment.Status = "Completed";
            payment.UpdatedAt = DateTime.UtcNow;

            var rental = await _context.Rentals.FindAsync(payment.RentalId);
            if (rental != null)
            {
                rental.PaymentStatus = "Paid";
                rental.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task<bool> IsCarAvailableByHourAsync(int carId, DateTime startDateTime, DateTime endDateTime)
        {
            // Kiểm tra xem xe có tồn tại và khả dụng không
            var car = await _context.Cars.FindAsync(carId);
            if (car == null || !car.IsAvailable || !car.IsApproved)
                return false;

            // Debug để xem khoảng thời gian đang kiểm tra
            Console.WriteLine($"Checking hourly availability for car {carId} on {startDateTime.Date:yyyy-MM-dd} from {startDateTime.TimeOfDay} to {endDateTime.TimeOfDay}");

            // Xác định ngày của startDateTime và endDateTime (chỉ lấy phần Date)
            DateTime requestedDate = startDateTime.Date;

            // Kiểm tra xem có đơn thuê ngày nào đè lên ngày này không
            var overlappingDayRental = await _context.Rentals
                .Where(r => r.CarId == carId && r.Status != 2) // Không phải là đã hủy
                .Where(r => r.Type == Rental.RentalType.ByDay) // Chỉ xét các đơn thuê theo ngày
                .Where(r => (r.StartDate.Date <= requestedDate && r.EndDate.Date >= requestedDate))
                .FirstOrDefaultAsync();

            // Nếu có đơn thuê theo ngày đè lên ngày này, xe không khả dụng
            if (overlappingDayRental != null)
            {
                Console.WriteLine($"Found overlapping day rental: ID={overlappingDayRental.Id}, StartDate={overlappingDayRental.StartDate:yyyy-MM-dd}, EndDate={overlappingDayRental.EndDate:yyyy-MM-dd}");
                return false;
            }

            // Lấy các đơn thuê theo giờ trong ngày này
            var hourRentalsOnDay = await _context.Rentals
                .Where(r => r.CarId == carId && r.Status != 2) // Không phải là đã hủy
                .Where(r => r.Type == Rental.RentalType.ByHour) // Chỉ xét các đơn thuê theo giờ
                .Where(r => r.StartDate.Date == requestedDate) // Chỉ trong ngày này
                .Where(r => r.StartTime.HasValue && r.EndTime.HasValue) // Đảm bảo có giờ bắt đầu và kết thúc
                .ToListAsync();

            // Nếu không có đơn thuê giờ nào trong ngày này, xe khả dụng
            if (!hourRentalsOnDay.Any())
                return true;

            // In thông tin các đơn thuê theo giờ trong ngày để debug
            Console.WriteLine($"Found {hourRentalsOnDay.Count} hourly rentals on date {requestedDate:yyyy-MM-dd}");

            // Kiểm tra từng đơn thuê giờ xem có chồng chéo không
            foreach (var rental in hourRentalsOnDay)
            {
                // Tạo DateTime đầy đủ từ ngày và giờ của đơn thuê hiện có
                TimeSpan existingStartTime = rental.StartTime.Value;
                TimeSpan existingEndTime = rental.EndTime.Value;

                Console.WriteLine($"- Rental ID={rental.Id}, StartTime={existingStartTime}, EndTime={existingEndTime}");
                Console.WriteLine($"- Comparing with requested: StartTime={startDateTime.TimeOfDay}, EndTime={endDateTime.TimeOfDay}");

                // Kiểm tra chồng chéo thời gian
                // Không chồng chéo nếu: kết thúc mới <= bắt đầu cũ HOẶC bắt đầu mới >= kết thúc cũ
                bool noOverlap = (endDateTime.TimeOfDay <= existingStartTime) || (startDateTime.TimeOfDay >= existingEndTime);
                bool overlaps = !noOverlap;

                Console.WriteLine($"- Overlap result: {overlaps}");

                // Nếu có chồng chéo, xe không khả dụng
                if (overlaps)
                    return false;
            }

            // Nếu không có đơn thuê nào chồng chéo, xe khả dụng
            return true;
        }

        // Cập nhật phương thức này trong EFRentalRepository
        public async Task<Rental> UpdateStatusAsync(int id, int status)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental == null)
                throw new ArgumentException("Rental not found");

            rental.Status = status;
            rental.UpdatedAt = DateTime.UtcNow;

            // Nếu đơn thuê hoàn thành, tính hoa hồng
            if (status == (int)Rental.RentalStatus.Completed)
            {
                // Đảm bảo tổng tiền > 0
                if (rental.TotalPrice > 0)
                {
                    // Lấy tỷ lệ hoa hồng cho loại xe cụ thể hoặc tỷ lệ mặc định
                    var category = await _context.Cars
                        .Where(c => c.Id == rental.CarId)
                        .Select(c => c.CategoryId)
                        .FirstOrDefaultAsync();

                    decimal commissionRate = await GetDefaultCommissionRateAsync();
                    var categoryRate = await GetCategoryCommissionRateAsync(category);

                    if (categoryRate.HasValue)
                    {
                        commissionRate = categoryRate.Value;
                    }

                    // Áp dụng tỷ lệ hoa hồng vào đơn thuê
                    rental.CommissionRate = commissionRate;

                    // Tính phí hoa hồng và số tiền của chủ xe
                    rental.CalculateCommission();
                }
            }

            await _context.SaveChangesAsync();
            return rental;
        }


        // Cài đặt giá trị mặc định cho tỷ lệ hoa hồng
        private const decimal DEFAULT_COMMISSION_RATE = 10.0m; // 10%

        public async Task<decimal> GetDefaultCommissionRateAsync()
        {
            // Lấy giá trị từ cài đặt hệ thống (có thể thay bằng cách lấy từ bảng Settings trong CSDL)
            var setting = await _context.Settings
                .FirstOrDefaultAsync(s => s.Key == "DefaultCommissionRate");

            if (setting != null && decimal.TryParse(setting.Value, out decimal rate))
            {
                return rate;
            }

            return DEFAULT_COMMISSION_RATE; // Giá trị mặc định nếu không tìm thấy trong cài đặt
        }

        public async Task SetDefaultCommissionRateAsync(decimal rate)
        {
            // Cập nhật giá trị trong cài đặt hệ thống
            var setting = await _context.Settings
                .FirstOrDefaultAsync(s => s.Key == "DefaultCommissionRate");

            if (setting == null)
            {
                // Nếu không tồn tại, tạo mới
                setting = new Setting
                {
                    Key = "DefaultCommissionRate",
                    Value = rate.ToString(),
                    Description = "Tỷ lệ hoa hồng mặc định (%)"
                };
                await _context.Settings.AddAsync(setting);
            }
            else
            {
                // Nếu tồn tại, cập nhật giá trị
                setting.Value = rate.ToString();
                _context.Settings.Update(setting);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<decimal?> GetCategoryCommissionRateAsync(int categoryId)
        {
            // Lấy tỷ lệ hoa hồng riêng cho từng loại xe
            var setting = await _context.Settings
                .FirstOrDefaultAsync(s => s.Key == $"CategoryCommissionRate:{categoryId}");

            if (setting != null && decimal.TryParse(setting.Value, out decimal rate))
            {
                return rate;
            }

            return null; // Trả về null nếu không có tỷ lệ riêng
        }

        public async Task SetCategoryCommissionRateAsync(int categoryId, decimal rate)
        {
            // Cập nhật tỷ lệ hoa hồng cho loại xe
            var setting = await _context.Settings
                .FirstOrDefaultAsync(s => s.Key == $"CategoryCommissionRate:{categoryId}");

            if (setting == null)
            {
                // Nếu không tồn tại, tạo mới
                var category = await _context.Categories.FindAsync(categoryId);
                if (category == null)
                    throw new ArgumentException($"Không tìm thấy loại xe với ID {categoryId}");

                setting = new Setting
                {
                    Key = $"CategoryCommissionRate:{categoryId}",
                    Value = rate.ToString(),
                    Description = $"Tỷ lệ hoa hồng cho loại xe {category.Name} (%)"
                };
                await _context.Settings.AddAsync(setting);
            }
            else
            {
                // Nếu tồn tại, cập nhật giá trị
                setting.Value = rate.ToString();
                _context.Settings.Update(setting);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Rental>> GetAllCompletedRentalsAsync()
        {
            // Lấy tất cả các đơn thuê đã hoàn thành
            return await _context.Rentals
                .Include(r => r.Car)
                    .ThenInclude(c => c.Brand)
                .Include(r => r.Car)
                    .ThenInclude(c => c.Category)
                .Include(r => r.Car)
                    .ThenInclude(c => c.User)
                .Include(r => r.User)
                .Where(r => r.Status == (int)Rental.RentalStatus.Completed)
                .OrderByDescending(r => r.UpdatedAt)
                .ToListAsync();
        }
    }
}
