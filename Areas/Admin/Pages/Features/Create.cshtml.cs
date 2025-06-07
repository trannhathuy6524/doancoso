using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GotoCarRental.Areas.Admin.Pages.Features
{
    [Authorize(Roles = "Admin,Manager")]
    public class CreateModel : PageModel
    {
        private readonly IFeatureRepository _featureRepository;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(IFeatureRepository featureRepository, ILogger<CreateModel> logger)
        {
            _featureRepository = featureRepository;
            _logger = logger;
        }

        [BindProperty]
        public Feature Feature { get; set; }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Kiểm tra nếu ModelState có lỗi
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Khởi tạo giá trị cho navigation property
            Feature.CarFeatures = new List<CarFeature>();

            try
            {
                var createdFeature = await _featureRepository.AddAsync(Feature);
                TempData["SuccessMessage"] = $"Tính năng '{createdFeature.Name}' đã được thêm thành công!";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm tính năng mới: {Name}", Feature.Name);

                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += $" - {ex.InnerException.Message}";
                    _logger.LogError(ex.InnerException, "Inner exception");
                }

                ModelState.AddModelError("", $"Không thể thêm tính năng: {errorMessage}");
                return Page();
            }
        }

    }
}
