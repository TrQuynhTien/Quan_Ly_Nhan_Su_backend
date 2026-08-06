using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.API.Data;
using QuanLyNhanSu.API.Models;

namespace QuanLyNhanSu.API.Controllers
{
    [Route("api/trinh-dos")]
    [ApiController]
    public class TrinhDoController : ControllerBase
    {
        private readonly QuanLyNhanSuDbContext _context;

        public TrinhDoController(QuanLyNhanSuDbContext context)
        {
            _context = context;
        }

        // GET: api/trinh-dos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TrinhDo>>> GetAll()
        {
            return await _context.TrinhDos.ToListAsync();
        }

        // GET: api/trinh-dos/1
        [HttpGet("{id}")]
        public async Task<ActionResult<TrinhDo>> GetById(int id)
        {
            var trinhDo = await _context.TrinhDos.FindAsync(id);

            if (trinhDo == null)
            {
                return NotFound();
            }

            return trinhDo;
        }

        // POST: api/trinh-dos
        [HttpPost]
        public async Task<ActionResult<TrinhDo>> Create(TrinhDo trinhDo)
        {
            _context.TrinhDos.Add(trinhDo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = trinhDo.MaTD },
                trinhDo
            );
        }

        // PUT: api/trinh-dos/1
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, TrinhDo trinhDo)
        {
            if (id != trinhDo.MaTD)
            {
                return BadRequest();
            }

            _context.Entry(trinhDo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                var exists = await _context.TrinhDos.AnyAsync(x => x.MaTD == id);

                if (!exists)
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }

        // DELETE: api/trinh-dos/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var trinhDo = await _context.TrinhDos.FindAsync(id);

            if (trinhDo == null)
            {
                return NotFound();
            }

            _context.TrinhDos.Remove(trinhDo);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}