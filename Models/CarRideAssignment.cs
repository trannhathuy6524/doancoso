namespace GotoCarRental.Models
{
    public class CarRideAssignment
    {
        public int Id { get; set; }
        public int RideId { get; set; }
        public string DriverId { get; set; }
        public DateTime AssignedTime { get; set; }

        public Car Ride { get; set; }
        public ApplicationUser Driver { get; set; }
    }
}
