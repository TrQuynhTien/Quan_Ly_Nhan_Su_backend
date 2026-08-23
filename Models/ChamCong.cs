using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyNhanSu.API.Models
{
    [Table("CHAMCONG")]
    public class ChamCong
    {
        [Key]
        public int MaCC { get; set; }

        public int MaNV { get; set; }

        public int MaCa { get; set; }

        public DateTime NgayChamCong { get; set; }

        public TimeSpan? GioVao { get; set; }

        public TimeSpan? GioRa { get; set; }

        public decimal? SoGioLam { get; set; }

        [StringLength(50)]
        public string? TrangThai { get; set; }

        [StringLength(255)]
        public string? GhiChu { get; set; }
    }
}