using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.API.Data;

namespace QuanLyNhanSu.API.Controllers
{
    [Route("api/thong-baos")]
    [ApiController]
    [Authorize]
    public class ThongBaoController : ControllerBase
    {
        private readonly QuanLyNhanSuDbContext _context;

        public ThongBaoController(QuanLyNhanSuDbContext context)
        {
            _context = context;
        }

        // Lấy thông báo của người đang đăng nhập
        [HttpGet("me")]
        public async Task<IActionResult> GetMyNotifications()
        {
            var maNVClaim = User.FindFirst("MaNV")?.Value;

            if (maNVClaim == null)
            {
                return Unauthorized();
            }

            int maNV = int.Parse(maNVClaim);

            var danhSach = await _context.ThongBaos
                .Where(x => x.MaNV == maNV)
                .OrderByDescending(x => x.NgayTao)
                .Select(x => new
                {
                    x.MaThongBao,
                    x.TieuDe,
                    x.NoiDung,
                    x.DaDoc,
                    x.NgayTao,
                    x.DuongDan
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                message = "Lấy danh sách thông báo thành công.",
                data = danhSach
            });
        }

        // Đánh dấu một thông báo là đã đọc
        [HttpPut("{maThongBao}/da-doc")]
        public async Task<IActionResult> MarkAsRead(int maThongBao)
        {
            var maNVClaim = User.FindFirst("MaNV")?.Value;

            if (maNVClaim == null)
            {
                return Unauthorized();
            }

            int maNV = int.Parse(maNVClaim);

            var thongBao = await _context.ThongBaos
                .FirstOrDefaultAsync(x =>
                    x.MaThongBao == maThongBao &&
                    x.MaNV == maNV);

            if (thongBao == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy thông báo."
                });
            }

            thongBao.DaDoc = true;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Đã đánh dấu thông báo là đã đọc."
            });
        }
    }
}