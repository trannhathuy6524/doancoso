using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GotoCarRental.Models
{
    // Sửa đổi các trường trong Car.cs
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

        public bool IsAvailable { get; set; } = true;

        [Required(ErrorMessage = "Vui lòng chọn hãng xe")]
        public int BrandId { get; set; }

        [ForeignKey("BrandId")]
        public Brand Brand { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại xe")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category Category { get; set; }

        // Không cần đánh dấu [Required] cho UserId vì nó sẽ được gán trong controller
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        public bool IsApproved { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Các collection và reference khác
        public virtual ICollection<CarImage> CarImages { get; set; } = new List<CarImage>();
        public virtual CarModel3D CarModel3D { get; set; }
        public virtual ICollection<CarFeature> CarFeatures { get; set; } = new List<CarFeature>();
        public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public int? ProvinceId { get; set; }

        [ForeignKey("ProvinceId")]
        public Province Province { get; set; }

        [MaxLength(255)]
        public string DetailedAddress { get; set; }

        
        [Range(1, double.MaxValue, ErrorMessage = "Giá thuê theo giờ phải lớn hơn 0")]
        public decimal PricePerHour { get; set; }

        // Thêm tọa độ để lưu vị trí của xe
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // Thuộc tính này không lưu vào database, chỉ dùng để hiển thị khoảng cách
        [NotMapped]
        public double? DistanceFromUser { get; set; }


    }

}
