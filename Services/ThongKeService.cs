using QuanLyNhanSu.API.Data;
using Microsoft.EntityFrameworkCore;

namespace QuanLyNhanSu.API.Services
{
    public class ThongKeService
    {
        private readonly QuanLyNhanSuDbContext _context;

        public ThongKeService(QuanLyNhanSuDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetOverviewAsync()
        {
            var tongNhanVien = await _context.NhanViens.CountAsync();

            var nhanVienDangLam = await _context.NhanViens
                .CountAsync(x => x.TrangThai == "Đang làm việc");

            var tongPhongBan = await _context.PhongBans.CountAsync();

            var donNghiPhepChoDuyet = await _context.NghiPheps
                .CountAsync(x => x.TrangThai == "Chờ duyệt");

            return new
            {
                TongNhanVien = tongNhanVien,
                NhanVienDangLam = nhanVienDangLam,
                TongPhongBan = tongPhongBan,
                DonNghiPhepChoDuyet = donNghiPhepChoDuyet
            };
        }
        public async Task<object> GetOverviewByManagerAsync(int maNV)
        {
            var homNay = DateTime.Today;

            var truongPhong = await _context.NhanViens
                .FirstOrDefaultAsync(x => x.MaNV == maNV);

            if (truongPhong == null || truongPhong.MaPB == null)
            {
                return new
                {
                    TongNhanVien = 0,
                    NhanVienDangLam = 0,
                    NghiPhepHomNay = 0,
                    DiMuonHomNay = 0,
                    DonNghiPhepChoDuyet = 0
                };
            }

            int maPB = truongPhong.MaPB.Value;

            var nhanVienTrongPhong = _context.NhanViens
                .Where(x => x.MaPB == maPB);

            var tongNhanVien = await nhanVienTrongPhong.CountAsync();

            var nhanVienDangLam = await nhanVienTrongPhong
                .CountAsync(x => x.TrangThai == "Đang làm việc");

            var nghiPhepHomNay = await _context.NghiPheps
                .Join(
                    _context.NhanViens,
                    np => np.MaNV,
                    nv => nv.MaNV,
                    (np, nv) => new
                    {
                        NghiPhep = np,
                        NhanVien = nv
                    }
                )
                .CountAsync(x =>
                    x.NhanVien.MaPB == maPB &&
                    x.NghiPhep.TrangThai == "Đã duyệt" &&
                    x.NghiPhep.TuNgay.Date <= homNay &&
                    x.NghiPhep.DenNgay.Date >= homNay);

            var diMuonHomNay = await _context.ChamCongs
                .Join(
                    _context.NhanViens,
                    cc => cc.MaNV,
                    nv => nv.MaNV,
                    (cc, nv) => new
                    {
                        ChamCong = cc,
                        NhanVien = nv
                    }
                )
                .CountAsync(x =>
                    x.NhanVien.MaPB == maPB &&
                    x.ChamCong.NgayChamCong.Date == homNay &&
                    x.ChamCong.TrangThai == "Đi trễ");

            var donNghiPhepChoDuyet = await _context.NghiPheps
                .Join(
                    _context.NhanViens,
                    np => np.MaNV,
                    nv => nv.MaNV,
                    (np, nv) => new
                    {
                        NghiPhep = np,
                        NhanVien = nv
                    }
                )
                .CountAsync(x =>
                    x.NhanVien.MaPB == maPB &&
                    x.NghiPhep.TrangThai == "Chờ duyệt");

            return new
            {
                TongNhanVien = tongNhanVien,
                NhanVienDangLam = nhanVienDangLam,
                NghiPhepHomNay = nghiPhepHomNay,
                DiMuonHomNay = diMuonHomNay,
                DonNghiPhepChoDuyet = donNghiPhepChoDuyet
            };
        }

        public async Task<object> GetPayrollByMonthAsync(int thang, int nam)
        {
            var tongQuyLuong = await _context.BangLuongs
                .Where(x => x.Thang == thang && x.Nam == nam)
                .SumAsync(x => x.TongLuong);

            var soNhanVienCoLuong = await _context.BangLuongs
                .CountAsync(x => x.Thang == thang && x.Nam == nam);

            return new
            {
                Thang = thang,
                Nam = nam,
                SoNhanVienCoLuong = soNhanVienCoLuong,
                TongQuyLuong = Math.Round(tongQuyLuong)
            };
        }

        public async Task<object> GetAttendanceByMonthAsync(int thang, int nam)
        {
            var chamCongs = _context.ChamCongs
                .Where(x =>
                    x.NgayChamCong.Month == thang &&
                    x.NgayChamCong.Year == nam);

            var tongLuotChamCong = await chamCongs.CountAsync();

            var duCong = await chamCongs
                .CountAsync(x => x.TrangThai == "Đủ công");

            var diTre = await chamCongs
                .CountAsync(x => x.TrangThai == "Đi trễ");

            var veSom = await chamCongs
                .CountAsync(x => x.TrangThai == "Về sớm");

            return new
            {
                Thang = thang,
                Nam = nam,
                TongLuotChamCong = tongLuotChamCong,
                DuCong = duCong,
                DiTre = diTre,
                VeSom = veSom
            };
        }
        public async Task<object> GetLeaveByMonthAsync(int thang, int nam)
        {
            var nghiPheps = _context.NghiPheps
                .Where(x =>
                    x.TuNgay.Month == thang &&
                    x.TuNgay.Year == nam);

            var tongDon = await nghiPheps.CountAsync();

            var choDuyet = await nghiPheps
                .CountAsync(x => x.TrangThai == "Chờ duyệt");

            var daDuyet = await nghiPheps
                .CountAsync(x => x.TrangThai == "Đã duyệt");

            var tuChoi = await nghiPheps
                .CountAsync(x => x.TrangThai == "Từ chối");

            return new
            {
                Thang = thang,
                Nam = nam,
                TongDon = tongDon,
                ChoDuyet = choDuyet,
                DaDuyet = daDuyet,
                TuChoi = tuChoi
            };
        }
        public async Task<object> GetEmployeesByDepartmentAsync()
        {
            var data = await _context.NhanViens
                .Join(
                    _context.PhongBans,
                    nv => nv.MaPB,
                    pb => pb.MaPB,
                    (nv, pb) => new
                    {
                        nv.MaPB,
                        pb.TenPB
                    })
                .GroupBy(x => new
                {
                    x.MaPB,
                    x.TenPB
                })
                .Select(g => new
                {
                    MaPB = g.Key.MaPB,
                    TenPB = g.Key.TenPB,
                    SoNhanVien = g.Count()
                })
                .ToListAsync();

            return data;
        }
        public async Task<object> GetEmployeesByPositionAsync()
        {
            var data = await _context.NhanViens
                .Join(
                    _context.ChucVus,
                    nv => nv.MaCV,
                    cv => cv.MaCV,
                    (nv, cv) => new
                    {
                        nv.MaCV,
                        cv.TenCV
                    })
                .GroupBy(x => new
                {
                    x.MaCV,
                    x.TenCV
                })
                .Select(g => new
                {
                    MaCV = g.Key.MaCV,
                    TenCV = g.Key.TenCV,
                    SoNhanVien = g.Count()
                })
                .ToListAsync();

            return data;
        }
        public async Task<object> GetEmployeesByStatusAsync()
        {
            var data = await _context.NhanViens
                .GroupBy(x => x.TrangThai)
                .Select(g => new
                {
                    TrangThai = g.Key,
                    SoNhanVien = g.Count()
                })
                .ToListAsync();

            return data;
        }
        public async Task<object> GetExpiringContractsAsync()
        {
            var homNay = DateTime.Today;
            var sau30Ngay = homNay.AddDays(30);

            var data = await _context.HopDongs
                .Where(x =>
                    x.NgayKetThuc != null &&
                    x.NgayKetThuc >= homNay &&
                    x.NgayKetThuc <= sau30Ngay)
                .Select(x => new
                {
                    x.MaHD,
                    x.MaNV,
                    x.NgayBatDau,
                    x.NgayKetThuc,
                    SoNgayConLai = EF.Functions.DateDiffDay(
                        homNay,
                        x.NgayKetThuc!.Value)
                })
                .OrderBy(x => x.NgayKetThuc)
                .ToListAsync();

            return data;
        }
    }
}