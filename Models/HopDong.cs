using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyNhanSu.API.Models
{
    [Table("HOPDONG")]
    public class HopDong
    {
        [Key]
        public int MaHD { get; set; }

        public int MaNV { get; set; }

        public int MaLoaiHD { get; set; }

        public DateTime NgayBatDau { get; set; }

        public DateTime? NgayKetThuc { get; set; }

        public decimal LuongCoBan { get; set; }

        [StringLength(50)]
        public string? TrangThai { get; set; }
    }
}