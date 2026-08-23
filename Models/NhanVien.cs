using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyNhanSu.API.Models
{
    [Table("NHANVIEN")]
    public class NhanVien
    {
        [Key]
        public int MaNV { get; set; }

        [Required]
        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string GioiTinh { get; set; } = string.Empty;

        [Required]
        public DateTime NgaySinh { get; set; }

        [StringLength(20)]
        public string? CCCD { get; set; }

        [Required]
        [StringLength(255)]
        public string DiaChi { get; set; } = string.Empty;

        [StringLength(15)]
        public string? SDT { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [Required]
        public DateTime NgayVaoLam { get; set; }

        [StringLength(255)]
        public string? HinhAnh { get; set; }

        public int? MaPB { get; set; }

        public int? MaCV { get; set; }

        public int? MaTD { get; set; }

        [StringLength(50)]
        public string TrangThai { get; set; } = "Đang làm việc";
    }
}