using System.ComponentModel.DataAnnotations;

namespace GotoCarRental.Models
{
    public class Feature
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên tính năng không được để trống")]
        [MaxLength(100, ErrorMessage = "Tên tính năng không được vượt quá 100 ký tự")]
        public string Name { get; set; }

        [MaxLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string Description { get; set; }

        public virtual ICollection<CarFeature> CarFeatures { get; set; } = new List<CarFeature>();
    }
}

