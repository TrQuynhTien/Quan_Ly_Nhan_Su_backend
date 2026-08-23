using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyNhanSu.API.Models
{
    [Table("QUYEN")]
    public class Quyen
    {
        [Key]
        public int MaQuyen { get; set; }

        [Required]
        [StringLength(100)]
        public string TenQuyen { get; set; } = string.Empty;

        [StringLength(255)]
        public string? MoTa { get; set; }
    }
}