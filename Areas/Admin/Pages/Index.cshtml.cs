using GotoCarRental.Models;
using GotoCarRental.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GotoCarRental.Areas.Admin.Pages
{
    [Authorize(Roles = "Admin,Manager")]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICarRepository _carRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly ICategoryRepository _categoryRepository;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            ICarRepository carRepository,
            IBrandRepository brandRepository,
            ICategoryRepository categoryRepository)
        {
            _userManager = userManager;
            _carRepository = carRepository;
            _brandRepository = brandRepository;
            _categoryRepository = categoryRepository;
        }

        public int TotalUsers { get; set; }
        public int TotalCars { get; set; }
        public int TotalBrands { get; set; }
        public int TotalCategories { get; set; }
        public int PendingApprovalCars { get; set; }
        public int PendingApprovalUsers { get; set; }

        public async Task OnGetAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            TotalUsers = users.Count;
            PendingApprovalUsers = users.Count(u => !u.IsApproved);

            var cars = await _carRepository.GetAllAsync();
            TotalCars = cars.Count();
            PendingApprovalCars = cars.Count(c => !c.IsApproved);

            var brands = await _brandRepository.GetAllAsync();
            TotalBrands = brands.Count();

            var categories = await _categoryRepository.GetAllAsync();
            TotalCategories = categories.Count();
        }
    }
}
