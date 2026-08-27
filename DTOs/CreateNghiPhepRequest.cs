namespace QuanLyNhanSu.API.DTOs
{
    public class CreateNghiPhepRequest
    {
        public int MaLoaiNP { get; set; }
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }
        public string? LyDo { get; set; }
    }
}