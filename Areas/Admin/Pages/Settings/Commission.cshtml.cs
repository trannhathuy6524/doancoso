using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace GotoCarRental.Areas.Admin.Pages.Settings
{
    [Authorize(Roles = "Admin,Manager")]
    public class CommissionModel : PageModel
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IRentalRepository _rentalRepository;

        public CommissionModel(ICategoryRepository categoryRepository, IRentalRepository rentalRepository)
        {
            _categoryRepository = categoryRepository;
            _rentalRepository = rentalRepository;
        }

        [BindProperty]
        [Range(0, 100, ErrorMessage = "Tỷ lệ hoa hồng phải từ 0 đến 100%")]
        [Display(Name = "Tỷ lệ hoa hồng mặc định")]
        public decimal DefaultCommissionRate { get; set; } = 10;

        [BindProperty]
        public List<CategoryCommissionRate> CategoryCommissionRates { get; set; } = new List<CategoryCommissionRate>();

        public class CategoryCommissionRate
        {
            public int CategoryId { get; set; }
            public string CategoryName { get; set; }

            [Range(0, 100, ErrorMessage = "Tỷ lệ hoa hồng phải từ 0 đến 100%")]
            public decimal CommissionRate { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Lấy tỷ lệ mặc định từ cài đặt
            DefaultCommissionRate = await _rentalRepository.GetDefaultCommissionRateAsync();

            // Lấy tất cả các loại xe
            var categories = await _categoryRepository.GetAllAsync();
            CategoryCommissionRates.Clear();

            foreach (var category in categories)
            {
                var rate = await _rentalRepository.GetCategoryCommissionRateAsync(category.Id);
                CategoryCommissionRates.Add(new CategoryCommissionRate
                {
                    CategoryId = category.Id,
                    CategoryName = category.Name,
                    CommissionRate = rate ?? DefaultCommissionRate // Nếu không có tỷ lệ riêng, dùng tỷ lệ mặc định
                });
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Cập nhật tỷ lệ hoa hồng mặc định
            await _rentalRepository.SetDefaultCommissionRateAsync(DefaultCommissionRate);

            // Cập nhật tỷ lệ hoa hồng cho từng loại xe
            foreach (var rate in CategoryCommissionRates)
            {
                await _rentalRepository.SetCategoryCommissionRateAsync(rate.CategoryId, rate.CommissionRate);
            }

            TempData["SuccessMessage"] = "Đã cập nhật thành công các tỷ lệ hoa hồng.";
            return RedirectToPage();
        }
    }
}