using GotoCarRental.Models;
using System.Linq.Expressions;

namespace GotoCarRental.Repository
{
    public interface IRentalRepository
    {
        // Phương thức CRUD cơ bản
        Task<Rental> GetByIdAsync(int id);
        Task<IEnumerable<Rental>> GetAllAsync();
        Task<IEnumerable<Rental>> GetAllForUserAsync(string userId);
        Task<Rental> AddAsync(Rental rental);
        Task UpdateAsync(Rental rental);
        Task DeleteAsync(int id);

        // Phương thức tìm kiếm và lọc
        Task<IEnumerable<Rental>> FindAsync(Expression<Func<Rental, bool>> predicate);

        // Phương thức xử lý trạng thái
        Task<Rental> ConfirmRentalAsync(int id);
        Task<Rental> CancelRentalAsync(int id);
        Task<Rental> CompleteRentalAsync(int id);

        // Phương thức liên quan đến thanh toán
        Task<Payment> AddPaymentAsync(Payment payment);
        Task<IEnumerable<Payment>> GetPaymentsForRentalAsync(int rentalId);

        // Phương thức kiểm tra
        Task<bool> IsCarAvailableAsync(int carId, DateTime startDate, DateTime endDate);
        Task<bool> HasUserRentedCarAsync(string userId, int carId);

        // Phương thức thống kê
        Task<int> GetRentalCountAsync(string userId);
        Task<decimal> GetTotalSpentAsync(string userId);

        // Phương thức phân trang
        Task<(IEnumerable<Rental> Rentals, int TotalCount)> GetRentalsPagedAsync(
            int pageNumber,
            int pageSize,
            string userId,
            int? status = null,
            string sortBy = "newest");
        // Thêm phương thức mới
        Task<Payment> AddPendingPaymentAsync(Payment payment);
        Task<Payment> ConfirmPaymentAsync(int paymentId);

        // Thêm vào IRentalRepository
        Task<bool> IsCarAvailableByHourAsync(int carId, DateTime startDateTime, DateTime endDateTime);

        // Phương thức cập nhật trạng thái đơn thuê
        Task<Rental> UpdateStatusAsync(int id, int status);
        Task<IEnumerable<Rental>> GetAllCompletedRentalsAsync();
        Task<decimal> GetDefaultCommissionRateAsync();
        Task SetDefaultCommissionRateAsync(decimal rate);
        Task<decimal?> GetCategoryCommissionRateAsync(int categoryId);
        Task SetCategoryCommissionRateAsync(int categoryId, decimal rate);


    }
}
