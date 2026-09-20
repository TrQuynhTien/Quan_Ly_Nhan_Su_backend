namespace QuanLyNhanSu.API.DTOs
{
    public class QuyenChucNangDto
    {
        public string ChucNang { get; set; } = string.Empty;
        public bool Xem { get; set; }
        public bool Them { get; set; }
        public bool Sua { get; set; }
        public bool Xoa { get; set; }
        public bool Duyet { get; set; }
    }

    public class PhanQuyenResponse
    {
        public int MaQuyen { get; set; }
        public string TenQuyen { get; set; } = string.Empty;
        public List<QuyenChucNangDto> QuyenHan { get; set; } = new();
    }
}