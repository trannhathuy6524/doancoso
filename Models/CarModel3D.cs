using System.ComponentModel.DataAnnotations;

namespace GotoCarRental.Models
{
    public class CarModel3D
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(255)]
        public string FileUrl { get; set; }

        [MaxLength(255)]
        public string PreviewImageUrl { get; set; }

        [Required]
        public int CarId { get; set; }

        public Car Car { get; set; }
        public int Model3DTemplateId { get; set; }
        public Model3DTemplate Model3DTemplate { get; set; }

    }
}