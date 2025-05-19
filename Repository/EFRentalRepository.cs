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
            if (rental.PaymentStatus == "Pending")
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
            // Kiểm tra xem xe có đang được thuê trong khoảng thời gian đó không
            var overlappingRentals = await _context.Rentals
                .Where(r => r.CarId == carId && r.Status != 2) // Không phải là đã hủy
                .Where(r => (startDate <= r.EndDate && endDate >= r.StartDate))
                .AnyAsync();

            // Kiểm tra xem xe có available không
            var car = await _context.Cars.FindAsync(carId);
            if (car == null || !car.IsAvailable || !car.IsApproved)
                return false;

            return !overlappingRentals;
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


    }
}
