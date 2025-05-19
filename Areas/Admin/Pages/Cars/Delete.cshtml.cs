using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GotoCarRental.Areas.Admin.Pages.Cars
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly ICarRepository _carRepository;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(ICarRepository carRepository, ILogger<DeleteModel> logger)
        {
            _carRepository = carRepository;
            _logger = logger;
        }

        [BindProperty]
        public Car Car { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Car = await _carRepository.GetByIdAsync(id);

            if (Car == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            try
            {
                await _carRepository.DeleteAsync(id);
                TempData["SuccessMessage"] = "Xe đã được xóa thành công!";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa xe ID {Id}", id);
                TempData["ErrorMessage"] = $"Có lỗi xảy ra khi xóa xe: {ex.Message}";
                return RedirectToPage("./Delete", new { id });
            }
        }
    }
}
