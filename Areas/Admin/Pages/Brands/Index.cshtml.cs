using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GotoCarRental.Areas.Admin.Pages.Brands
{
    [Authorize(Roles = "Admin,Manager")]
    public class IndexModel : PageModel
    {
        private readonly IBrandRepository _brandRepository;

        public IndexModel(IBrandRepository brandRepository)
        {
            _brandRepository = brandRepository;
        }

        public IEnumerable<Brand> Brands { get; set; }
        public Dictionary<int, int> BrandStats { get; set; }
        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        public async Task OnGetAsync()
        {
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                Brands = await _brandRepository.SearchAsync(SearchTerm);
            }
            else
            {
                Brands = await _brandRepository.GetAllAsync();
            }

            // Get car count for each brand
            BrandStats = new Dictionary<int, int>();
            foreach (var brand in Brands)
            {
                BrandStats[brand.Id] = await _brandRepository.GetCarCountAsync(brand.Id);
            }
        }
    }
}
