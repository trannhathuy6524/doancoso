using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GotoCarRental.Areas.Admin.Pages.Cars
{
    [Authorize(Roles = "Admin,Manager")]
    public class IndexModel : PageModel
    {
        private readonly ICarRepository _carRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly ICategoryRepository _categoryRepository;

        public IndexModel(
            ICarRepository carRepository,
            IBrandRepository brandRepository,
            ICategoryRepository categoryRepository)
        {
            _carRepository = carRepository;
            _brandRepository = brandRepository;
            _categoryRepository = categoryRepository;
        }

        public IEnumerable<Car> Cars { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MinPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MaxPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? BrandId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? IsAvailable { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? IsApproved { get; set; }

        public IEnumerable<Brand> Brands { get; set; }
        public IEnumerable<Category> Categories { get; set; }

        public async Task OnGetAsync(int pageNumber = 1)
        {
            PageNumber = pageNumber;
            var result = await _carRepository.GetCarsPagedAsync(
                PageNumber,
                PageSize,
                SearchTerm,
                MinPrice,
                MaxPrice,
                BrandId,
                CategoryId,
                IsAvailable);

            Cars = result.Cars;
            TotalCount = result.TotalCount;

            Brands = await _brandRepository.GetAllAsync();
            Categories = await _categoryRepository.GetAllAsync();
        }

        public async Task<IActionResult> OnPostApproveCarAsync(int id)
        {
            var car = await _carRepository.GetByIdAsync(id);
            if (car == null)
            {
                return NotFound();
            }

            await _carRepository.ApproveCarAsync(id);

            TempData["SuccessMessage"] = "Xe đã được phê duyệt thành công!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleAvailabilityAsync(int id)
        {
            var car = await _carRepository.GetByIdAsync(id);
            if (car == null)
            {
                return NotFound();
            }

            await _carRepository.ToggleAvailabilityAsync(id);

            TempData["SuccessMessage"] = $"Đã thay đổi trạng thái sẵn sàng của xe thành công!";
            return RedirectToPage();
        }
    }
}
