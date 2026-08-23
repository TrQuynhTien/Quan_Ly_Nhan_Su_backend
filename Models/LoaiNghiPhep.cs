using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyNhanSu.API.Models
{
    [Table("LOAINGHIPHEP")]
    public class LoaiNghiPhep
    {
        [Key]
        public int MaLoaiNP { get; set; }

        [Required]
        [StringLength(100)]
        public string TenLoaiNP { get; set; } = string.Empty;

        [StringLength(255)]
        public string? MoTa { get; set; }
    }
}