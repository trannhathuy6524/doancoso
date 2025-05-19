using System.ComponentModel.DataAnnotations;

namespace GotoCarRental.Models
{
    public class Province
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên tỉnh/thành phố không được để trống")]
        [MaxLength(100, ErrorMessage = "Tên tỉnh/thành phố không được vượt quá 100 ký tự")]
        public string Name { get; set; }

        [MaxLength(10)]
        public string Code { get; set; }

        public virtual ICollection<Car> Cars { get; set; } = new List<Car>();
    }
}
