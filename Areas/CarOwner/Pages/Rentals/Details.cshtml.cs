using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GotoCarRental.Areas.CarOwner.Pages.Rentals
{
    [Authorize(Roles = "CarOwner")]
    public class DetailsModel : PageModel
    {
        private readonly IRentalRepository _rentalRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public DetailsModel(IRentalRepository rentalRepository, UserManager<ApplicationUser> userManager)
        {
            _rentalRepository = rentalRepository;
            _userManager = userManager;
        }

        public Rental Rental { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            Rental = await _rentalRepository.GetByIdAsync(id);

            if (Rental == null || Rental.Car.UserId != currentUserId)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostConfirmAsync(int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            var rental = await _rentalRepository.GetByIdAsync(id);

            if (rental == null || rental.Car.UserId != currentUserId)
            {
                return NotFound();
            }

            if (rental.Status != (int)Rental.RentalStatus.Pending)
            {
                TempData["ErrorMessage"] = "Chỉ có thể xác nhận đơn hàng đang ở trạng thái chờ xác nhận.";
                return RedirectToPage("./Details", new { id });
            }

            await _rentalRepository.UpdateStatusAsync(id, (int)Rental.RentalStatus.Confirmed);
            TempData["SuccessMessage"] = "Đã xác nhận đơn thuê xe thành công!";

            return RedirectToPage("./Details", new { id });
        }

        public async Task<IActionResult> OnPostRejectAsync(int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            var rental = await _rentalRepository.GetByIdAsync(id);

            if (rental == null || rental.Car.UserId != currentUserId)
            {
                return NotFound();
            }

            if (rental.Status != (int)Rental.RentalStatus.Pending)
            {
                TempData["ErrorMessage"] = "Chỉ có thể từ chối đơn hàng đang ở trạng thái chờ xác nhận.";
                return RedirectToPage("./Details", new { id });
            }

            await _rentalRepository.CancelRentalAsync(id);
            TempData["SuccessMessage"] = "Đã từ chối đơn thuê xe!";

            return RedirectToPage("./Details", new { id });
        }

        public async Task<IActionResult> OnPostStartRentalAsync(int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            var rental = await _rentalRepository.GetByIdAsync(id);

            if (rental == null || rental.Car.UserId != currentUserId)
            {
                return NotFound();
            }

            if (rental.Status != (int)Rental.RentalStatus.Confirmed)
            {
                TempData["ErrorMessage"] = "Chỉ có thể bắt đầu đơn hàng đã được xác nhận.";
                return RedirectToPage("./Details", new { id });
            }

            await _rentalRepository.UpdateStatusAsync(id, (int)Rental.RentalStatus.InProgress);
            TempData["SuccessMessage"] = "Đã bắt đầu thuê xe!";

            return RedirectToPage("./Details", new { id });
        }

        public async Task<IActionResult> OnPostCompleteRentalAsync(int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            var rental = await _rentalRepository.GetByIdAsync(id);

            if (rental == null || rental.Car.UserId != currentUserId)
            {
                return NotFound();
            }

            if (rental.Status != (int)Rental.RentalStatus.InProgress)
            {
                TempData["ErrorMessage"] = "Chỉ có thể hoàn thành đơn hàng đang thực hiện.";
                return RedirectToPage("./Details", new { id });
            }

            await _rentalRepository.UpdateStatusAsync(id, (int)Rental.RentalStatus.Completed);
            TempData["SuccessMessage"] = "Đã hoàn thành dịch vụ thuê xe!";

            return RedirectToPage("./Details", new { id });
        }

        // Thêm handler mới để cập nhật thông tin tài xế
        public async Task<IActionResult> OnPostUpdateDriverInfoAsync(int id, string driverName, string driverPhone)
        {
            var currentUserId = _userManager.GetUserId(User);
            var rental = await _rentalRepository.GetByIdAsync(id);

            if (rental == null || rental.Car.UserId != currentUserId)
            {
                return NotFound();
            }

            // Cập nhật thông tin tài xế
            rental.DriverName = driverName;
            rental.DriverPhone = driverPhone;

            await _rentalRepository.UpdateAsync(rental);

            TempData["SuccessMessage"] = "Đã cập nhật thông tin tài xế thành công!";
            return RedirectToPage("./Details", new { id });
        }
    }
}