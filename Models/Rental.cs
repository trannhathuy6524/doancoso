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

        
        public enum RentalType
        {
            ByDay = 0,
            ByHour = 1
        }

        public RentalType Type { get; set; } = RentalType.ByDay;
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public int? Hours { get; set; } // Số giờ thuê khi thuê theo giờ

        public bool IsDeliveryRequested { get; set; } = false; // Người thuê có yêu cầu giao xe không
        public string? DeliveryAddress { get; set; } // Địa chỉ giao xe
        public double? DeliveryLatitude { get; set; } // Vĩ độ điểm giao xe
        public double? DeliveryLongitude { get; set; } // Kinh độ điểm giao xe
        public decimal DeliveryFee { get; set; } = 0; // Phí giao xe


    }
}