using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public int Days { get; set; }
        public int? Hours { get; set; } // Số giờ thuê khi thuê theo giờ
        [NotMapped] // Không tạo cột trong database
        public DateTime? RentalDate
        {
            get => Type == RentalType.ByHour ? StartDate : null;
        }
        // Thêm phương thức tiện ích này vào lớp Rental
        public decimal CalculateHourlyRate()
        {
            if (Car == null) return 0;

            // Tính giá theo giờ, đảm bảo luôn > 0
            return Car.PricePerHour > 0 ? Car.PricePerHour : Car.PricePerDay / 24;
        }
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

        public bool IsDeliveryRequested { get; set; } = false; // Người thuê có yêu cầu giao xe không
        public string? DeliveryAddress { get; set; } // Địa chỉ giao xe
        public double? DeliveryLatitude { get; set; } // Vĩ độ điểm giao xe
        public double? DeliveryLongitude { get; set; } // Kinh độ điểm giao xe
        public decimal DeliveryFee { get; set; } = 0; // Phí giao xe

        // Thêm thuộc tính mới cho địa điểm đón khách
        public string? PickupAddress { get; set; } // Địa chỉ đón khách
        public double? PickupLatitude { get; set; } // Vĩ độ điểm đón
        public double? PickupLongitude { get; set; } // Kinh độ điểm đón

        // Thêm enum mới cho loại hình di chuyển khi có tài xế
        public enum TripType
        {
            LocalTrip = 0,       // Nội thành
            InterCityRoundTrip = 1,  // Liên tỉnh
            InterCityOneWay = 2  // Liên tỉnh (1 chiều)
        }
        public enum RentalType
        {
            ByDay = 0,
            ByHour = 1
        }
        public enum ServiceType
        {
            SelfDrive = 0, // Tự lái
            WithDriver = 1 // Có tài xế
        }
        public enum RentalStatus
        {
            Pending = 0,    // Chờ xác nhận
            Confirmed = 1,  // Đã xác nhận
            Canceled = 2,   // Đã hủy
            Completed = 3,  // Đã hoàn thành
            InProgress = 4  // Đang thực hiện
        }
        public RentalType Type { get; set; } = RentalType.ByDay;
        public ServiceType Service { get; set; } = ServiceType.SelfDrive;
        // Thêm thuộc tính mới cho loại hình di chuyển
        public TripType? Trip { get; set; } = null;

        public decimal DriverFee { get; set; } = 0; // Phí tài xế nếu có
        public string? DriverName { get; set; } // Tên tài xế được chỉ định
        public string? DriverPhone { get; set; } // Số điện thoại tài xế


        // Thêm thuộc tính cho khoảng cách ước tính khi chọn liên tỉnh
        public decimal? EstimatedDistance { get; set; } // Khoảng cách ước tính (km)
        public decimal? DistanceFee { get; set; } // Phí theo khoảng cách

        public decimal GetTotalFee()
        {
            decimal baseFee = 0;
            
            if (Type == RentalType.ByDay)
            {
                // Calculate days between start and end dates
                int days = (int)(EndDate - StartDate).TotalDays;
                if (days < 1) days = 1;
                
                baseFee = days * Car.PricePerDay;
            }
            else // ByHour
            {
                // Calculate hours between start and end times
                int hours = (int)(EndDate - StartDate).TotalHours;
                if (hours < 1) hours = 1;
                
                baseFee = hours * Car.PricePerHour;
            }
            
            // Add driver fee if applicable
            if (Service == ServiceType.WithDriver)
            {
                baseFee += DriverFee;
            }
            // Add distance fee if applicable
            if (DistanceFee.HasValue)
            {
                baseFee += DistanceFee.Value;
            }

            // Add delivery fee if applicable
            if (IsDeliveryRequested)
            {
                baseFee += DeliveryFee;
            }
            return baseFee;
        }

        public ICollection<Payment> Payments { get; set; }

        // Thêm thuộc tính mới cho địa điểm trả khách
        public string? DropoffAddress { get; set; } // Địa chỉ trả khách
        public double? DropoffLatitude { get; set; } // Vĩ độ điểm trả
        public double? DropoffLongitude { get; set; } // Kinh độ điểm trả

        // Phí hoa hồng hệ thống (tính bằng %)
        [Range(0, 100)]
        public decimal CommissionRate { get; set; } = 10; // Mặc định 10%

        // Số tiền hoa hồng thực tế
        [Column(TypeName = "decimal(18,2)")]
        public decimal CommissionAmount { get; set; } = 0;

        // Số tiền thực tế thanh toán cho chủ xe sau khi trừ hoa hồng
        [Column(TypeName = "decimal(18,2)")]
        public decimal OwnerAmount { get; set; } = 0;

        // Tính toán phí hoa hồng và số tiền thanh toán cho chủ xe
        public void CalculateCommission()
        {
            // Tính số tiền hoa hồng
            CommissionAmount = Math.Round(TotalPrice * CommissionRate / 100, 0);

            // Tính số tiền còn lại cho chủ xe
            OwnerAmount = TotalPrice - CommissionAmount;
        }

    }
}