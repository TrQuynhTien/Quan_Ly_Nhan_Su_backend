using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.API.Data;
using QuanLyNhanSu.API.Models;
using Microsoft.AspNetCore.Authorization;

namespace QuanLyNhanSu.API.Controllers
{
    [Route("api/nhan-vien-phu-caps")]
    [ApiController]
    public class NhanVienPhuCapController : ControllerBase
    {
        private readonly QuanLyNhanSuDbContext _context;

        public NhanVienPhuCapController(QuanLyNhanSuDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Trưởng phòng,Ban giám đốc")]
        public async Task<ActionResult<IEnumerable<NhanVienPhuCap>>> GetAll()
        {
            return await _context.NhanVienPhuCaps.ToListAsync();
        }

        [HttpGet("{maNV}/{maPC}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Trưởng phòng,Ban giám đốc")]
        public async Task<ActionResult<NhanVienPhuCap>> GetById(
            int maNV,
            int maPC)
        {
            var nhanVienPhuCap =
                await _context.NhanVienPhuCaps.FindAsync(maNV, maPC);

            if (nhanVienPhuCap == null)
            {
                return NotFound();
            }

            return nhanVienPhuCap;
        }
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<NhanVienPhuCap>>> GetMyAllowances()
        {
            var maNVClaim = User.FindFirst("MaNV")?.Value;

            if (maNVClaim == null)
            {
                return Unauthorized();
            }

            int maNV = int.Parse(maNVClaim);

            var danhSach = await _context.NhanVienPhuCaps
                .Where(x => x.MaNV == maNV)
                .ToListAsync();

            return Ok(danhSach);
        }

        [HttpPost]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán")]
        public async Task<ActionResult<NhanVienPhuCap>> Create(
            NhanVienPhuCap nhanVienPhuCap)
        {
            _context.NhanVienPhuCaps.Add(nhanVienPhuCap);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new
                {
                    maNV = nhanVienPhuCap.MaNV,
                    maPC = nhanVienPhuCap.MaPC
                },
                nhanVienPhuCap
            );
        }

        [HttpPut("{maNV}/{maPC}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán")]
        public async Task<IActionResult> Update(
            int maNV,
            int maPC,
            NhanVienPhuCap nhanVienPhuCap)
        {
            if (maNV != nhanVienPhuCap.MaNV ||
                maPC != nhanVienPhuCap.MaPC)
            {
                return BadRequest();
            }

            _context.Entry(nhanVienPhuCap).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.NhanVienPhuCaps.AnyAsync(
                    x => x.MaNV == maNV && x.MaPC == maPC))
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }

        [HttpDelete("{maNV}/{maPC}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán")]
        public async Task<IActionResult> Delete(int maNV, int maPC)
        {
            var nhanVienPhuCap =
                await _context.NhanVienPhuCaps.FindAsync(maNV, maPC);

            if (nhanVienPhuCap == null)
            {
                return NotFound();
            }

            _context.NhanVienPhuCaps.Remove(nhanVienPhuCap);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}