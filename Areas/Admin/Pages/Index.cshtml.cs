using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GotoCarRental.Areas.Admin.Pages
{
    [Authorize(Roles = "Admin,Manager")]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICarRepository _carRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IRentalRepository _rentalRepository;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            ICarRepository carRepository,
            IBrandRepository brandRepository,
            ICategoryRepository categoryRepository,
            IRentalRepository rentalRepository)
        {
            _userManager = userManager;
            _carRepository = carRepository;
            _brandRepository = brandRepository;
            _categoryRepository = categoryRepository;
            _rentalRepository = rentalRepository;
        }

        public int TotalUsers { get; set; }
        public int TotalCars { get; set; }
        public int TotalBrands { get; set; }
        public int TotalCategories { get; set; }
        public int PendingApprovalCars { get; set; }
        public int PendingApprovalUsers { get; set; }

        // Thống kê mới
        public int TotalRentals { get; set; }
        public int PendingRentals { get; set; }
        public int CompletedRentals { get; set; }
        public int CancelledRentals { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCommission { get; set; }


        // Dữ liệu cho biểu đồ
        public string MonthlyRentalsJson { get; set; }
        public string BrandDistributionJson { get; set; }
        public string CategoryDistributionJson { get; set; }
        public string RentalStatusChartJson { get; set; }
        public string WeeklyRevenueJson { get; set; }

        public async Task OnGetAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            TotalUsers = users.Count;
            PendingApprovalUsers = users.Count(u => !u.IsApproved);

            var cars = await _carRepository.GetAllAsync();
            TotalCars = cars.Count();
            PendingApprovalCars = cars.Count(c => !c.IsApproved);

            var brands = await _brandRepository.GetAllAsync();
            TotalBrands = brands.Count();

            var categories = await _categoryRepository.GetAllAsync();
            TotalCategories = categories.Count();

            // Lấy thống kê về đơn thuê
            var allRentals = await _rentalRepository.GetAllAsync();
            TotalRentals = allRentals.Count();
            PendingRentals = allRentals.Count(r => r.Status == 0); // Chờ xác nhận
            CompletedRentals = allRentals.Count(r => r.Status == 3); // Đã hoàn thành
            CancelledRentals = allRentals.Count(r => r.Status == 2); // Đã hủy
            TotalRevenue = allRentals.Where(r => r.Status == 3 && r.PaymentStatus == "Paid").Sum(r => r.TotalPrice);
            // Lấy tổng số tiền hoa hồng và đơn hàng hoàn thành
            var completedRentals = await _rentalRepository.GetAllCompletedRentalsAsync();
            TotalCommission = completedRentals.Sum(r => r.CommissionAmount);

            // Tạo dữ liệu cho biểu đồ đơn thuê theo tháng (6 tháng gần nhất)
            var lastSixMonths = Enumerable.Range(0, 6)
                .Select(i => DateTime.Now.AddMonths(-i))
                .Select(date => new { Month = date.Month, Year = date.Year })
                .ToList();

            var monthlyRentals = lastSixMonths.Select(m => new
            {
                Month = $"{m.Month}/{m.Year}",
                Count = allRentals.Count(r => r.CreatedAt.Month == m.Month && r.CreatedAt.Year == m.Year)
            }).Reverse().ToList();

            MonthlyRentalsJson = JsonSerializer.Serialize(monthlyRentals);

            // Tạo dữ liệu cho biểu đồ phân bố theo hãng xe
            var brandDistribution = brands.Select(b => new
            {
                Name = b.Name,
                Count = cars.Count(c => c.BrandId == b.Id)
            }).Where(b => b.Count > 0).ToList();

            BrandDistributionJson = JsonSerializer.Serialize(brandDistribution);

            // Tạo dữ liệu cho biểu đồ phân bố theo loại xe
            var categoryDistribution = categories.Select(c => new
            {
                Name = c.Name,
                Count = cars.Count(car => car.CategoryId == c.Id)
            }).Where(c => c.Count > 0).ToList();

            CategoryDistributionJson = JsonSerializer.Serialize(categoryDistribution);

            // Tạo dữ liệu cho biểu đồ trạng thái đơn thuê
            var rentalStatusChart = new[]
            {
                new { Status = "Đang chờ xác nhận", Count = PendingRentals },
                new { Status = "Đã xác nhận", Count = allRentals.Count(r => r.Status == 1) },
                new { Status = "Đã hủy", Count = CancelledRentals },
                new { Status = "Đã hoàn thành", Count = CompletedRentals }
            };

            RentalStatusChartJson = JsonSerializer.Serialize(rentalStatusChart);

            // Tạo dữ liệu cho biểu đồ doanh thu theo tuần (4 tuần gần nhất)
            var lastFourWeeks = Enumerable.Range(0, 4)
                .Select(i => DateTime.Now.AddDays(-7 * i))
                .Select(date => new {
                    WeekStart = date.AddDays(-(int)date.DayOfWeek),
                    WeekEnd = date.AddDays(6 - (int)date.DayOfWeek)
                })
                .ToList();

            var weeklyRevenue = lastFourWeeks.Select(w => new
            {
                Week = $"{w.WeekStart:dd/MM} - {w.WeekEnd:dd/MM}",
                Revenue = allRentals.Where(r => r.CreatedAt >= w.WeekStart && r.CreatedAt <= w.WeekEnd && r.Status == 3 && r.PaymentStatus == "Paid")
                    .Sum(r => r.TotalPrice)
            }).Reverse().ToList();
            

            WeeklyRevenueJson = JsonSerializer.Serialize(weeklyRevenue);
        }
    }
}