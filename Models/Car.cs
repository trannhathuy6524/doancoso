using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GotoCarRental.Models
{
    public class Car
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên xe không được để trống")]
        [MaxLength(100, ErrorMessage = "Tên xe không được vượt quá 100 ký tự")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Mô tả không được để trống")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Giá thuê không được để trống")]
        [Range(1, double.MaxValue, ErrorMessage = "Giá thuê phải lớn hơn 0")]
        public decimal PricePerDay { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "Giá thuê theo giờ phải lớn hơn 0")]
        public decimal PricePerHour { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hãng xe")]
        public int BrandId { get; set; }

        [ForeignKey("BrandId")]
        public Brand Brand { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại xe")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category Category { get; set; }

        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        public int? ProvinceId { get; set; }

        [ForeignKey("ProvinceId")]
        public Province Province { get; set; }

        [MaxLength(255)]
        public string DetailedAddress { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        [NotMapped]
        public double? DistanceFromUser { get; set; }

        public bool IsAvailable { get; set; } = true;
        public bool IsApproved { get; set; } = false;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool OfferDriverService { get; set; } = false;
        public decimal DriverFeePerDay { get; set; } = 0;
        public decimal DriverFeePerHour { get; set; } = 0;
        // Thêm phương thức này vào class Car
        public decimal GetHourlyPrice()
        {
            // Đảm bảo luôn có giá trị > 0 cho thuê theo giờ
            return PricePerHour > 0 ? PricePerHour : PricePerDay / 24;
        }

        public virtual ICollection<CarImage> CarImages { get; set; } = new List<CarImage>();
        public virtual CarModel3D CarModel3D { get; set; }
        public virtual ICollection<CarFeature> CarFeatures { get; set; } = new List<CarFeature>();
        public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}