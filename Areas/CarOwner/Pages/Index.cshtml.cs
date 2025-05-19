using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GotoCarRental.Areas.CarOwner.Pages
{
    [Authorize(Roles = "CarOwner")]
    public class IndexModel : PageModel
    {
        private readonly ICarRepository _carRepository;
        private readonly IRentalRepository _rentalRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(
            ICarRepository carRepository,
            IRentalRepository rentalRepository,
            UserManager<ApplicationUser> userManager)
        {
            _carRepository = carRepository;
            _rentalRepository = rentalRepository;
            _userManager = userManager;
        }

        public IEnumerable<Car> MyCars { get; set; }
        public int TotalCars { get; set; }
        public int AvailableCars { get; set; }
        public int PendingApproval { get; set; }

        // Thêm các thuộc tính mới cho thống kê đơn thuê
        public int TotalRentals { get; set; }
        public int PendingRentals { get; set; }
        public int ConfirmedRentals { get; set; }
        public int CompletedRentals { get; set; }
        public decimal TotalEarnings { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Lấy thống kê xe
            MyCars = await _carRepository.GetCarsByUserAsync(userId);
            TotalCars = MyCars.Count();
            AvailableCars = MyCars.Count(c => c.IsApproved && c.IsAvailable);
            PendingApproval = MyCars.Count(c => !c.IsApproved);

            // Lấy danh sách ID xe của chủ xe hiện tại
            var carIds = MyCars.Select(c => c.Id).ToList();

            // Lấy tất cả các đơn thuê liên quan đến xe của người dùng
            var rentals = (await _rentalRepository.GetAllAsync())
                .Where(r => carIds.Contains(r.CarId))
                .ToList();

            // Tính toán thống kê đơn thuê
            TotalRentals = rentals.Count;
            PendingRentals = rentals.Count(r => r.Status == 0); // Chờ xác nhận
            ConfirmedRentals = rentals.Count(r => r.Status == 1); // Đã xác nhận
            CompletedRentals = rentals.Count(r => r.Status == 3); // Hoàn thành
            TotalEarnings = rentals
                .Where(r => r.Status == 1 || r.Status == 3) // Đã xác nhận hoặc hoàn thành
                .Sum(r => r.TotalPrice);

            return Page();
        }
    }


}
