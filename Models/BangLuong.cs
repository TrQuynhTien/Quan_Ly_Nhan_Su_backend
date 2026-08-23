using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyNhanSu.API.Models
{
    [Table("BANGLUONG")]
    public class BangLuong
    {
        [Key]
        public int MaLuong { get; set; }

        public int MaNV { get; set; }

        public int Thang { get; set; }

        public int Nam { get; set; }

        public decimal LuongCoBan { get; set; }

        public decimal TongPhuCap { get; set; }

        public decimal TongThuong { get; set; }

        public decimal TongKhauTru { get; set; }

        public decimal SoNgayCong { get; set; }

        public decimal TongLuong { get; set; }
    }
}