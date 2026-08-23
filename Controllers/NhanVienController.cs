using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.API.Data;
using QuanLyNhanSu.API.Models;
using Microsoft.AspNetCore.Authorization;

namespace QuanLyNhanSu.API.Controllers
{
    [Route("api/nhan-viens")]
    [ApiController]
    public class NhanVienController : ControllerBase
    {
        private readonly QuanLyNhanSuDbContext _context;

        public NhanVienController(QuanLyNhanSuDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Trưởng phòng,Ban giám đốc")]
        public async Task<ActionResult<IEnumerable<NhanVien>>> GetAll()
        {
            return await _context.NhanViens.ToListAsync();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Trưởng phòng,Ban giám đốc")]
        public async Task<ActionResult<NhanVien>> GetById(int id)
        {
            var nhanVien = await _context.NhanViens.FindAsync(id);

            if (nhanVien == null)
            {
                return NotFound();
            }

            return nhanVien;
        }

        [HttpPost]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự")]
        public async Task<ActionResult<NhanVien>> Create(NhanVien nhanVien)
        {
            _context.NhanViens.Add(nhanVien);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = nhanVien.MaNV },
                nhanVien
            );
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự")]
        public async Task<IActionResult> Update(int id, NhanVien nhanVien)
        {
            if (id != nhanVien.MaNV)
            {
                return BadRequest();
            }

            _context.Entry(nhanVien).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                var exists = await _context.NhanViens
                    .AnyAsync(x => x.MaNV == id);

                if (!exists)
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
            var nhanVien = await _context.NhanViens.FindAsync(id);

            if (nhanVien == null)
            {
                return NotFound();
            }

            _context.NhanViens.Remove(nhanVien);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}