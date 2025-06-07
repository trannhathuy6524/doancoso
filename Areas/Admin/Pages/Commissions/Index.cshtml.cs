using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GotoCarRental.Areas.Admin.Pages.Commissions
{
    // Định nghĩa ChartData bên ngoài lớp IndexModel
    public class ChartData
    {
        public string[] Labels { get; set; }
        public decimal[] Data { get; set; }
    }

    [Authorize(Roles = "Admin,Manager")]
    public class IndexModel : PageModel
    {
        private readonly IRentalRepository _rentalRepository;

        public IndexModel(IRentalRepository rentalRepository)
        {
            _rentalRepository = rentalRepository;
        }

        public decimal TotalCommission { get; set; }
        public int CompletedRentals { get; set; }
        public DateTime CurrentMonth { get; set; } = DateTime.Now;
        public List<Rental> Rentals { get; set; } = new List<Rental>();

        // Dữ liệu cho biểu đồ
        public ChartData ChartData { get; set; }

        public async Task<IActionResult> OnGetAsync(string month = null)
        {
            // Xử lý tham số month nếu được cung cấp
            if (!string.IsNullOrEmpty(month))
            {
                try
                {
                    // Phân tích giá trị từ input type="month" (format: yyyy-MM)
                    var parts = month.Split('-');
                    if (parts.Length == 2 && int.TryParse(parts[0], out int year) && int.TryParse(parts[1], out int monthNumber))
                    {
                        CurrentMonth = new DateTime(year, monthNumber, 1);
                    }
                }
                catch
                {
                    // Nếu có lỗi, sử dụng tháng hiện tại
                    CurrentMonth = DateTime.Now;
                }
            }
            else
            {
                // Nếu không có tham số month, sử dụng tháng hiện tại
                CurrentMonth = DateTime.Now;
            }
            
            // Lấy tất cả các đơn thuê đã hoàn thành
            var completedRentals = await _rentalRepository.GetAllCompletedRentalsAsync();

            // Tính tổng hoa hồng và số đơn hoàn thành
            TotalCommission = completedRentals.Sum(r => r.CommissionAmount);
            CompletedRentals = completedRentals.Count();

            // Lọc đơn thuê theo tháng hiện tại nếu cần
            var startOfMonth = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            Rentals = completedRentals
                .Where(r => r.UpdatedAt.Date >= startOfMonth && r.UpdatedAt.Date <= endOfMonth)
                .OrderByDescending(r => r.UpdatedAt)
                .ToList();

            // Tạo dữ liệu cho biểu đồ (6 tháng gần đây)
            await PrepareChartData();

            return Page();
        }

        private async Task PrepareChartData()
        {
            // Lấy dữ liệu hoa hồng theo tháng trong 6 tháng gần đây
            var lastSixMonths = Enumerable.Range(0, 6)
                .Select(i => DateTime.Now.AddMonths(-i))
                .Select(date => new DateTime(date.Year, date.Month, 1))
                .Reverse()
                .ToArray();

            var labels = lastSixMonths
                .Select(date => date.ToString("MM/yyyy"))
                .ToArray();

            // Lấy tất cả đơn thuê đã hoàn thành
            var allCompletedRentals = await _rentalRepository.GetAllCompletedRentalsAsync();

            var monthlyCommission = new decimal[6];

            for (int i = 0; i < 6; i++)
            {
                DateTime startOfMonth = lastSixMonths[i];
                DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                // Tính tổng hoa hồng cho tháng này
                monthlyCommission[i] = allCompletedRentals
                    .Where(r => r.UpdatedAt.Date >= startOfMonth && r.UpdatedAt.Date <= endOfMonth)
                    .Sum(r => r.CommissionAmount);
            }

            ChartData = new ChartData
            {
                Labels = labels,
                Data = monthlyCommission
            };
        }
    }
}