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
            if (id != taiKhoan.MaTK)
            {
                return BadRequest();
            }

            var taiKhoanCu = await _context.TaiKhoans
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaTK == id);

            if (taiKhoanCu == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(taiKhoan.MatKhau))
            {
                taiKhoan.MatKhau =
                    BCrypt.Net.BCrypt.HashPassword(taiKhoan.MatKhau);
            }
            else
            {
                taiKhoan.MatKhau = taiKhoanCu.MatKhau;
            }

            _context.Entry(taiKhoan).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return NoContent();
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