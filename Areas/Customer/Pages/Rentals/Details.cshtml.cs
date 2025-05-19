using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace GotoCarRental.Areas.Customer.Pages.Rentals
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IRentalRepository _rentalRepository;

        public DetailsModel(IRentalRepository rentalRepository) // Cập nhật constructor
        {
            _rentalRepository = rentalRepository;
        }

        public Rental Rental { get; set; }
        public IEnumerable<Payment> Payments { get; set; }
        public int RentalDays { get; set; }
        public bool CanCancel { get; set; }
        public bool CanReview { get; set; }
        public bool CanPayWithCash { get; set; } // Thêm thuộc tính này
        public string PaymentStatus { get; set; } // Thêm thuộc tính này


        // Trong phương thức OnGetAsync của DetailsModel
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // Giữ nguyên code hiện tại
            if (id == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Rental = await _rentalRepository.GetByIdAsync(id.Value);

            if (Rental == null || Rental.UserId != userId)
                return NotFound();

            // Tính số ngày thuê
            RentalDays = (Rental.EndDate - Rental.StartDate).Days + 1;
            if (RentalDays < 1) RentalDays = 1;

            // Kiểm tra và cập nhật tổng tiền nếu nó bằng 0
            if (Rental.TotalPrice == 0 && Rental.Car != null && Rental.Car.PricePerDay > 0)
            {
                Rental.TotalPrice = Rental.Car.PricePerDay * RentalDays;
                await _rentalRepository.UpdateAsync(Rental);
            }

            // Lấy danh sách thanh toán
            Payments = await _rentalRepository.GetPaymentsForRentalAsync(id.Value);

            // Kiểm tra quyền hủy đơn: chỉ hủy đơn chờ xác nhận
            CanCancel = Rental.Status == 0;

            // Kiểm tra quyền đánh giá: chỉ đánh giá đơn đã hoàn thành
            CanReview = Rental.Status == 3;

            // Kiểm tra nếu có thể thanh toán bằng Tienf mặt
            // Cập nhật điều kiện hiển thị nút thanh toán
            CanPayWithCash = (Rental.Status == 1 && Rental.PaymentStatus != "Paid" && Rental.PaymentStatus != "Pending");


            return Page();
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            // Giữ nguyên code hiện tại
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rental = await _rentalRepository.GetByIdAsync(id);

            if (rental == null || rental.UserId != userId)
                return NotFound();

            if (rental.Status != 0)
            {
                TempData["ErrorMessage"] = "Chỉ có thể hủy những đơn đang chờ xác nhận.";
                return RedirectToPage(new { id });
            }

            await _rentalRepository.CancelRentalAsync(id);
            TempData["SuccessMessage"] = "Hủy đơn thuê xe thành công!";

            return RedirectToPage(new { id });
        }

        // Thêm phương thức thanh toán Momo
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
