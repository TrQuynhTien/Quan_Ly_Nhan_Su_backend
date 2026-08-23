using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.API.Data;
using QuanLyNhanSu.API.Models;
using Microsoft.AspNetCore.Authorization;

namespace QuanLyNhanSu.API.Controllers
{
    [ApiController]
    [Route("api/phong-bans")]
    public class PhongBanController : ControllerBase
    {
        private readonly QuanLyNhanSuDbContext _context;

        public PhongBanController(QuanLyNhanSuDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Trưởng phòng,Ban giám đốc")]
        public async Task<ActionResult<IEnumerable<PhongBan>>> GetPhongBans()
        {
            return await _context.PhongBans.ToListAsync();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Trưởng phòng,Ban giám đốc")]
        public async Task<ActionResult<PhongBan>> GetPhongBan(int id)
        {
            var phongBan = await _context.PhongBans.FindAsync(id);

            if (phongBan == null)
            {
                return NotFound();
            }

            return phongBan;
        }

        [HttpPost]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự")]
        public async Task<ActionResult<PhongBan>> CreatePhongBan(PhongBan phongBan)
        {
            _context.PhongBans.Add(phongBan);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetPhongBan),
                new { id = phongBan.MaPB },
                phongBan
            );
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự")]
        public async Task<IActionResult> UpdatePhongBan(
            int id,
            PhongBan phongBan)
        {
            if (id != phongBan.MaPB)
            {
                return BadRequest();
            }

            _context.Entry(phongBan).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                bool tonTai = await _context.PhongBans
                    .AnyAsync(pb => pb.MaPB == id);

                if (!tonTai)
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự")]
        public async Task<IActionResult> DeletePhongBan(int id)
        {
            var phongBan = await _context.PhongBans.FindAsync(id);

            if (phongBan == null)
            {
                return NotFound();
            }

            _context.PhongBans.Remove(phongBan);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}