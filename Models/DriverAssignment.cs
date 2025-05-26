using System;
using System.ComponentModel.DataAnnotations;

namespace GotoCarRental.Models
{
    public class DriverAssignment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RentalId { get; set; }

        [Required]
        public string DriverId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } // Pending, Confirmed, Rejected

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Rental Rental { get; set; }
        public ApplicationUser Driver { get; set; }
    }
}