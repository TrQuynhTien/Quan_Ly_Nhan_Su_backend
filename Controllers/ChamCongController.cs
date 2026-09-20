using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.API.Data;
using QuanLyNhanSu.API.Models;

namespace QuanLyNhanSu.API.Controllers
{
    [Route("api/cham-congs")]
    [ApiController]
    public class ChamCongController : ControllerBase
    {
        private readonly QuanLyNhanSuDbContext _context;

        public ChamCongController(QuanLyNhanSuDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Trưởng phòng,Ban giám đốc")]
        public async Task<ActionResult<IEnumerable<ChamCong>>> GetAll()
        {
            if (User.IsInRole("Trưởng phòng"))
            {
                var maNVClaim = User.FindFirst("MaNV")?.Value;

                if (maNVClaim == null)
                {
                    return Unauthorized();
                }

                int maNV = int.Parse(maNVClaim);

                var truongPhong = await _context.NhanViens
                    .FirstOrDefaultAsync(x => x.MaNV == maNV);

                if (truongPhong == null)
                {
                    return NotFound("Không tìm thấy thông tin trưởng phòng.");
                }

                var danhSach = await _context.ChamCongs
                    .Join(
                        _context.NhanViens,
                        cc => cc.MaNV,
                        nv => nv.MaNV,
                        (cc, nv) => new
                        {
                            ChamCong = cc,
                            MaPB = nv.MaPB
                        }
                    )
                    .Where(x => x.MaPB == truongPhong.MaPB)
                    .Select(x => x.ChamCong)
                    .ToListAsync();

                return Ok(danhSach);
            }

            return Ok(await _context.ChamCongs.ToListAsync());
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ChamCong>>> GetMyAttendance()
        {
            var maNVClaim = User.FindFirst("MaNV")?.Value;

            if (maNVClaim == null)
            {
                return Unauthorized();
            }

            int maNV = int.Parse(maNVClaim);

            var danhSach = await _context.ChamCongs
                .Where(x => x.MaNV == maNV)
                .OrderByDescending(x => x.NgayChamCong)
                .ToListAsync();

            return Ok(danhSach);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Trưởng phòng,Ban giám đốc")]
        public async Task<ActionResult<ChamCong>> GetById(int id)
        {
            var chamCong = await _context.ChamCongs.FindAsync(id);

            if (chamCong == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Trưởng phòng"))
            {
                var maNVClaim = User.FindFirst("MaNV")?.Value;

                if (maNVClaim == null)
                {
                    return Unauthorized();
                }

                int maNV = int.Parse(maNVClaim);

                var truongPhong = await _context.NhanViens
                    .FirstOrDefaultAsync(x => x.MaNV == maNV);

                var nhanVien = await _context.NhanViens
                    .FirstOrDefaultAsync(x => x.MaNV == chamCong.MaNV);

                if (truongPhong == null || nhanVien == null)
                {
                    return NotFound();
                }

                if (truongPhong.MaPB != nhanVien.MaPB)
                {
                    return Forbid();
                }
            }

            return Ok(chamCong);
        }

        [HttpPost]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán")]
        public async Task<ActionResult<ChamCong>> Create(ChamCong chamCong)
        {
            var nhanVienTonTai = await _context.NhanViens
                .AnyAsync(x => x.MaNV == chamCong.MaNV);

            if (!nhanVienTonTai)
            {
                return BadRequest(new
                {
                    message = "Nhân viên không tồn tại."
                });
            }

            var daChamCong = await _context.ChamCongs
                .AnyAsync(x =>
                    x.MaNV == chamCong.MaNV &&
                    x.MaCa == chamCong.MaCa &&
                    x.NgayChamCong.Date == chamCong.NgayChamCong.Date);

            if (daChamCong)
            {
                return Conflict(new
                {
                    message = "Nhân viên đã có chấm công cho ca này trong ngày."
                });
            }

            if (chamCong.SoGioLam < 0)
            {
                return BadRequest(new
                {
                    message = "Số giờ làm không hợp lệ."
                });
            }
            _context.ChamCongs.Add(chamCong);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = chamCong.MaCC },
                chamCong
            );
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán")]
        public async Task<IActionResult> Update(
            int id,
            ChamCong chamCong)
        {
            if (id != chamCong.MaCC)
            {
                return BadRequest();
            }

            var chamCongCu = await _context.ChamCongs
                .FirstOrDefaultAsync(x => x.MaCC == id);

            if (chamCongCu == null)
            {
                return NotFound();
            }

            var biTrung = await _context.ChamCongs
                .AnyAsync(x =>
                    x.MaCC != id &&
                    x.MaNV == chamCong.MaNV &&
                    x.MaCa == chamCong.MaCa &&
                    x.NgayChamCong.Date == chamCong.NgayChamCong.Date);

            if (biTrung)
            {
                return Conflict(new
                {
                    message = "Nhân viên đã có chấm công cho ca này trong ngày."
                });
            }

            if (chamCong.SoGioLam == null || chamCong.SoGioLam < 0)
            {
                return BadRequest(new
                {
                    message = "Số giờ làm không hợp lệ."
                });
            }

            chamCongCu.MaNV = chamCong.MaNV;
            chamCongCu.MaCa = chamCong.MaCa;
            chamCongCu.NgayChamCong = chamCong.NgayChamCong;
            chamCongCu.GioVao = chamCong.GioVao;
            chamCongCu.GioRa = chamCong.GioRa;
            chamCongCu.SoGioLam = chamCong.SoGioLam;
            chamCongCu.TrangThai = chamCong.TrangThai;
            chamCongCu.GhiChu = chamCong.GhiChu;

            await _context.SaveChangesAsync();

            return NoContent();

        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán")]
        public async Task<IActionResult> Delete(int id)
        {
            var chamCong = await _context.ChamCongs.FindAsync(id);

            if (chamCong == null)
            {
                return NotFound();
            }

            _context.ChamCongs.Remove(chamCong);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}