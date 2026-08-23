namespace QuanLyNhanSu.API.DTOs
{
    public class TaiKhoanResponse
    {
        public int MaTK { get; set; }
        public string TenDangNhap { get; set; } = string.Empty;
        public int MaNV { get; set; }
        public int MaQuyen { get; set; }
        public string? TrangThai { get; set; }
    }
}