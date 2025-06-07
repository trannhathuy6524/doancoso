using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GotoCarRental.Areas.Admin.Pages.Categories
{
    [Authorize(Roles = "Admin,Manager")]
    public class DeleteModel : PageModel
    {
        private readonly ICategoryRepository _categoryRepository;

        public DeleteModel(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        [BindProperty]
        public Category Category { get; set; }
        public int CarCount { get; set; }
        public bool HasCars => CarCount > 0;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Category = await _categoryRepository.GetByIdAsync(id);
            if (Category == null)
            {
                return NotFound();
            }

            // Kiểm tra xem loại xe có xe nào không
            CarCount = await _categoryRepository.GetCarCountAsync(id);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var id = Category.Id;
            Category = await _categoryRepository.GetByIdAsync(id);

            if (Category == null)
            {
                return NotFound();
            }

            // Kiểm tra lại xem loại có xe không
            var carCount = await _categoryRepository.GetCarCountAsync(id);
            if (carCount > 0)
            {
                TempData["ErrorMessage"] = "Không thể xóa loại xe này vì có xe thuộc loại đang tồn tại.";
                return RedirectToPage(new { id });
            }

            try
            {
                await _categoryRepository.DeleteAsync(id);
                TempData["SuccessMessage"] = "Xóa loại xe thành công!";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa loại xe: {ex.Message}";
                return RedirectToPage(new { id });
            }
        }
    }
}
