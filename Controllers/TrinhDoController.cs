using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.API.Data;
using QuanLyNhanSu.API.Models;
using Microsoft.AspNetCore.Authorization;

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

        [HttpGet]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Trưởng phòng,Ban giám đốc")]
        public async Task<ActionResult<IEnumerable<TrinhDo>>> GetAll()
        {
            return await _context.TrinhDos.ToListAsync();
        }
        [HttpGet("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Trưởng phòng,Ban giám đốc")]
        public async Task<ActionResult<TrinhDo>> GetById(int id)   
        {
            var trinhDo = await _context.TrinhDos.FindAsync(id);

            if (trinhDo == null)
            {
                return NotFound();
            }

            return trinhDo;
        }
        [HttpPost]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự")]
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
   
        [HttpPut("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự")]
        public async Task<IActionResult> Update(int id, TrinhDo trinhDo)
        {
            if (id != trinhDo.MaTD)
            {
                return BadRequest(new
                {
                    message = "Mã trình độ không hợp lệ."
                });
            }

            var trinhDoCu = await _context.TrinhDos
                .FirstOrDefaultAsync(x => x.MaTD == id);

            if (trinhDoCu == null)
            {
                return NotFound();
            }

            trinhDoCu.TenTD = trinhDo.TenTD;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự")]
        public async Task<IActionResult> Delete(int id)
        {
            var trinhDo = await _context.TrinhDos.FindAsync(id);

            if (trinhDo == null)
            {
                return NotFound();
            }

            var dangDuocSuDung = await _context.NhanViens
                .AnyAsync(x => x.MaTD == id);

            if (dangDuocSuDung)
            {
                return Conflict(new
                {
                    message = "Không thể xóa trình độ vì đang có nhân viên sử dụng trình độ này."
                });
            }

            _context.TrinhDos.Remove(trinhDo);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}