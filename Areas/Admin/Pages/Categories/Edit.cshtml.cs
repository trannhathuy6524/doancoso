using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GotoCarRental.Areas.Admin.Pages.Categories
{
    [Authorize(Roles = "Admin,Manager")]
    public class EditModel : PageModel
    {
        private readonly ICategoryRepository _categoryRepository;

        public EditModel(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        [BindProperty]
        public Category Category { get; set; }
        public int CarCount { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Category = await _categoryRepository.GetByIdAsync(id);
            if (Category == null)
            {
                return NotFound();
            }

            // Lấy số lượng xe thuộc loại này
            CarCount = await _categoryRepository.GetCarCountAsync(id);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                CarCount = await _categoryRepository.GetCarCountAsync(Category.Id);
                return Page();
            }

            // Kiểm tra trùng tên
            var existingCategory = await _categoryRepository.GetByIdAsync(Category.Id);
            if (existingCategory.Name != Category.Name)
            {
                if (await _categoryRepository.ExistsByNameAsync(Category.Name))
                {
                    ModelState.AddModelError("Category.Name", "Tên loại xe này đã tồn tại.");
                    CarCount = await _categoryRepository.GetCarCountAsync(Category.Id);
                    return Page();
                }
            }

            try
            {
                await _categoryRepository.UpdateAsync(Category);
                TempData["SuccessMessage"] = "Cập nhật loại xe thành công!";
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật loại xe.");
                CarCount = await _categoryRepository.GetCarCountAsync(Category.Id);
                return Page();
            }

            return RedirectToPage("./Index");
        }
    }
}
