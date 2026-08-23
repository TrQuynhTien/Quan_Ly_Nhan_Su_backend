using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.API.Data;
using QuanLyNhanSu.API.Models;
using Microsoft.AspNetCore.Authorization;

namespace QuanLyNhanSu.API.Controllers
{
    [Route("api/quyens")]
    [ApiController]
    public class QuyenController : ControllerBase
    {
        private readonly QuanLyNhanSuDbContext _context;

        public QuyenController(QuanLyNhanSuDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Quản trị viên")]
        public async Task<ActionResult<IEnumerable<Quyen>>> GetAll()
        {
            return await _context.Quyens.ToListAsync();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Quản trị viên")]
        public async Task<ActionResult<Quyen>> GetById(int id)
        {
            var quyen = await _context.Quyens.FindAsync(id);

            if (quyen == null)
            {
                return NotFound();
            }

            return quyen;
        }

        [HttpPost]
        [Authorize(Roles = "Quản trị viên")]
        public async Task<ActionResult<Quyen>> Create(Quyen quyen)
        {
            _context.Quyens.Add(quyen);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = quyen.MaQuyen },
                quyen
            );
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Quản trị viên")]
        public async Task<IActionResult> Update(int id, Quyen quyen)
        {
            if (id != quyen.MaQuyen)
            {
                return BadRequest();
            }

            _context.Entry(quyen).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Quyens.AnyAsync(x => x.MaQuyen == id))
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Quản trị viên")]
        public async Task<IActionResult> Delete(int id)
        {
            var quyen = await _context.Quyens.FindAsync(id);

            if (quyen == null)
            {
                return NotFound();
            }

            _context.Quyens.Remove(quyen);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}