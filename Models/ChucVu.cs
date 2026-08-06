using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyNhanSu.API.Models
{
    [Table("CHUCVU")]
    public class ChucVu
    {
        [Key]
        public int MaCV { get; set; }

        [Required]
        [StringLength(100)]
        public string TenCV { get; set; } = string.Empty;

        [StringLength(255)]
        public string? MoTa { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? HeSoPhuCap { get; set; } = 0;
    }
}