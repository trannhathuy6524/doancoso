using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GotoCarRental.Areas.Admin.Pages.Brands
{
    [Authorize(Roles = "Admin,Manager")]
    public class CreateModel : PageModel
    {
        private readonly IBrandRepository _brandRepository;

        public CreateModel(IBrandRepository brandRepository)
        {
            _brandRepository = brandRepository;
        }

        [BindProperty]
        public Brand Brand { get; set; }

        public void OnGet()
        {
            Brand = new Brand
            {
                Cars = new List<Car>() // Khởi tạo collection rỗng
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Kiểm tra tên hãng có trùng không
            if (await _brandRepository.ExistsByNameAsync(Brand.Name))
            {
                ModelState.AddModelError("Brand.Name", "Tên hãng xe này đã tồn tại.");
                return Page();
            }

            await _brandRepository.AddAsync(Brand);
            TempData["SuccessMessage"] = "Thêm hãng xe mới thành công!";
            return RedirectToPage("./Index");
        }
    }
}
