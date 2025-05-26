using GotoCarRental.Models;

namespace GotoCarRental.Repository
{
    public interface IDriverScheduleRepository
    {
        Task<List<ScheduleAvailability>> GetDriverAvailabilityAsync(string driverId);
        Task AddAvailabilityAsync(ScheduleAvailability availability);
    }
}
