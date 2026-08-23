using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyNhanSu.API.Models
{
    [Table("TAIKHOAN")]
    public class TaiKhoan
    {
        [Key]
        public int MaTK { get; set; }

        [Required]
        [StringLength(100)]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string MatKhau { get; set; } = string.Empty;

        public int MaNV { get; set; }

        public int MaQuyen { get; set; }

        [StringLength(50)]
        public string? TrangThai { get; set; }
    }
}