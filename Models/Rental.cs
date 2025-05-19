using System.ComponentModel.DataAnnotations;

namespace GotoCarRental.Models
{
    public class Rental
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        public int CarId { get; set; }
        public Car Car { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public decimal TotalPrice { get; set; }

        public int Status { get; set; } = 0; // 0: Chờ duyệt, 1: Đã xác nhận, 2: Đã hủy, 3: Hoàn thành

        [Required]
        [MaxLength(50)]
        public string PaymentStatus { get; set; } = "Pending";

        [MaxLength(50)]
        public string PaymentMethod { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Payment> Payments { get; set; }
    }
}