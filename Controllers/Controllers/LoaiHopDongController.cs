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

        // GET: api/loai-hop-dongs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoaiHopDong>>> GetAll()
        {
            return await _context.LoaiHopDongs.ToListAsync();
        }

        // GET: api/loai-hop-dongs/1
        [HttpGet("{id}")]
        public async Task<ActionResult<LoaiHopDong>> GetById(int id)
        {
            var loaiHopDong = await _context.LoaiHopDongs.FindAsync(id);

            if (loaiHopDong == null)
            {
                return NotFound();
            }

            return loaiHopDong;
        }

        // POST: api/loai-hop-dongs
        [HttpPost]
        public async Task<ActionResult<LoaiHopDong>> Create(LoaiHopDong loaiHopDong)
        {
            _context.LoaiHopDongs.Add(loaiHopDong);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = loaiHopDong.MaLoaiHD },
                loaiHopDong
            );
        }

        // PUT: api/loai-hop-dongs/1
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, LoaiHopDong loaiHopDong)
        {
            if (id != loaiHopDong.MaLoaiHD)
            {
                return BadRequest();
            }

            _context.Entry(loaiHopDong).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                var exists = await _context.LoaiHopDongs
                    .AnyAsync(x => x.MaLoaiHD == id);

                if (!exists)
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }

        // DELETE: api/loai-hop-dongs/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var loaiHopDong = await _context.LoaiHopDongs.FindAsync(id);

            if (loaiHopDong == null)
            {
                return NotFound();
            }

            _context.LoaiHopDongs.Remove(loaiHopDong);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}