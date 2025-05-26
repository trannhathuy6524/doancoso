namespace GotoCarRental.Models
{
    public class ScheduleAvailability
    {
        public int Id { get; set; }
        public string DriverId { get; set; }
        public DateTime AvailableFrom { get; set; }
        public DateTime AvailableTo { get; set; }
    }
}
