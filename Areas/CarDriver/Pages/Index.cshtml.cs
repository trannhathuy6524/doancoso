using GotoCarRental.Models;
using GotoCarRental.Repository;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GotoCarRental.Areas.CarDriver.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IDriverScheduleRepository _repo;

        public IndexModel(IDriverScheduleRepository repo)
        {
            _repo = repo;
        }

        public List<ScheduleAvailability> Availabilities { get; set; }

        public async Task OnGetAsync()
        {
            var driverId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Availabilities = await _repo.GetDriverAvailabilityAsync(driverId);
        }
    }

}
