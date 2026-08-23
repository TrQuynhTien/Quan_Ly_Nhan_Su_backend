using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyNhanSu.API.Models
{
    [Table("NHANVIEN_PHUCAP")]
    public class NhanVienPhuCap
    {
        public int MaNV { get; set; }

        public int MaPC { get; set; }

        public DateTime NgayApDung { get; set; }

        public string? TrangThai { get; set; }
    }
}