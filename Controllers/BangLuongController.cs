using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.API.Data;
using QuanLyNhanSu.API.Models;
using Microsoft.AspNetCore.Authorization;

namespace QuanLyNhanSu.API.Controllers
{
    [Route("api/bang-luongs")]
    [ApiController]
    public class BangLuongController : ControllerBase
    {
        private readonly QuanLyNhanSuDbContext _context;

        public BangLuongController(QuanLyNhanSuDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Quản trị viên,Kế toán,Ban giám đốc")]
        public async Task<ActionResult<IEnumerable<BangLuong>>> GetAll()
        {
            return await _context.BangLuongs.ToListAsync();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Quản trị viên,Kế toán,Ban giám đốc")]
        public async Task<ActionResult<BangLuong>> GetById(int id)
        {
            var bangLuong = await _context.BangLuongs.FindAsync(id);

            if (bangLuong == null)
            {
                return NotFound();
            }

            return bangLuong;
        }

        [HttpPost]
        [Authorize(Roles = "Quản trị viên,Kế toán")]
        public async Task<ActionResult<BangLuong>> Create(BangLuong bangLuong)
        {
            _context.BangLuongs.Add(bangLuong);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = bangLuong.MaLuong },
                bangLuong
            );
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Quản trị viên,Kế toán")]
        public async Task<IActionResult> Update(int id, BangLuong bangLuong)
        {
            if (id != bangLuong.MaLuong)
            {
                return BadRequest();
            }

            _context.Entry(bangLuong).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.BangLuongs.AnyAsync(x => x.MaLuong == id))
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Quản trị viên,Kế toán")]
        public async Task<IActionResult> Delete(int id)
        {
            var bangLuong = await _context.BangLuongs.FindAsync(id);

            if (bangLuong == null)
            {
                return NotFound();
            }

            _context.BangLuongs.Remove(bangLuong);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}