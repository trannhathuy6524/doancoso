using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GotoCarRental.Areas.Admin.Pages.Categories
{
    [Authorize(Roles = "Admin,Manager")]
    public class CreateModel : PageModel
    {
        private readonly ICategoryRepository _categoryRepository;

        public CreateModel(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        [BindProperty]
        public Category Category { get; set; }

        public void OnGet()
        {
            Category = new Category
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

            // Kiểm tra tên loại xe có trùng không
            if (await _categoryRepository.ExistsByNameAsync(Category.Name))
            {
                ModelState.AddModelError("Category.Name", "Tên loại xe này đã tồn tại.");
                return Page();
            }

            await _categoryRepository.AddAsync(Category);
            TempData["SuccessMessage"] = "Thêm loại xe mới thành công!";
            return RedirectToPage("./Index");
        }
    }
}
