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
            var danhSach = await _context.Quyens
                .OrderBy(x => x.MaQuyen)
                .ToListAsync();

            return Ok(danhSach);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Quản trị viên")]
        public async Task<ActionResult<Quyen>> GetById(int id)
        {
            var quyen = await _context.Quyens
                .FirstOrDefaultAsync(x => x.MaQuyen == id);

            if (quyen == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy quyền."
                });
            }

            return Ok(quyen);
        }
    }
}