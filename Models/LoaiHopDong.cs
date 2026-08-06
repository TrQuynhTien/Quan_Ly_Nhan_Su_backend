using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyNhanSu.API.Models
{
    [Table("LOAIHOPDONG")]
    public class LoaiHopDong
    {
        [Key]
        public int MaLoaiHD { get; set; }

        [Required]
        [StringLength(100)]
        public string TenLoaiHD { get; set; } = string.Empty;

        [StringLength(255)]
        public string? MoTa { get; set; }
    }
}