using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GotoCarRental.Areas.Admin.Pages.Features
{
    [Authorize(Roles = "Admin,Manager")]
    public class IndexModel : PageModel
    {
        private readonly IFeatureRepository _featureRepository;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IFeatureRepository featureRepository, ILogger<IndexModel> logger)
        {
            _featureRepository = featureRepository;
            _logger = logger;
        }

        public IEnumerable<Feature> Features { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Features = await _featureRepository.GetAllAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                await _featureRepository.DeleteAsync(id);
                TempData["SuccessMessage"] = "Tính năng đã được xóa thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa tính năng");
                TempData["ErrorMessage"] = $"Không thể xóa tính năng: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}
