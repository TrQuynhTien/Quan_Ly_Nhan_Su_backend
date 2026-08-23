using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.API.Data;
using QuanLyNhanSu.API.Models;
using Microsoft.AspNetCore.Authorization;

namespace QuanLyNhanSu.API.Controllers
{
    [Route("api/loai-cas")]
    [ApiController]
    public class LoaiCaController : ControllerBase
    {
        private readonly QuanLyNhanSuDbContext _context;

        public LoaiCaController(QuanLyNhanSuDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Trưởng phòng,Ban giám đốc")]
        public async Task<ActionResult<IEnumerable<LoaiCa>>> GetAll()
        {
            return await _context.LoaiCas.ToListAsync();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Trưởng phòng,Ban giám đốc")]
        public async Task<ActionResult<LoaiCa>> GetById(int id)
        {
            var loaiCa = await _context.LoaiCas.FindAsync(id);

            if (loaiCa == null)
            {
                return NotFound();
            }

            return loaiCa;
        }

        [HttpPost]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán")]
        public async Task<ActionResult<LoaiCa>> Create(LoaiCa loaiCa)
        {
            _context.LoaiCas.Add(loaiCa);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = loaiCa.MaCa },
                loaiCa
            );
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán")]
        public async Task<IActionResult> Update(int id, LoaiCa loaiCa)
        {
            if (id != loaiCa.MaCa)
            {
                return BadRequest();
            }

            _context.Entry(loaiCa).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.LoaiCas.AnyAsync(x => x.MaCa == id))
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
            var loaiCa = await _context.LoaiCas.FindAsync(id);

            if (loaiCa == null)
            {
                return NotFound();
            }

            _context.LoaiCas.Remove(loaiCa);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}