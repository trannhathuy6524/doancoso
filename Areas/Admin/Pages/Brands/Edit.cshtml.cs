using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GotoCarRental.Areas.Admin.Pages.Brands
{
    [Authorize(Roles = "Admin,Manager")]
    public class EditModel : PageModel
    {
        private readonly IBrandRepository _brandRepository;

        public EditModel(IBrandRepository brandRepository)
        {
            _brandRepository = brandRepository;
        }

        [BindProperty]
        public Brand Brand { get; set; }
        public int CarCount { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Brand = await _brandRepository.GetByIdAsync(id);
            if (Brand == null)
            {
                return NotFound();
            }

            // Lấy số lượng xe thuộc hãng này
            CarCount = await _brandRepository.GetCarCountAsync(id);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                CarCount = await _brandRepository.GetCarCountAsync(Brand.Id);
                return Page();
            }

            // Kiểm tra trùng tên
            var existingBrand = await _brandRepository.GetByIdAsync(Brand.Id);
            if (existingBrand.Name != Brand.Name)
            {
                if (await _brandRepository.ExistsByNameAsync(Brand.Name))
                {
                    ModelState.AddModelError("Brand.Name", "Tên hãng xe này đã tồn tại.");
                    CarCount = await _brandRepository.GetCarCountAsync(Brand.Id);
                    return Page();
                }
            }

            try
            {
                await _brandRepository.UpdateAsync(Brand);
                TempData["SuccessMessage"] = "Cập nhật hãng xe thành công!";
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật hãng xe.");
                CarCount = await _brandRepository.GetCarCountAsync(Brand.Id);
                return Page();
            }

            return RedirectToPage("./Index");
        }
    }
}
