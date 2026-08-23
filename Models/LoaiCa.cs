using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyNhanSu.API.Models
{
    [Table("LOAICA")]
    public class LoaiCa
    {
        [Key]
        public int MaCa { get; set; }

        [Required]
        [StringLength(100)]
        public string TenCa { get; set; } = string.Empty;

        public TimeSpan GioBatDau { get; set; }

        public TimeSpan GioKetThuc { get; set; }

        public decimal SoGioQuyDinh { get; set; }
    }
}