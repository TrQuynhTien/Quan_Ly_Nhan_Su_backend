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
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Trưởng phòng,Ban giám đốc")]
        public async Task<ActionResult<IEnumerable<KhenThuongKyLuat>>> GetAll()
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

                var danhSach = await _context.KhenThuongKyLuats
                    .Join(
                        _context.NhanViens,
                        ktkl => ktkl.MaNV,
                        nv => nv.MaNV,
                        (ktkl, nv) => new
                        {
                            KTKL = ktkl,
                            MaPB = nv.MaPB
                        }
                    )
                    .Where(x => x.MaPB == truongPhong.MaPB)
                    .Select(x => x.KTKL)
                    .ToListAsync();

                return Ok(danhSach);
            }

            return Ok(await _context.KhenThuongKyLuats
                .OrderByDescending(x => x.NgayQuyetDinh)
                .ToListAsync());
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Trưởng phòng,Ban giám đốc")]
        public async Task<ActionResult<KhenThuongKyLuat>> GetById(int id)
        {
            var khenThuongKyLuat =
                await _context.KhenThuongKyLuats.FindAsync(id);

            if (khenThuongKyLuat == null)
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
                    .FirstOrDefaultAsync(x => x.MaNV == khenThuongKyLuat.MaNV);

                if (truongPhong == null || nhanVien == null)
                {
                    return NotFound();
                }

                if (truongPhong.MaPB != nhanVien.MaPB)
                {
                    return Forbid();
                }
            }
            return khenThuongKyLuat;
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<KhenThuongKyLuat>>> GetMyRecords()
        {
            var maNVClaim = User.FindFirst("MaNV")?.Value;

            if (maNVClaim == null)
            {
                return Unauthorized();
            }

            int maNV = int.Parse(maNVClaim);

            var danhSach = await _context.KhenThuongKyLuats
                .Where(x => x.MaNV == maNV)
                .OrderByDescending(x => x.NgayQuyetDinh)
                .ToListAsync();

            return Ok(danhSach);
        }

        [HttpPost]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự")]
        public async Task<ActionResult<KhenThuongKyLuat>> Create(
            KhenThuongKyLuat khenThuongKyLuat)
        {
            var nhanVienTonTai = await _context.NhanViens
                .AnyAsync(x => x.MaNV == khenThuongKyLuat.MaNV);

            if (!nhanVienTonTai)
            {
                return BadRequest(new
                {
                    message = "Nhân viên không tồn tại."
                });
            }

            if (khenThuongKyLuat.Loai != "Khen thưởng" &&
                khenThuongKyLuat.Loai != "Kỷ luật")
            {
                return BadRequest(new
                {
                    message = "Loại phải là 'Khen thưởng' hoặc 'Kỷ luật'."
                });
            }

            if (khenThuongKyLuat.SoTien < 0)
            {
                return BadRequest(new
                {
                    message = "Số tiền không được âm."
                });
            }

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
                return BadRequest(new
                {
                    message = "Mã khen thưởng/kỷ luật không hợp lệ."
                });
            }

            var banGhiCu = await _context.KhenThuongKyLuats
                .FirstOrDefaultAsync(x => x.MaKTKL == id);

            if (banGhiCu == null)
            {
                return NotFound();
            }

            var nhanVienTonTai = await _context.NhanViens
                .AnyAsync(x => x.MaNV == khenThuongKyLuat.MaNV);

            if (!nhanVienTonTai)
            {
                return BadRequest(new
                {
                    message = "Nhân viên không tồn tại."
                });
            }

            if (khenThuongKyLuat.Loai != "Khen thưởng" &&
                khenThuongKyLuat.Loai != "Kỷ luật")
            {
                return BadRequest(new
                {
                    message = "Loại phải là 'Khen thưởng' hoặc 'Kỷ luật'."
                });
            }

            if (khenThuongKyLuat.SoTien < 0)
            {
                return BadRequest(new
                {
                    message = "Số tiền không được âm."
                });
            }

            banGhiCu.MaNV = khenThuongKyLuat.MaNV;
            banGhiCu.Loai = khenThuongKyLuat.Loai;
            banGhiCu.NgayQuyetDinh = khenThuongKyLuat.NgayQuyetDinh;
            banGhiCu.SoTien = khenThuongKyLuat.SoTien;

            await _context.SaveChangesAsync();

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