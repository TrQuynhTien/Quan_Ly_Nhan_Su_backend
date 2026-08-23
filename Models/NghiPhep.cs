using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyNhanSu.API.Models
{
    [Table("NGHIPHEP")]
    public class NghiPhep
    {
        [Key]
        public int MaNP { get; set; }

        public int MaNV { get; set; }

        public int MaLoaiNP { get; set; }

        public DateTime TuNgay { get; set; }

        public DateTime DenNgay { get; set; }

        [StringLength(255)]
        public string? LyDo { get; set; }

        [StringLength(50)]
        public string? TrangThai { get; set; }

        public int? NguoiDuyet { get; set; }
    }
}