using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.API.Data;
using QuanLyNhanSu.API.Models;
using QuanLyNhanSu.API.Services;

namespace QuanLyNhanSu.API.Controllers
{
    [Route("api/nghi-pheps")]
    [ApiController]
    public class NghiPhepController : ControllerBase
    {
        private readonly QuanLyNhanSuDbContext _context;
        private readonly NghiPhepService _nghiPhepService;

        public NghiPhepController(QuanLyNhanSuDbContext context, NghiPhepService nghiPhepService)
        {
            _context = context;
            _nghiPhepService = nghiPhepService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NghiPhep>>> GetAll()
        {
            return await _context.NghiPheps.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<NghiPhep>> GetById(int id)
        {
            var nghiPhep = await _context.NghiPheps.FindAsync(id);

            if (nghiPhep == null)
            {
                return NotFound();
            }

            return nghiPhep;
        }
        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Trưởng phòng")]
        public async Task<IActionResult> Approve(int id, int nguoiDuyet)
        {
            var success = await _nghiPhepService
                .ApproveLeaveRequestAsync(id, nguoiDuyet);

            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
        [HttpPut("{id}/reject")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Trưởng phòng")]
        public async Task<IActionResult> Reject(int id, int nguoiDuyet)
        {
            var success = await _nghiPhepService
                .RejectLeaveRequestAsync(id, nguoiDuyet);

            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
        [HttpPost]
        public async Task<ActionResult<NghiPhep>> Create(NghiPhep nghiPhep)
        {
            _context.NghiPheps.Add(nghiPhep);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = nghiPhep.MaNP },
                nghiPhep
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, NghiPhep nghiPhep)
        {
            if (id != nghiPhep.MaNP)
            {
                return BadRequest();
            }

            _context.Entry(nghiPhep).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.NghiPheps.AnyAsync(x => x.MaNP == id))
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var nghiPhep = await _context.NghiPheps.FindAsync(id);

            if (nghiPhep == null)
            {
                return NotFound();
            }

            _context.NghiPheps.Remove(nghiPhep);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    
    }
}