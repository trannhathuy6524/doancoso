using GotoCarRental.Data;
using GotoCarRental.Models;
using Microsoft.EntityFrameworkCore;

namespace GotoCarRental.Repository
{
    public class EFDriverScheduleRepository : IDriverScheduleRepository
    {
        private readonly ApplicationDbContext _context;

        public EFDriverScheduleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ScheduleAvailability>> GetDriverAvailabilityAsync(string driverId)
        {
            return await _context.ScheduleAvailabilities
                .Where(a => a.DriverId == driverId)
                .ToListAsync();
        }

        public async Task AddAvailabilityAsync(ScheduleAvailability availability)
        {
            _context.ScheduleAvailabilities.Add(availability);
            await _context.SaveChangesAsync();
        }
    }
}
