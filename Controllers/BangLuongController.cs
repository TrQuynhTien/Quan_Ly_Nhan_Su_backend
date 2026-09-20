using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.API.Data;
using QuanLyNhanSu.API.Models;
using QuanLyNhanSu.API.Services;

namespace QuanLyNhanSu.API.Controllers
{
    [Route("api/bang-luongs")]
    [ApiController]
    public class BangLuongController : ControllerBase
    {
        private readonly QuanLyNhanSuDbContext _context;
        private readonly BangLuongService _bangLuongService;

        public BangLuongController(
            QuanLyNhanSuDbContext context,
            BangLuongService bangLuongService)
        {
            _context = context;
            _bangLuongService = bangLuongService;
        }

        // Xem toàn bộ bảng lương
        [HttpGet]
        [Authorize(Roles =
            "Quản trị viên,Nhân viên nhân sự,Kế toán,Ban giám đốc")]
        public async Task<ActionResult<IEnumerable<BangLuong>>> GetAll()
        {
            var danhSach = await _context.BangLuongs
                .OrderByDescending(x => x.Nam)
                .ThenByDescending(x => x.Thang)
                .ToListAsync();

            return Ok(danhSach);
        }

        // Người dùng xem bảng lương của chính mình
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<BangLuong>>> GetMyPayroll()
        {
            var maNVClaim = User.FindFirst("MaNV")?.Value;

            if (maNVClaim == null)
            {
                return Unauthorized();
            }

            int maNV = int.Parse(maNVClaim);

            var bangLuongs = await _context.BangLuongs
                .Where(x => x.MaNV == maNV)
                .OrderByDescending(x => x.Nam)
                .ThenByDescending(x => x.Thang)
                .ToListAsync();

            return Ok(bangLuongs);
        }

        // Xem chi tiết một bảng lương
        [HttpGet("{id}")]
        [Authorize(Roles =
            "Quản trị viên,Nhân viên nhân sự,Kế toán,Ban giám đốc")]
        public async Task<ActionResult<BangLuong>> GetById(int id)
        {
            var bangLuong = await _context.BangLuongs
                .FirstOrDefaultAsync(x => x.MaLuong == id);

            if (bangLuong == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy bảng lương."
                });
            }

            return Ok(bangLuong);
        }

        // Tính tự động / tính lại bảng lương
        [HttpPost("tinh-luong")]
        [Authorize(Roles = "Quản trị viên,Kế toán")]
        public async Task<ActionResult> TinhLuong(
            int maNV,
            int thang,
            int nam)
        {
            var nhanVienTonTai = await _context.NhanViens
                .AnyAsync(x => x.MaNV == maNV);

            if (!nhanVienTonTai)
            {
                return BadRequest(new
                {
                    message = "Nhân viên không tồn tại."
                });
            }

            var result = await _bangLuongService
                .CalculatePayrollAsync(maNV, thang, nam);

            if (result.Status == "NO_CONTRACT")
            {
                return BadRequest(new
                {
                    message = "Nhân viên không có hợp đồng phù hợp."
                });
            }

            if (result.Status == "INVALID_MONTH")
            {
                return BadRequest(new
                {
                    message = "Tháng phải từ 1 đến 12."
                });
            }

            if (result.Status == "INVALID_YEAR")
            {
                return BadRequest(new
                {
                    message = "Năm không hợp lệ."
                });
            }

            return Ok(new
            {
                message = "Tính lương thành công.",
                data = result.Data
            });
        }

        // Xóa bảng lương khi dữ liệu được tạo sai
        [HttpDelete("{id}")]
        [Authorize(Roles = "Quản trị viên,Kế toán")]
        public async Task<IActionResult> Delete(int id)
        {
            var bangLuong = await _context.BangLuongs
                .FirstOrDefaultAsync(x => x.MaLuong == id);

            if (bangLuong == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy bảng lương."
                });
            }

            _context.BangLuongs.Remove(bangLuong);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}