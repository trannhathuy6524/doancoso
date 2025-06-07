using GotoCarRental.Data;
using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GotoCarRental.Areas.Admin.Pages.Model3DTemplates
{
    [Authorize(Roles = "Admin")]
    public class DetailsModel : PageModel
    {
        private readonly IModel3DTemplateRepository _model3DTemplateRepository;
        private readonly ICarRepository _carRepository;
        private readonly ApplicationDbContext _context;

        public DetailsModel(
            IModel3DTemplateRepository model3DTemplateRepository,
            ICarRepository carRepository,
            ApplicationDbContext context)
        {
            _model3DTemplateRepository = model3DTemplateRepository;
            _carRepository = carRepository;
            _context = context;
        }

        public Model3DTemplate Model3DTemplate { get; set; }
        public List<Car> RelatedCars { get; set; } = new List<Car>();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Model3DTemplate = await _model3DTemplateRepository.GetByIdAsync(id);

            if (Model3DTemplate == null)
            {
                return NotFound();
            }

            // Lấy danh sách xe đang sử dụng mô hình này
            RelatedCars = await _context.CarModel3Ds
                .Where(cm => cm.Model3DTemplateId == id)
                .Include(cm => cm.Car)
                .ThenInclude(c => c.Brand)
                .Include(cm => cm.Car)
                .ThenInclude(c => c.Category)
                .Include(cm => cm.Car)
                .ThenInclude(c => c.CarImages)
                .Select(cm => cm.Car)
                .ToListAsync();

            return Page();
        }
    }
}
