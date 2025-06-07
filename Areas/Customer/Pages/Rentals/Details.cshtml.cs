using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using static GotoCarRental.Models.Rental;

namespace GotoCarRental.Areas.Customer.Pages.Rentals
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IRentalRepository _rentalRepository;
        private readonly IReviewRepository _reviewRepository;

        public DetailsModel(IRentalRepository rentalRepository, IReviewRepository reviewRepository)
        {
            _rentalRepository = rentalRepository;
            _reviewRepository = reviewRepository;
        }

        public Rental Rental { get; set; }
        public bool HasReviewed { get; set; }
        public IEnumerable<Payment> Payments { get; set; }
        public int RentalDays { get; set; }
        public bool CanCancel { get; set; }
        public bool CanReview { get; set; }
        public bool CanPayWithCash { get; set; }
        public string PaymentStatus { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Rental = await _rentalRepository.GetByIdAsync(id.Value);

            if (Rental == null || Rental.UserId != userId)
                return NotFound();

            // Xử lý đơn thuê theo loại
            if (Rental.Type == RentalType.ByDay)
            {
                // Tính số ngày thuê
                RentalDays = (Rental.EndDate - Rental.StartDate).Days + 1;
                if (RentalDays < 1) RentalDays = 1;

                // Kiểm tra và cập nhật tổng tiền nếu nó bằng 0
                if (Rental.TotalPrice == 0 && Rental.Car != null && Rental.Car.PricePerDay > 0)
                {
                    Rental.TotalPrice = Rental.Car.PricePerDay * RentalDays;
                    await _rentalRepository.UpdateAsync(Rental);
                }
            }
            else // RentalType.ByHour
            {
                // Nếu đơn thuê theo giờ nhưng field Hours là null hoặc 0
                if (!Rental.Hours.HasValue || Rental.Hours == 0)
                {
                    // Tính toán số giờ dựa trên StartTime và EndTime
                    if (Rental.StartTime.HasValue && Rental.EndTime.HasValue)
                    {
                        int calculatedHours = (int)Math.Ceiling((Rental.EndTime.Value - Rental.StartTime.Value).TotalHours);
                        if (calculatedHours < 1) calculatedHours = 1;

                        // Cập nhật số giờ vào đơn thuê
                        Rental.Hours = calculatedHours;

                        // Kiểm tra và cập nhật tổng tiền nếu cần
                        if (Rental.TotalPrice == 0 && Rental.Car != null && Rental.Car.PricePerHour > 0)
                        {
                            Rental.TotalPrice = Rental.Car.PricePerHour * calculatedHours;
                        }

                        // Lưu các thay đổi
                        await _rentalRepository.UpdateAsync(Rental);
                    }
                }
            }

            // Lấy danh sách thanh toán
            Payments = await _rentalRepository.GetPaymentsForRentalAsync(id.Value);

            // Kiểm tra quyền hủy đơn: chỉ hủy đơn chờ xác nhận
            CanCancel = Rental.Status == 0;

            // Kiểm tra quyền đánh giá: chỉ đánh giá đơn đã hoàn thành
            CanReview = Rental.Status == 3;

            // Kiểm tra nếu có thể thanh toán bằng tiền mặt
            CanPayWithCash = (Rental.Status == 1 && Rental.PaymentStatus != "Paid" && Rental.PaymentStatus != "Pending");

            // Kiểm tra xem người dùng đã đánh giá xe sau chuyến đi này chưa
            try
            {
                HasReviewed = await _reviewRepository.HasUserReviewedRentalAsync(id.Value, userId);
            }
            catch
            {
                // Nếu phương thức không tồn tại, sử dụng phương thức dự phòng (kiểm tra đánh giá theo xe)
                HasReviewed = await _reviewRepository.HasUserReviewedCarAsync(userId, Rental.CarId);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rental = await _rentalRepository.GetByIdAsync(id);

            if (rental == null || rental.UserId != userId)
            {
                return NotFound();
            }

            if (rental.Status != (int)RentalStatus.Pending)
            {
                TempData["ErrorMessage"] = "Chỉ có thể hủy đơn hàng đang ở trạng thái chờ xác nhận.";
                return RedirectToPage("./Details", new { id });
            }

            await _rentalRepository.CancelRentalAsync(id);
            TempData["SuccessMessage"] = "Đã hủy đơn thuê xe thành công!";

            return RedirectToPage("./Details", new { id });
        }

        public async Task<IActionResult> OnPostPayWithCashAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rental = await _rentalRepository.GetByIdAsync(id);

            if (rental == null || rental.UserId != userId)
                return NotFound();

            if (rental.Status != 1 || rental.PaymentStatus == "Paid" || rental.PaymentStatus == "Pending")
            {
                TempData["ErrorMessage"] = "Chỉ có thể thanh toán các đơn đã được xác nhận và chưa thanh toán hoặc chưa có yêu cầu thanh toán đang chờ.";
                return RedirectToPage(new { id });
            }

            try
            {
                // Tạo bản ghi thanh toán tiền mặt với trạng thái Pending
                var payment = new Payment
                {
                    RentalId = rental.Id,
                    Amount = rental.TotalPrice,
                    Method = "Tiền mặt",
                    Status = "Pending", // Đặt trạng thái Pending
                    PaymentDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _rentalRepository.AddPendingPaymentAsync(payment);

                TempData["SuccessMessage"] = "Yêu cầu thanh toán tiền mặt đã được gửi, vui lòng chờ chủ xe xác nhận!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi thanh toán: {ex.Message}";
            }

            return RedirectToPage(new { id });
        }
    }
}