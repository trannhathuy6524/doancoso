using System.ComponentModel.DataAnnotations;

namespace GotoCarRental.Models
{
    public class Model3DTemplate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public int BrandId { get; set; }
        public Brand Brand { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileUrl { get; set; }

        [Required]
        [MaxLength(255)]
        public string PreviewImageUrl { get; set; }

        [Required]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
