using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.API.Data;
using QuanLyNhanSu.API.Models;
using Microsoft.AspNetCore.Authorization;

namespace QuanLyNhanSu.API.Controllers
{
    [Route("api/loai-nghi-pheps")]
    [ApiController]
    public class LoaiNghiPhepController : ControllerBase
    {
        private readonly QuanLyNhanSuDbContext _context;

        public LoaiNghiPhepController(QuanLyNhanSuDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Trưởng phòng,Ban giám đốc,Nhân viên")]
        public async Task<ActionResult<IEnumerable<LoaiNghiPhep>>> GetAll()
        {
            return await _context.LoaiNghiPheps.ToListAsync();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Trưởng phòng,Ban giám đốc,Nhân viên")]
        public async Task<ActionResult<LoaiNghiPhep>> GetById(int id)
        {
            var loaiNghiPhep = await _context.LoaiNghiPheps.FindAsync(id);

            if (loaiNghiPhep == null)
            {
                return NotFound();
            }

            return loaiNghiPhep;
        }

        [HttpPost]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự")]
        public async Task<ActionResult<LoaiNghiPhep>> Create(LoaiNghiPhep loaiNghiPhep)
        {
            _context.LoaiNghiPheps.Add(loaiNghiPhep);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = loaiNghiPhep.MaLoaiNP },
                loaiNghiPhep
            );
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự")]
        public async Task<IActionResult> Update(
            int id,
            LoaiNghiPhep loaiNghiPhep)
        {
            if (id != loaiNghiPhep.MaLoaiNP)
            {
                return BadRequest(new
                {
                    message = "Mã loại nghỉ phép không hợp lệ."
                });
            }

            var loaiCu = await _context.LoaiNghiPheps
                .FirstOrDefaultAsync(x => x.MaLoaiNP == id);

            if (loaiCu == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy loại nghỉ phép."
                });
            }

            loaiCu.TenLoaiNP = loaiNghiPhep.TenLoaiNP;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Cập nhật loại nghỉ phép thành công."
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự")]
        public async Task<IActionResult> Delete(int id)
        {
            var loaiNghiPhep = await _context.LoaiNghiPheps.FindAsync(id);

            if (loaiNghiPhep == null)
            {
                return NotFound();
            }

            _context.LoaiNghiPheps.Remove(loaiNghiPhep);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}