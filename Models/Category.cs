using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GotoCarRental.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên loại xe không được để trống")]
        [MaxLength(100, ErrorMessage = "Tên loại xe không được vượt quá 100 ký tự")]
        public string Name { get; set; }

        [NotMapped] // Không map thuộc tính này vào database, chỉ dùng để navigation
        public virtual ICollection<Car>? Cars { get; set; }
    }

}
