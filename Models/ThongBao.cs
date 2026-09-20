using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyNhanSu.API.Models
{
    [Table("THONGBAO")]
    public class ThongBao
    {
        [Key]
        public int MaThongBao { get; set; }

        public int MaNV { get; set; }

        [Required]
        [StringLength(200)]
        public string TieuDe { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string NoiDung { get; set; } = string.Empty;

        public bool DaDoc { get; set; } = false;

        public DateTime NgayTao { get; set; } = DateTime.Now;

        [StringLength(255)]
        public string? DuongDan { get; set; }
    }
}