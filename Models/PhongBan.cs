using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace QuanLyNhanSu.API.Models
{
    [Table("PHONGBAN")]
    public class PhongBan
    {
        [Key]
        public int MaPB {get; set;}

        [Required]
        [StringLength(100)]
        public string TenPB {get; set;} = string.Empty;

        [StringLength(255)]
        public string? MoTa {get; set;}

        [StringLength(50)]
        public string? TrangThai {get; set;} = "Đang hoạt động";
    }
}