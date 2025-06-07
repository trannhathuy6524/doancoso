using System.ComponentModel.DataAnnotations;

namespace GotoCarRental.Models
{
    public class Setting
    {
        [Key]
        [MaxLength(100)]
        public string Key { get; set; }

        [Required]
        public string Value { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }
    }
}