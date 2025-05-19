using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GotoCarRental.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string UserType { get; set; } = "Customer"; // Giá trị mặc định
        public bool IsApproved { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Car> Cars { get; set; }
        public ICollection<Rental> Rentals { get; set; }
        public ICollection<Review> Reviews { get; set; }
    }

}
