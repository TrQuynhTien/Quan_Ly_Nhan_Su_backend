using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.API.Data;
using QuanLyNhanSu.API.Models;
using Microsoft.AspNetCore.Authorization;

namespace QuanLyNhanSu.API.Controllers
{
    [Route("api/cham-congs")]
    [ApiController]
    public class ChamCongController : ControllerBase
    {
        private readonly QuanLyNhanSuDbContext _context;

        public ChamCongController(QuanLyNhanSuDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Trưởng phòng,Ban giám đốc")]
        public async Task<ActionResult<IEnumerable<ChamCong>>> GetAll()
        {
            return await _context.ChamCongs.ToListAsync();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Trưởng phòng,Ban giám đốc")]
        public async Task<ActionResult<ChamCong>> GetById(int id)
        {
            var chamCong = await _context.ChamCongs.FindAsync(id);

            if (chamCong == null)
            {
                return NotFound();
            }

            return chamCong;
        }

        [HttpPost]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán")]
        public async Task<ActionResult<ChamCong>> Create(ChamCong chamCong)
        {
            _context.ChamCongs.Add(chamCong);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = chamCong.MaCC },
                chamCong
            );
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán")]
        public async Task<IActionResult> Update(int id, ChamCong chamCong)
        {
            if (id != chamCong.MaCC)
            {
                return BadRequest();
            }

            _context.Entry(chamCong).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.ChamCongs.AnyAsync(x => x.MaCC == id))
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán")]
        public async Task<IActionResult> Delete(int id)
        {
            var chamCong = await _context.ChamCongs.FindAsync(id);

            if (chamCong == null)
            {
                return NotFound();
            }

            _context.ChamCongs.Remove(chamCong);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}