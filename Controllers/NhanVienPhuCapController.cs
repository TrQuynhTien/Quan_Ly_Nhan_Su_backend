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

                var danhSach = await _context.NhanVienPhuCaps
                    .Join(
                        _context.NhanViens,
                        nvpc => nvpc.MaNV,
                        nv => nv.MaNV,
                        (nvpc, nv) => new
                        {
                            NhanVienPhuCap = nvpc,
                            MaPB = nv.MaPB
                        }
                    )
                    .Where(x => x.MaPB == truongPhong.MaPB)
                    .Select(x => x.NhanVienPhuCap)
                    .ToListAsync();

                return Ok(danhSach);
            }

            return Ok(await _context.NhanVienPhuCaps.ToListAsync());
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
        [HttpGet("{maNV}/{maPC}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Trưởng phòng,Ban giám đốc")]
        public async Task<ActionResult<NhanVienPhuCap>> GetById(
            int maNV,
            int maPC)
        {
            var nhanVienPhuCap = await _context.NhanVienPhuCaps
                .FindAsync(maNV, maPC);

            if (nhanVienPhuCap == null)
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

                int maNVTruongPhong = int.Parse(maNVClaim);

                var truongPhong = await _context.NhanViens
                    .FirstOrDefaultAsync(x => x.MaNV == maNVTruongPhong);

                var nhanVien = await _context.NhanViens
                    .FirstOrDefaultAsync(x => x.MaNV == nhanVienPhuCap.MaNV);

                if (truongPhong == null || nhanVien == null)
                {
                    return NotFound();
                }

                if (truongPhong.MaPB != nhanVien.MaPB)
                {
                    return Forbid();
                }
            }

            return Ok(nhanVienPhuCap);
        }

        [HttpPost]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán")]
        public async Task<ActionResult<NhanVienPhuCap>> Create(
            NhanVienPhuCap nhanVienPhuCap)
        {
            var nhanVienTonTai = await _context.NhanViens
                .AnyAsync(x => x.MaNV == nhanVienPhuCap.MaNV);

            if (!nhanVienTonTai)
            {
                return BadRequest(new
                {
                    message = "Nhân viên không tồn tại."
                });
            }

            var phuCapTonTai = await _context.PhuCaps
                .AnyAsync(x => x.MaPC == nhanVienPhuCap.MaPC);

            if (!phuCapTonTai)
            {
                return BadRequest(new
                {
                    message = "Phụ cấp không tồn tại."
                });
            }

            var daGan = await _context.NhanVienPhuCaps
                .AnyAsync(x =>
                    x.MaNV == nhanVienPhuCap.MaNV &&
                    x.MaPC == nhanVienPhuCap.MaPC);

            if (daGan)
            {
                return Conflict(new
                {
                    message = "Nhân viên đã được gán phụ cấp này."
                });
            }
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

            var banGhiCu = await _context.NhanVienPhuCaps
                .FirstOrDefaultAsync(x =>
                    x.MaNV == maNV &&
                    x.MaPC == maPC);

            if (banGhiCu == null)
            {
                return NotFound();
            }

            banGhiCu.TrangThai = nhanVienPhuCap.TrangThai;

            await _context.SaveChangesAsync();

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