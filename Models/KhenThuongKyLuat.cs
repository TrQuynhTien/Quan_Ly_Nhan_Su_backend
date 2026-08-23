using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyNhanSu.API.Models
{
    [Table("KHENTHUONGKYLUAT")]
    public class KhenThuongKyLuat
    {
        [Key]
        public int MaKTKL { get; set; }

        public int MaNV { get; set; }

        [Required]
        [StringLength(50)]
        public string Loai { get; set; } = string.Empty;

        [StringLength(255)]
        public string? LyDo { get; set; }

        public decimal? SoTien { get; set; }

        public DateTime NgayQuyetDinh { get; set; }
    }
}