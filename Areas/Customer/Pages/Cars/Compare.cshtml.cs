using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GotoCarRental.Areas.Customer.Pages.Cars
{
    public class CompareModel : PageModel
    {
        private readonly IComparisonRepository _comparisonRepository;

        public CompareModel(IComparisonRepository comparisonRepository)
        {
            _comparisonRepository = comparisonRepository;
        }

        public CarComparisonViewModel ComparisonViewModel { get; set; } = new CarComparisonViewModel();

        public async Task<IActionResult> OnGetAsync(string ids)
        {
            if (string.IsNullOrEmpty(ids))
            {
                return RedirectToPage("/Cars/Index", new { area = "Customer" });
            }

            try
            {
                // Parse car IDs from query string
                var carIdsList = ids.Split(',')
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Select(s => int.Parse(s.Trim()))
                    .ToList();

                if (!carIdsList.Any() || carIdsList.Count > 4) // Limit to 4 cars for comparison
                {
                    TempData["ErrorMessage"] = "Vui lòng chọn từ 1 đến 4 xe để so sánh.";
                    return RedirectToPage("/Cars/Index", new { area = "Customer" });
                }

                // Get cars and features for comparison
                ComparisonViewModel.Cars = (await _comparisonRepository.GetCarsForComparisonAsync(carIdsList)).ToList();
                ComparisonViewModel.AllFeatures = (await _comparisonRepository.GetAllFeaturesForCarsAsync(carIdsList)).ToList();

                return Page();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return RedirectToPage("/Cars/Index", new { area = "Customer" });
            }
        }
    }
}
