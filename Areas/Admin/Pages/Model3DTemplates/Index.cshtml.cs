using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GotoCarRental.Areas.Admin.Pages.Model3DTemplates
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IModel3DTemplateRepository _model3DTemplateRepository;
        private readonly IBrandRepository _brandRepository;

        public IndexModel(IModel3DTemplateRepository model3DTemplateRepository, IBrandRepository brandRepository)
        {
            _model3DTemplateRepository = model3DTemplateRepository;
            _brandRepository = brandRepository;
        }

        public IEnumerable<Model3DTemplate> Model3DTemplates { get; set; }
        public SelectList BrandSelectList { get; set; }

        public async Task OnGetAsync(int? brandId)
        {
            // Lấy danh sách mô hình 3D (có thể lọc theo brandId)
            if (brandId.HasValue)
            {
                Model3DTemplates = await _model3DTemplateRepository.GetByBrandAndCategoryAsync(brandId, null);
            }
            else
            {
                Model3DTemplates = await _model3DTemplateRepository.GetAllAsync();
            }

            // Lấy danh sách brand để hiển thị trong dropdown lọc
            var brands = await _brandRepository.GetAllAsync();
            BrandSelectList = new SelectList(brands, "Id", "Name", brandId);
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                await _model3DTemplateRepository.DeleteAsync(id);
                TempData["SuccessMessage"] = "Xóa mô hình 3D thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Xóa mô hình 3D thất bại: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}
