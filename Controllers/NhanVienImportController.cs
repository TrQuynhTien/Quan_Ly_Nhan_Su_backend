using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyNhanSu.API.Data;
using QuanLyNhanSu.API.Models;
using Microsoft.EntityFrameworkCore;

namespace QuanLyNhanSu.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Quản trị viên, Nhân viên nhân sự")]
    public class NhanVienImportController : ControllerBase
    {
        private readonly QuanLyNhanSuDbContext _context;

        public NhanVienImportController(QuanLyNhanSuDbContext context)
        {
            _context = context;
        }

        [HttpPost("preview")]
        public IActionResult PreviewExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File không hợp lệ.");

            var extension = Path.GetExtension(file.FileName).ToLower();

            if (extension != ".xlsx")
                return BadRequest("Chỉ hỗ trợ file .xlsx");

            var danhSach = new List<object>();

            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);

            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(1);
            if (!rows.Any())
                return BadRequest("File không có dữ liệu nhân viên.");

            int soDong = 2;

            foreach (var row in rows)
            {
                var hoTen = row.Cell(1).GetString().Trim();
                var gioiTinh = row.Cell(2).GetString().Trim();
                var ngaySinhText = row.Cell(3).GetString().Trim();
                var cccd = row.Cell(4).GetString().Trim();
                var diaChi = row.Cell(5).GetString().Trim();
                var sdt = row.Cell(6).GetString().Trim();
                var email = row.Cell(7).GetString().Trim();
                var ngayVaoLamText = row.Cell(8).GetString().Trim();
                var maPBText = row.Cell(9).GetString().Trim();
                var maCVText = row.Cell(10).GetString().Trim();
                var maTDText = row.Cell(11).GetString().Trim();
                var trangThai = row.Cell(12).GetString().Trim();

                var loi = new List<string>();

                if (string.IsNullOrWhiteSpace(hoTen))
                    loi.Add("Họ tên không được để trống");

                if (string.IsNullOrWhiteSpace(cccd))
                    loi.Add("CCCD không được để trống");

                if (string.IsNullOrWhiteSpace(email))
                    loi.Add("Email không được để trống");

                if (!DateTime.TryParse(ngaySinhText, out _))
                    loi.Add("Ngày sinh không hợp lệ");

                if (!DateTime.TryParse(ngayVaoLamText, out _))
                    loi.Add("Ngày vào làm không hợp lệ");

                if (!int.TryParse(maPBText, out var maPB))
                    loi.Add("Mã phòng ban không hợp lệ");

                if (!int.TryParse(maCVText, out var maCV))
                    loi.Add("Mã chức vụ không hợp lệ");

                if (!int.TryParse(maTDText, out var maTD))
                    loi.Add("Mã trình độ không hợp lệ");

                if (!string.IsNullOrWhiteSpace(cccd))
                {
                    if (_context.NhanViens.Any(x => x.CCCD == cccd))
                        loi.Add("CCCD đã tồn tại");
                }

                if (!string.IsNullOrWhiteSpace(email))
                {
                    if (_context.NhanViens.Any(x => x.Email == email))
                        loi.Add("Email đã tồn tại");
                }

                if (int.TryParse(maPBText, out maPB))
                {
                    if (!_context.PhongBans.Any(x => x.MaPB == maPB))
                        loi.Add("Phòng ban không tồn tại");
                }

                if (int.TryParse(maCVText, out maCV))
                {
                    if (!_context.ChucVus.Any(x => x.MaCV == maCV))
                        loi.Add("Chức vụ không tồn tại");
                }

                if (int.TryParse(maTDText, out maTD))
                {
                    if (!_context.TrinhDos.Any(x => x.MaTD == maTD))
                        loi.Add("Trình độ không tồn tại");
                }

                danhSach.Add(new
                {
                    Dong = soDong,
                    HoTen = hoTen,
                    GioiTinh = gioiTinh,
                    NgaySinh = ngaySinhText,
                    CCCD = cccd,
                    DiaChi = diaChi,
                    SDT = sdt,
                    Email = email,
                    NgayVaoLam = ngayVaoLamText,
                    MaPB = maPBText,
                    MaCV = maCVText,
                    MaTD = maTDText,
                    TrangThai = trangThai,
                    HopLe = loi.Count == 0,
                    Loi = loi
                });

                soDong++;
            }

            return Ok(danhSach);
        }
        [HttpPost("import")]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File không hợp lệ.");

            var extension = Path.GetExtension(file.FileName).ToLower();

            if (extension != ".xlsx")
                return BadRequest("Chỉ hỗ trợ file .xlsx");

            var danhSachNhanVien = new List<NhanVien>();
            var danhSachLoi = new List<object>();

            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);

            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(1);

            int soDong = 2;

            foreach (var row in rows)
            {
                var hoTen = row.Cell(1).GetString().Trim();
                var gioiTinh = row.Cell(2).GetString().Trim();
                var ngaySinhText = row.Cell(3).GetString().Trim();
                var cccd = row.Cell(4).GetString().Trim();
                var diaChi = row.Cell(5).GetString().Trim();
                var sdt = row.Cell(6).GetString().Trim();
                var email = row.Cell(7).GetString().Trim();
                var ngayVaoLamText = row.Cell(8).GetString().Trim();
                var maPBText = row.Cell(9).GetString().Trim();
                var maCVText = row.Cell(10).GetString().Trim();
                var maTDText = row.Cell(11).GetString().Trim();
                var trangThai = row.Cell(12).GetString().Trim();

                var loi = new List<string>();

                if (string.IsNullOrWhiteSpace(hoTen))
                    loi.Add("Họ tên không được để trống");

                if (string.IsNullOrWhiteSpace(cccd))
                    loi.Add("CCCD không được để trống");

                if (string.IsNullOrWhiteSpace(email))
                    loi.Add("Email không được để trống");

                if (!DateTime.TryParse(ngaySinhText, out var ngaySinh))
                    loi.Add("Ngày sinh không hợp lệ");

                if (!DateTime.TryParse(ngayVaoLamText, out var ngayVaoLam))
                    loi.Add("Ngày vào làm không hợp lệ");

                if (!int.TryParse(maPBText, out var maPB))
                    loi.Add("Mã phòng ban không hợp lệ");

                if (!int.TryParse(maCVText, out var maCV))
                    loi.Add("Mã chức vụ không hợp lệ");

                if (!int.TryParse(maTDText, out var maTD))
                    loi.Add("Mã trình độ không hợp lệ");

                if (!string.IsNullOrWhiteSpace(cccd) &&
                    await _context.NhanViens.AnyAsync(x => x.CCCD == cccd))
                    loi.Add("CCCD đã tồn tại");

                if (!string.IsNullOrWhiteSpace(email) &&
                    await _context.NhanViens.AnyAsync(x => x.Email == email))
                    loi.Add("Email đã tồn tại");

                if (int.TryParse(maPBText, out maPB) &&
                    !await _context.PhongBans.AnyAsync(x => x.MaPB == maPB))
                    loi.Add("Phòng ban không tồn tại");

                if (int.TryParse(maCVText, out maCV) &&
                    !await _context.ChucVus.AnyAsync(x => x.MaCV == maCV))
                    loi.Add("Chức vụ không tồn tại");

                if (int.TryParse(maTDText, out maTD) &&
                    !await _context.TrinhDos.AnyAsync(x => x.MaTD == maTD))
                    loi.Add("Trình độ không tồn tại");

                if (loi.Count > 0)
                {
                    danhSachLoi.Add(new
                    {
                        Dong = soDong,
                        Loi = loi
                    });

                    soDong++;
                    continue;
                }

                danhSachNhanVien.Add(new NhanVien
                {
                    HoTen = hoTen,
                    GioiTinh = gioiTinh,
                    NgaySinh = ngaySinh,
                    CCCD = cccd,
                    DiaChi = diaChi,
                    SDT = sdt,
                    Email = email,
                    NgayVaoLam = ngayVaoLam,
                    MaPB = maPB,
                    MaCV = maCV,
                    MaTD = maTD,
                    TrangThai = trangThai
                });

                soDong++;
            }

            if (danhSachLoi.Count > 0)
            {
                return BadRequest(new
                {
                    Message = "File có dữ liệu không hợp lệ. Không import.",
                    Loi = danhSachLoi
                });
            }

            await _context.NhanViens.AddRangeAsync(danhSachNhanVien);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Import nhân viên thành công.",
                SoLuong = danhSachNhanVien.Count
            });
        }
    }
}