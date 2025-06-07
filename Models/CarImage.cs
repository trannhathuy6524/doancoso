using System.ComponentModel.DataAnnotations;

namespace GotoCarRental.Models
{
    public class CarImage
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(255)]
        public string ImageUrl { get; set; }

        public int CarId { get; set; }
        public Car Car { get; set; }

        // Thêm thuộc tính này
        public bool IsMainImage { get; set; } = false;
    }

}
