using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.API.Data;
using QuanLyNhanSu.API.Models;
using Microsoft.AspNetCore.Authorization;

namespace QuanLyNhanSu.API.Controllers
{
    [Route("api/khen-thuong-ky-luats")]
    [ApiController]
    public class KhenThuongKyLuatController : ControllerBase
    {
        private readonly QuanLyNhanSuDbContext _context;

        public KhenThuongKyLuatController(QuanLyNhanSuDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Ban giám đốc,Nhân viên")]
        public async Task<ActionResult<IEnumerable<KhenThuongKyLuat>>> GetAll()
        {
            return await _context.KhenThuongKyLuats.ToListAsync();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Ban giám đốc,Nhân viên")]
        public async Task<ActionResult<KhenThuongKyLuat>> GetById(int id)
        {
            var khenThuongKyLuat =
                await _context.KhenThuongKyLuats.FindAsync(id);

            if (khenThuongKyLuat == null)
            {
                return NotFound();
            }

            return khenThuongKyLuat;
        }

        [HttpPost]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự")]
        public async Task<ActionResult<KhenThuongKyLuat>> Create(
            KhenThuongKyLuat khenThuongKyLuat)
        {
            _context.KhenThuongKyLuats.Add(khenThuongKyLuat);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = khenThuongKyLuat.MaKTKL },
                khenThuongKyLuat
            );
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự")]
        public async Task<IActionResult> Update(
            int id,
            KhenThuongKyLuat khenThuongKyLuat)
        {
            if (id != khenThuongKyLuat.MaKTKL)
            {
                return BadRequest();
            }

            _context.Entry(khenThuongKyLuat).State =
                EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.KhenThuongKyLuats
                    .AnyAsync(x => x.MaKTKL == id))
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
            var khenThuongKyLuat =
                await _context.KhenThuongKyLuats.FindAsync(id);

            if (khenThuongKyLuat == null)
            {
                return NotFound();
            }

            _context.KhenThuongKyLuats.Remove(khenThuongKyLuat);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}