using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GotoCarRental.Areas.Admin.Pages.Features
{
    [Authorize(Roles = "Admin,Manager")]
    public class EditModel : PageModel
    {
        private readonly IFeatureRepository _featureRepository;
        private readonly ILogger<EditModel> _logger;

        public EditModel(IFeatureRepository featureRepository, ILogger<EditModel> logger)
        {
            _featureRepository = featureRepository;
            _logger = logger;
        }

        [BindProperty]
        public Feature Feature { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Feature = await _featureRepository.GetByIdAsync(id.Value);

            if (Feature == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                await _featureRepository.UpdateAsync(Feature);
                TempData["SuccessMessage"] = "Tính năng đã được cập nhật thành công!";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật tính năng ID {Id}", Feature.Id);
                ModelState.AddModelError("", $"Không thể cập nhật tính năng: {ex.Message}");
                return Page();
            }
        }
    }
}
