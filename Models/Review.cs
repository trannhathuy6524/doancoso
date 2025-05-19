using System.ComponentModel.DataAnnotations;

namespace GotoCarRental.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int CarId { get; set; }
        public Car Car { get; set; }

        public int Rating { get; set; }

        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
