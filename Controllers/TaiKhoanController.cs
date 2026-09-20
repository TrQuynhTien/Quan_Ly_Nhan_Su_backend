using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.API.Data;
using QuanLyNhanSu.API.Models;
using QuanLyNhanSu.API.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace QuanLyNhanSu.API.Controllers
{
    [Route("api/tai-khoans")]
    [ApiController]
    public class TaiKhoanController : ControllerBase
    {
        private readonly QuanLyNhanSuDbContext _context;

        public TaiKhoanController(QuanLyNhanSuDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Quản trị viên")]    
        public async Task<ActionResult<IEnumerable<TaiKhoanResponse>>> GetTaiKhoans()
        {
            var taiKhoans = await _context.TaiKhoans
                .Select(x => new TaiKhoanResponse
                {
                    MaTK = x.MaTK,
                    TenDangNhap = x.TenDangNhap,
                    MaNV = x.MaNV,
                    MaQuyen = x.MaQuyen,
                    TrangThai = x.TrangThai
                })
                .ToListAsync();

            return Ok(taiKhoans);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Quản trị viên")]
        public async Task<ActionResult<TaiKhoanResponse>> GetById(int id)
        {
            var taiKhoan = await _context.TaiKhoans
                .Where(x => x.MaTK == id)
                .Select(x => new TaiKhoanResponse
                {
                    MaTK = x.MaTK,
                    TenDangNhap = x.TenDangNhap,
                    MaNV = x.MaNV,
                    MaQuyen = x.MaQuyen,
                    TrangThai = x.TrangThai
                })
                .FirstOrDefaultAsync();

            if (taiKhoan == null)
            {
                return NotFound();
            }

            return Ok(taiKhoan);
        }

        [HttpPost]
        [Authorize(Roles = "Quản trị viên")]
        public async Task<ActionResult<TaiKhoanResponse>> Create(TaiKhoan taiKhoan)
        {
            var tenDangNhapDaTonTai = await _context.TaiKhoans
                .AnyAsync(x => x.TenDangNhap == taiKhoan.TenDangNhap);

            if (tenDangNhapDaTonTai)
            {
                return Conflict(new
                {
                    message = "Tên đăng nhập đã tồn tại."
                });
            }
            var nhanVienTonTai = await _context.NhanViens
                .AnyAsync(x => x.MaNV == taiKhoan.MaNV);

            if (!nhanVienTonTai)
            {
                return BadRequest(new
                {
                    message = "Nhân viên không tồn tại."
                });
            }

            var quyenTonTai = await _context.Quyens
                .AnyAsync(x => x.MaQuyen == taiKhoan.MaQuyen);

            if (!quyenTonTai)
            {
                return BadRequest(new
                {
                    message = "Quyền không tồn tại."
                });
            }
            if (string.IsNullOrWhiteSpace(taiKhoan.MatKhau))
            {
                return BadRequest(new
                {
                    message = "Mật khẩu không được để trống."
                });
            }
            taiKhoan.MatKhau =
                BCrypt.Net.BCrypt.HashPassword(taiKhoan.MatKhau);

            _context.TaiKhoans.Add(taiKhoan);
            await _context.SaveChangesAsync();

            var response = new TaiKhoanResponse
            {
                MaTK = taiKhoan.MaTK,
                TenDangNhap = taiKhoan.TenDangNhap,
                MaNV = taiKhoan.MaNV,
                MaQuyen = taiKhoan.MaQuyen,
                TrangThai = taiKhoan.TrangThai
            };

            return CreatedAtAction(
                nameof(GetById),
                new { id = taiKhoan.MaTK },
                response
            );
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Quản trị viên")]
        public async Task<IActionResult> Update(int id, TaiKhoan taiKhoan)
        {
            // 1. Tìm tài khoản cần cập nhật theo id trên URL
            var taiKhoanCu = await _context.TaiKhoans
                .FirstOrDefaultAsync(x => x.MaTK == id);

            if (taiKhoanCu == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy tài khoản."
                });
            }

            // 2. Kiểm tra tên đăng nhập có bị trùng với tài khoản khác không
            var tenDangNhapDaTonTai = await _context.TaiKhoans
                .AnyAsync(x =>
                    x.TenDangNhap == taiKhoan.TenDangNhap &&
                    x.MaTK != id);

            if (tenDangNhapDaTonTai)
            {
                return Conflict(new
                {
                    message = "Tên đăng nhập đã tồn tại."
                });
            }

            // 3. Kiểm tra nhân viên
            var nhanVienTonTai = await _context.NhanViens
                .AnyAsync(x => x.MaNV == taiKhoan.MaNV);

            if (!nhanVienTonTai)
            {
                return BadRequest(new
                {
                    message = "Nhân viên không tồn tại."
                });
            }

            // 4. Kiểm tra quyền
            var quyenTonTai = await _context.Quyens
                .AnyAsync(x => x.MaQuyen == taiKhoan.MaQuyen);

            if (!quyenTonTai)
            {
                return BadRequest(new
                {
                    message = "Quyền không tồn tại."
                });
            }

            // 5. Cập nhật thông tin
            taiKhoanCu.TenDangNhap = taiKhoan.TenDangNhap;
            taiKhoanCu.MaNV = taiKhoan.MaNV;
            taiKhoanCu.MaQuyen = taiKhoan.MaQuyen;
            taiKhoanCu.TrangThai = taiKhoan.TrangThai;

            // Chỉ đổi mật khẩu khi FE gửi mật khẩu mới
            if (!string.IsNullOrWhiteSpace(taiKhoan.MatKhau))
            {
                taiKhoanCu.MatKhau =
                    BCrypt.Net.BCrypt.HashPassword(taiKhoan.MatKhau);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Cập nhật tài khoản thành công."
            });
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Quản trị viên")]
        public async Task<IActionResult> Delete(int id)
        {
            var taiKhoan = await _context.TaiKhoans.FindAsync(id);

            if (taiKhoan == null)
            {
                return NotFound();
            }

            _context.TaiKhoans.Remove(taiKhoan);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}