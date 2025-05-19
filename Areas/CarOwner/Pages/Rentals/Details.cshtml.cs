using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GotoCarRental.Areas.CarOwner.Pages.Rentals
{
    [Authorize(Roles = "CarOwner")]
    public class DetailsModel : PageModel
    {
        private readonly IRentalRepository _rentalRepository;
        private readonly ICarRepository _carRepository;

        public DetailsModel(IRentalRepository rentalRepository, ICarRepository carRepository)
        {
            _rentalRepository = rentalRepository;
            _carRepository = carRepository;
        }

        public Rental Rental { get; set; }
        public Car Car { get; set; }
        public IEnumerable<Payment> Payments { get; set; }
        public int RentalDays { get; set; }
        public bool CanConfirm { get; set; }
        public bool CanCancel { get; set; }
        public bool CanComplete { get; set; }
        public bool CanConfirmPayment { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Rental = await _rentalRepository.GetByIdAsync(id.Value);

            if (Rental == null)
                return NotFound();

            // Lấy thông tin xe và kiểm tra người dùng có phải chủ xe không
            Car = await _carRepository.GetByIdAsync(Rental.CarId);
            if (Car == null || Car.UserId != userId)
                return Forbid();

            // Lấy danh sách thanh toán
            Payments = await _rentalRepository.GetPaymentsForRentalAsync(id.Value);

            // Tính số ngày thuê
            RentalDays = (Rental.EndDate - Rental.StartDate).Days + 1;

            // Kiểm tra quyền thao tác với đơn
            CanConfirm = Rental.Status == 0; // Chờ xác nhận
            CanCancel = Rental.Status == 0 || Rental.Status == 1; // Chờ xác nhận hoặc đã xác nhận
            CanComplete = Rental.Status == 1; // Đã xác nhận
                                              // Thêm điều kiện kiểm tra Status != 2 (không phải đơn đã hủy)
            CanConfirmPayment = Rental.PaymentStatus == "Pending" && Rental.Status != 2;

            return Page();
        }

        public async Task<IActionResult> OnPostConfirmPaymentAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rental = await _rentalRepository.GetByIdAsync(id);

            if (rental == null)
                return NotFound();

            // Kiểm tra xem người dùng có sở hữu xe này không
            var car = await _carRepository.GetByIdAsync(rental.CarId);
            if (car == null || car.UserId != userId)
                return Forbid();

            // Lấy thanh toán gần nhất có trạng thái Pending
            var payments = await _rentalRepository.GetPaymentsForRentalAsync(id);

            // Debug thông tin
            foreach (var p in payments)
            {
                System.Diagnostics.Debug.WriteLine($"Payment ID: {p.PaymentId}, Status: '{p.Status}', Method: {p.Method}");
            }

            // Tìm kiếm không phân biệt chữ hoa/chữ thường
            var pendingPayment = payments.FirstOrDefault(p =>
                string.Equals(p.Status, "Pending", StringComparison.OrdinalIgnoreCase));

            if (pendingPayment != null)
            {
                try
                {
                    // Cập nhật trạng thái thanh toán
                    await _rentalRepository.ConfirmPaymentAsync(pendingPayment.PaymentId);
                    TempData["SuccessMessage"] = "Xác nhận thanh toán thành công!";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Lỗi khi xác nhận thanh toán: {ex.Message}";
                }
            }
            else
            {
                // Nếu không tìm thấy thanh toán nhưng trạng thái của đơn hàng là "Pending"
                if (rental.PaymentStatus == "Pending")
                {
                    try
                    {
                        // Tạo mới một Payment và xác nhận ngay
                        var newPayment = new Payment
                        {
                            RentalId = rental.Id,
                            Amount = rental.TotalPrice,
                            Method = rental.PaymentMethod ?? "Tiền mặt",
                            Status = "Completed",
                            PaymentDate = DateTime.UtcNow
                        };

                        await _rentalRepository.AddPaymentAsync(newPayment);

                        // Cập nhật trạng thái rental thành "Paid"
                        rental.PaymentStatus = "Paid";
                        await _rentalRepository.UpdateAsync(rental);

                        TempData["SuccessMessage"] = "Đã tự động tạo và xác nhận thanh toán!";
                        return RedirectToPage(new { id });
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMessage"] = $"Không thể tự động tạo thanh toán: {ex.Message}";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = $"Không tìm thấy thông tin thanh toán đang chờ xác nhận! Trạng thái hiện tại: {rental.PaymentStatus}";
                }
            }

            return RedirectToPage(new { id });
        }



        public async Task<IActionResult> OnPostConfirmAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rental = await _rentalRepository.GetByIdAsync(id);

            if (rental == null)
                return NotFound();

            // Kiểm tra xem người dùng có sở hữu xe này không
            var car = await _carRepository.GetByIdAsync(rental.CarId);
            if (car == null || car.UserId != userId)
                return Forbid();

            if (rental.Status != 0)
            {
                TempData["ErrorMessage"] = "Chỉ có thể xác nhận những đơn đang chờ xác nhận.";
                return RedirectToPage(new { id });
            }

            await _rentalRepository.ConfirmRentalAsync(id);
            TempData["SuccessMessage"] = "Xác nhận đơn thuê xe thành công!";

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rental = await _rentalRepository.GetByIdAsync(id);

            if (rental == null)
                return NotFound();

            // Kiểm tra xem người dùng có sở hữu xe này không
            var car = await _carRepository.GetByIdAsync(rental.CarId);
            if (car == null || car.UserId != userId)
                return Forbid();

            if (rental.Status != 0 && rental.Status != 1)
            {
                TempData["ErrorMessage"] = "Chỉ có thể hủy những đơn đang chờ xác nhận hoặc đã xác nhận.";
                return RedirectToPage(new { id });
            }

            await _rentalRepository.CancelRentalAsync(id);
            TempData["SuccessMessage"] = "Hủy đơn thuê xe thành công!";

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostCompleteAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rental = await _rentalRepository.GetByIdAsync(id);

            if (rental == null)
                return NotFound();

            // Kiểm tra xem người dùng có sở hữu xe này không
            var car = await _carRepository.GetByIdAsync(rental.CarId);
            if (car == null || car.UserId != userId)
                return Forbid();

            if (rental.Status != 1)
            {
                TempData["ErrorMessage"] = "Chỉ có thể hoàn thành những đơn đã xác nhận.";
                return RedirectToPage(new { id });
            }

            await _rentalRepository.CompleteRentalAsync(id);
            TempData["SuccessMessage"] = "Đơn thuê xe đã hoàn thành thành công!";

            return RedirectToPage(new { id });
        }


    }
}
