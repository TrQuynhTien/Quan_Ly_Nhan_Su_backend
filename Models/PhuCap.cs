using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyNhanSu.API.Models
{
    [Table("PHUCAP")]
    public class PhuCap
    {
        [Key]
        public int MaPC { get; set; }

        [Required]
        [StringLength(100)]
        public string TenPC { get; set; } = string.Empty;

        public decimal SoTien { get; set; }

        [StringLength(255)]
        public string? MoTa { get; set; }
    }
}