using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.API.Data;
using QuanLyNhanSu.API.Models;

namespace QuanLyNhanSu.API.Services
{
    public class BangLuongService
    {
        private readonly QuanLyNhanSuDbContext _context;

        public BangLuongService(QuanLyNhanSuDbContext context)
        {
            
            _context = context;
        }

        public async Task<(string Status, BangLuong? Data)> CalculatePayrollAsync(
            int maNV,
            int thang,
            int nam)
        {
            if (thang < 1 || thang > 12)
            {
                return ("INVALID_MONTH", null);
            }

            if (nam < 2000)
            {
                return ("INVALID_YEAR", null);
            }

            var ngayDauThang = new DateTime(nam, thang, 1);
            var ngayCuoiThang = ngayDauThang.AddMonths(1).AddDays(-1);

            var hopDong = await _context.HopDongs
                .Where(x => x.MaNV == maNV)
                .Where(x =>
                    x.NgayBatDau <= ngayCuoiThang &&
                    (x.NgayKetThuc == null || x.NgayKetThuc >= ngayDauThang))
                .OrderByDescending(x => x.NgayBatDau)
                .FirstOrDefaultAsync();

            if (hopDong == null)
            {
                return ("NO_CONTRACT", null);
            }

            var tongGioCong = await _context.ChamCongs
                .Where(x =>
                    x.MaNV == maNV &&
                    x.NgayChamCong >= ngayDauThang &&
                    x.NgayChamCong <= ngayCuoiThang)
                .SumAsync(x => x.SoGioLam ?? 0);

            var soNgayCong = tongGioCong / 8m;

            var tongPhuCap = await _context.NhanVienPhuCaps
                .Where(x =>
                    x.MaNV == maNV &&
                    x.TrangThai == "Đang áp dụng")
                .Join(
                    _context.PhuCaps,
                    nvpc => nvpc.MaPC,
                    pc => pc.MaPC,
                    (nvpc, pc) => pc.SoTien
                )
                .SumAsync();

            var tongThuong = await _context.KhenThuongKyLuats
                .Where(x =>
                    x.MaNV == maNV &&
                    x.Loai == "Khen thưởng" &&
                    x.NgayQuyetDinh >= ngayDauThang &&
                    x.NgayQuyetDinh <= ngayCuoiThang)
                .SumAsync(x => x.SoTien ?? 0);

            var tongKhauTru = await _context.KhenThuongKyLuats
                .Where(x =>
                    x.MaNV == maNV &&
                    x.Loai == "Kỷ luật" &&
                    x.NgayQuyetDinh >= ngayDauThang &&
                    x.NgayQuyetDinh <= ngayCuoiThang)
                .SumAsync(x => x.SoTien ?? 0);

            var luongTheoCong =
                hopDong.LuongCoBan / 26 * soNgayCong;

            var tongLuong = Math.Round(
                luongTheoCong
                + tongPhuCap
                + tongThuong
                - tongKhauTru
            );

            var bangLuongDaTonTai = await _context.BangLuongs
                .FirstOrDefaultAsync(x =>
                    x.MaNV == maNV &&
                    x.Thang == thang &&
                    x.Nam == nam);

            if (bangLuongDaTonTai != null)
            {
                bangLuongDaTonTai.LuongCoBan = hopDong.LuongCoBan;
                bangLuongDaTonTai.TongPhuCap = tongPhuCap;
                bangLuongDaTonTai.TongThuong = tongThuong;
                bangLuongDaTonTai.TongKhauTru = tongKhauTru;
                bangLuongDaTonTai.SoNgayCong = soNgayCong;
                bangLuongDaTonTai.TongLuong = tongLuong;

                await _context.SaveChangesAsync();

                return ("SUCCESS", bangLuongDaTonTai);
            }

            var bangLuong = new BangLuong
            {
                MaNV = maNV,
                Thang = thang,
                Nam = nam,
                LuongCoBan = hopDong.LuongCoBan,
                TongPhuCap = tongPhuCap,
                TongThuong = tongThuong,
                TongKhauTru = tongKhauTru,
                SoNgayCong = soNgayCong,
                TongLuong = tongLuong
            };

            _context.BangLuongs.Add(bangLuong);
            await _context.SaveChangesAsync();

            return ("SUCCESS", bangLuong);
        }
    }
}