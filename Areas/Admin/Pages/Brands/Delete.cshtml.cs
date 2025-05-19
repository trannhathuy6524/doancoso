using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GotoCarRental.Areas.Admin.Pages.Brands
{
    [Authorize(Roles = "Admin,Manager")]
    public class DeleteModel : PageModel
    {
        private readonly IBrandRepository _brandRepository;

        public DeleteModel(IBrandRepository brandRepository)
        {
            _brandRepository = brandRepository;
        }

        [BindProperty]
        public Brand Brand { get; set; }
        public int CarCount { get; set; }
        public bool HasCars => CarCount > 0;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Brand = await _brandRepository.GetByIdAsync(id);
            if (Brand == null)
            {
                return NotFound();
            }

            // Kiểm tra xem hãng có xe không
            CarCount = await _brandRepository.GetCarCountAsync(id);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var id = Brand.Id;
            Brand = await _brandRepository.GetByIdAsync(id);

            if (Brand == null)
            {
                return NotFound();
            }

            // Kiểm tra lại xem hãng có xe không
            var carCount = await _brandRepository.GetCarCountAsync(id);
            if (carCount > 0)
            {
                TempData["ErrorMessage"] = "Không thể xóa hãng xe này vì có xe thuộc hãng đang tồn tại.";
                return RedirectToPage(new { id });
            }

            try
            {
                await _brandRepository.DeleteAsync(id);
                TempData["SuccessMessage"] = "Xóa hãng xe thành công!";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa hãng xe: {ex.Message}";
                return RedirectToPage(new { id });
            }
        }
    }
}
