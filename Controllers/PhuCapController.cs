using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.API.Data;
using QuanLyNhanSu.API.Models;
using Microsoft.AspNetCore.Authorization;

namespace QuanLyNhanSu.API.Controllers
{
    [Route("api/phu-caps")]
    [ApiController]
    public class PhuCapController : ControllerBase
    {
        private readonly QuanLyNhanSuDbContext _context;

        public PhuCapController(QuanLyNhanSuDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Trưởng phòng,Ban giám đốc")]
        public async Task<ActionResult<IEnumerable<PhuCap>>> GetAll()
        {
            return await _context.PhuCaps.ToListAsync();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Trưởng phòng,Ban giám đốc")]
        public async Task<ActionResult<PhuCap>> GetById(int id)
        {
            var phuCap = await _context.PhuCaps.FindAsync(id);

            if (phuCap == null)
            {
                return NotFound();
            }

            return phuCap;
        }

        [HttpPost]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán")]
        public async Task<ActionResult<PhuCap>> Create(PhuCap phuCap)
        {
            _context.PhuCaps.Add(phuCap);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = phuCap.MaPC },
                phuCap
            );
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán")]
        public async Task<IActionResult> Update(int id, PhuCap phuCap)
        {
            if (id != phuCap.MaPC)
            {
                return BadRequest();
            }

            _context.Entry(phuCap).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.PhuCaps.AnyAsync(x => x.MaPC == id))
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
            var phuCap = await _context.PhuCaps.FindAsync(id);

            if (phuCap == null)
            {
                return NotFound();
            }

            _context.PhuCaps.Remove(phuCap);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}