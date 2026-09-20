using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.API.Data;
using QuanLyNhanSu.API.Models;

namespace QuanLyNhanSu.API.Controllers
{
    [Route("api/loai-hop-dongs")]
    [ApiController]
    public class LoaiHopDongController : ControllerBase
    {
        private readonly QuanLyNhanSuDbContext _context;

        public LoaiHopDongController(QuanLyNhanSuDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Trưởng phòng,Ban giám đốc")]
        public async Task<ActionResult<IEnumerable<LoaiHopDong>>> GetAll()
        {
            return await _context.LoaiHopDongs.ToListAsync();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Trưởng phòng,Ban giám đốc")]
        public async Task<ActionResult<LoaiHopDong>> GetById(int id)
        {
            var loaiHopDong = await _context.LoaiHopDongs.FindAsync(id);

            if (loaiHopDong == null)
            {
                return NotFound();
            }

            return loaiHopDong;
        }

        [HttpPost]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự")]
        public async Task<ActionResult<LoaiHopDong>> Create(
            LoaiHopDong loaiHopDong)
        {
            _context.LoaiHopDongs.Add(loaiHopDong);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = loaiHopDong.MaLoaiHD },
                loaiHopDong
            );
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự")]
        public async Task<IActionResult> Update(
            int id,
            LoaiHopDong loaiHopDong)
        {
            if (id != loaiHopDong.MaLoaiHD)
            {
                return BadRequest(new
                {
                    message = "Mã loại hợp đồng không hợp lệ."
                });
            }

            var loaiHopDongCu = await _context.LoaiHopDongs
                .FirstOrDefaultAsync(x => x.MaLoaiHD == id);

            if (loaiHopDongCu == null)
            {
                return NotFound();
            }

            loaiHopDongCu.TenLoaiHD = loaiHopDong.TenLoaiHD;
            loaiHopDongCu.MoTa = loaiHopDong.MoTa;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự")]
        public async Task<IActionResult> Delete(int id)
        {
            var loaiHopDong = await _context.LoaiHopDongs
                .FindAsync(id);

            if (loaiHopDong == null)
            {
                return NotFound();
            }

            var dangDuocSuDung = await _context.HopDongs
                .AnyAsync(x => x.MaLoaiHD == id);

            if (dangDuocSuDung)
            {
                return Conflict(new
                {
                    message = "Không thể xóa loại hợp đồng vì đang có hợp đồng sử dụng loại này."
                });
            }

            _context.LoaiHopDongs.Remove(loaiHopDong);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
