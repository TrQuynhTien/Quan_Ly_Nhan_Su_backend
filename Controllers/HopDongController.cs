using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.API.Data;
using QuanLyNhanSu.API.Models;
using Microsoft.AspNetCore.Authorization;

namespace QuanLyNhanSu.API.Controllers
{
    [Route("api/hop-dongs")]
    [ApiController]
    public class HopDongController : ControllerBase
    {
        private readonly QuanLyNhanSuDbContext _context;

        public HopDongController(QuanLyNhanSuDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Trưởng phòng,Ban giám đốc")]
        public async Task<ActionResult<IEnumerable<HopDong>>> GetAll()
        {
            var hopDongs = await _context.HopDongs.ToListAsync();

            foreach (var hopDong in hopDongs)
            {
                CapNhatTrangThai(hopDong);
            }

            await _context.SaveChangesAsync();

            return Ok(hopDongs);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Trưởng phòng,Ban giám đốc")]
        public async Task<ActionResult<HopDong>> GetById(int id)
        {
            var hopDong = await _context.HopDongs.FindAsync(id);

            if (hopDong == null)
            {
                return NotFound();
            }

            CapNhatTrangThai(hopDong);
            await _context.SaveChangesAsync();

            return Ok(hopDong);
        }
        private void CapNhatTrangThai(HopDong hopDong)
            {
                if (hopDong.NgayKetThuc == null || hopDong.NgayKetThuc.Value.Date >= DateTime.Today)
                {
                    hopDong.TrangThai = "Còn hiệu lực";
                }
                else
                {
                    hopDong.TrangThai = "Hết hiệu lực";
                }
            }
        [HttpPost]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự")]
        public async Task<ActionResult<HopDong>> Create(HopDong hopDong)
        {
            CapNhatTrangThai(hopDong);
            _context.HopDongs.Add(hopDong);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = hopDong.MaHD },
                hopDong
            );
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự")]
        public async Task<IActionResult> Update(int id, HopDong hopDong)
        {
            if (id != hopDong.MaHD)
            {
                return BadRequest();
            }
            CapNhatTrangThai(hopDong);
            _context.Entry(hopDong).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.HopDongs.AnyAsync(x => x.MaHD == id))
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự")]
        public async Task<IActionResult> Delete(int id)
        {
            var hopDong = await _context.HopDongs.FindAsync(id);

            if (hopDong == null)
            {
                return NotFound();
            }

            _context.HopDongs.Remove(hopDong);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}