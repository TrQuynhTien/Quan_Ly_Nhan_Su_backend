using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.API.Data;
using QuanLyNhanSu.API.Models;
using Microsoft.AspNetCore.Authorization;

namespace QuanLyNhanSu.API.Controllers
{
    [ApiController]
    [Route("api/chuc-vus")]
    public class ChucVuController : ControllerBase
    {
        private readonly QuanLyNhanSuDbContext _context;

        public ChucVuController(QuanLyNhanSuDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Trưởng phòng,Ban giám đốc")]
        public async Task<ActionResult<IEnumerable<ChucVu>>> GetChucVus()
        {
            return await _context.ChucVus.ToListAsync();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Trưởng phòng,Ban giám đốc")]
        public async Task<ActionResult<ChucVu>> GetChucVu(int id)
        {
            var chucVu = await _context.ChucVus.FindAsync(id);

            if (chucVu == null)
            {
                return NotFound();
            }

            return chucVu;
        }

        [HttpPost]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự")]
        public async Task<ActionResult<ChucVu>> CreateChucVu(ChucVu chucVu)
        {
            _context.ChucVus.Add(chucVu);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetChucVu),
                new { id = chucVu.MaCV },
                chucVu
            );
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự")]
        public async Task<IActionResult> UpdateChucVu(
            int id,
            ChucVu chucVu)
        {
            if (id != chucVu.MaCV)
            {
                return BadRequest(new
                {
                    message = "Mã chức vụ không hợp lệ."
                });
            }

            var chucVuCu = await _context.ChucVus
                .FirstOrDefaultAsync(x => x.MaCV == id);

            if (chucVuCu == null)
            {
                return NotFound();
            }

            chucVuCu.TenCV = chucVu.TenCV;
            chucVuCu.MoTa = chucVu.MoTa;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự")]
        public async Task<IActionResult> DeleteChucVu(int id)
        {
            var chucVu = await _context.ChucVus.FindAsync(id);

            if (chucVu == null)
            {
                return NotFound();
            }

            var dangDuocSuDung = await _context.NhanViens
                .AnyAsync(x => x.MaCV == id);

            if (dangDuocSuDung)
            {
                return Conflict(new
                {
                    message = "Không thể xóa chức vụ vì đang có nhân viên sử dụng chức vụ này."
                });
            }

            _context.ChucVus.Remove(chucVu);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}