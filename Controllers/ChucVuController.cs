using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.API.Data;
using QuanLyNhanSu.API.Models;

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

        // GET: api/ChucVu
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChucVu>>> GetChucVus()
        {
            return await _context.ChucVus.ToListAsync();
        }

        // GET: api/ChucVu/1
        [HttpGet("{id}")]
        public async Task<ActionResult<ChucVu>> GetChucVu(int id)
        {
            var chucVu = await _context.ChucVus.FindAsync(id);

            if (chucVu == null)
            {
                return NotFound();
            }

            return chucVu;
        }

        // POST: api/ChucVu
        [HttpPost]
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

        // PUT: api/ChucVu/1
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateChucVu(int id, ChucVu chucVu)
        {
            if (id != chucVu.MaCV)
            {
                return BadRequest();
            }

            _context.Entry(chucVu).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                bool tonTai = await _context.ChucVus
                    .AnyAsync(cv => cv.MaCV == id);

                if (!tonTai)
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }

        // DELETE: api/ChucVu/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChucVu(int id)
        {
            var chucVu = await _context.ChucVus.FindAsync(id);

            if (chucVu == null)
            {
                return NotFound();
            }

            try
            {
                _context.ChucVus.Remove(chucVu);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException)
            {
                return BadRequest(
                    "Không thể xóa chức vụ vì đang có nhân viên sử dụng chức vụ này."
                );
            }
        }
    }
}