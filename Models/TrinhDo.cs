using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyNhanSu.API.Models
{
    [Table("TRINHDO")]
    public class TrinhDo
    {
        [Key]
        public int MaTD { get; set; }

        [Required]
        [StringLength(100)]
        public string TenTD { get; set; } = string.Empty;
    }
}