using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.API.Data;
using QuanLyNhanSu.API.DTOs;
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

        public NghiPhepController(
            QuanLyNhanSuDbContext context,
            NghiPhepService nghiPhepService)
        {
            _context = context;
            _nghiPhepService = nghiPhepService;
        }

        [HttpGet]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự, Kế toán, Trưởng phòng,Ban giám đốc")]
        public async Task<ActionResult<IEnumerable<NghiPhep>>> GetAll()
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

                var danhSach = await _context.NghiPheps
                    .Join(
                        _context.NhanViens,
                        np => np.MaNV,
                        nv => nv.MaNV,
                        (np, nv) => new
                        {
                            NghiPhep = np,
                            MaPB = nv.MaPB
                        }
                    )
                    .Where(x => x.MaPB == truongPhong.MaPB)
                    .Select(x => x.NghiPhep)
                    .ToListAsync();

                return Ok(danhSach);
            }

            return Ok(await _context.NghiPheps.ToListAsync());
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<NghiPhep>>> GetMyLeaveRequests()
        {
            var maNVClaim = User.FindFirst("MaNV")?.Value;

            if (maNVClaim == null)
            {
                return Unauthorized();
            }

            int maNV = int.Parse(maNVClaim);

            var danhSach = await _context.NghiPheps
                .Where(x => x.MaNV == maNV)
                .ToListAsync();

            return Ok(danhSach);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Trưởng phòng,Ban giám đốc,Nhân viên")]
        public async Task<ActionResult<NghiPhep>> GetById(int id)
        {
            var nghiPhep = await _context.NghiPheps.FindAsync(id);

            if (nghiPhep == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Nhân viên"))
            {
                var maNVClaim = User.FindFirst("MaNV")?.Value;

                if (maNVClaim == null)
                {
                    return Unauthorized();
                }

                int maNV = int.Parse(maNVClaim);

                if (nghiPhep.MaNV != maNV)
                {
                    return Forbid();
                }
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
                    .FirstOrDefaultAsync(x => x.MaNV == nghiPhep.MaNV);

                if (truongPhong == null || nhanVien == null)
                {
                    return NotFound();
                }

                if (truongPhong.MaPB != nhanVien.MaPB)
                {
                    return Forbid();
                }
            }

            return Ok(nghiPhep);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<NghiPhep>> Create(
            CreateNghiPhepRequest request)
        {
            var maNVClaim = User.FindFirst("MaNV")?.Value;

            if (maNVClaim == null)
            {
                return Unauthorized();
            }

            int maNV = int.Parse(maNVClaim);

            // Kiểm tra ngày nghỉ
            if (request.DenNgay < request.TuNgay)
            {
                return BadRequest(new
                {
                    message = "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu."
                });
            }

            // Kiểm tra loại nghỉ phép
            var loaiNghiPhepTonTai = await _context.LoaiNghiPheps
                .AnyAsync(x => x.MaLoaiNP == request.MaLoaiNP);

            if (!loaiNghiPhepTonTai)
            {
                return BadRequest(new
                {
                    message = "Loại nghỉ phép không tồn tại."
                });
            }

            // Lấy thông tin nhân viên gửi đơn
            var nhanVien = await _context.NhanViens
                .FirstOrDefaultAsync(x => x.MaNV == maNV);

            if (nhanVien == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy thông tin nhân viên."
                });
            }

            // Tạo đơn nghỉ phép
            var nghiPhep = new NghiPhep
            {
                MaNV = maNV,
                MaLoaiNP = request.MaLoaiNP,
                TuNgay = request.TuNgay,
                DenNgay = request.DenNgay,
                LyDo = request.LyDo,
                TrangThai = "Chờ duyệt",
                NguoiDuyet = null
            };

        _context.NghiPheps.Add(nghiPhep);
        await _context.SaveChangesAsync();

        // Tìm trưởng phòng cùng phòng ban với nhân viên
        var truongPhong = await (
            from tk in _context.TaiKhoans
            join q in _context.Quyens
                on tk.MaQuyen equals q.MaQuyen
            join nv in _context.NhanViens
                on tk.MaNV equals nv.MaNV
            where q.TenQuyen == "Trưởng phòng"
                && nv.MaPB == nhanVien.MaPB
                && tk.TrangThai == "Hoạt động"
            select nv
        ).FirstOrDefaultAsync();

        if (truongPhong != null)
        {
            var thongBao = new ThongBao
            {
                MaNV = truongPhong.MaNV,
                TieuDe = "Có đơn nghỉ phép mới",
                NoiDung = $"{nhanVien.HoTen} vừa gửi một đơn nghỉ phép cần duyệt.",
                DaDoc = false,
                NgayTao = DateTime.Now,
                DuongDan = $"/leave/{nghiPhep.MaNP}"
            };

            _context.ThongBaos.Add(thongBao);
            await _context.SaveChangesAsync();
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = nghiPhep.MaNP },
            nghiPhep
        );  
        }
        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Trưởng phòng")]
        public async Task<IActionResult> Approve(int id)
        {
            var maNVClaim = User.FindFirst("MaNV")?.Value;

            if (maNVClaim == null)
            {
                return Unauthorized();
            }

            int nguoiDuyet = int.Parse(maNVClaim);
            if (User.IsInRole("Trưởng phòng"))
            {
                var nghiPhep = await _context.NghiPheps
                    .FirstOrDefaultAsync(x => x.MaNP == id);

                if (nghiPhep == null)
                {
                    return NotFound("Không tìm thấy đơn nghỉ phép");
                }

                var truongPhong = await _context.NhanViens
                    .FirstOrDefaultAsync(x => x.MaNV == nguoiDuyet);

                var nhanVien = await _context.NhanViens
                    .FirstOrDefaultAsync(x => x.MaNV == nghiPhep.MaNV);

                if (truongPhong == null || nhanVien == null)
                {
                    return NotFound();
                }

                if (truongPhong.MaPB != nhanVien.MaPB)
                {
                    return Forbid();
                }
            }

            var result = await _nghiPhepService
                .ApproveLeaveRequestAsync(id, nguoiDuyet);

            if (result == "NOT_FOUND")
            {
                return NotFound("Không tìm thấy đơn nghỉ phép");
            }

            if (result == "ALREADY_PROCESSED")
            {
                return Conflict("Đơn nghỉ phép đã được xử lý");
            }

            return NoContent();
        }

        [HttpPut("{id}/reject")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Trưởng phòng")]
        public async Task<IActionResult> Reject(int id)
        {
            var maNVClaim = User.FindFirst("MaNV")?.Value;

            if (maNVClaim == null)
            {
                return Unauthorized();
            }

            int nguoiDuyet = int.Parse(maNVClaim);
            if (User.IsInRole("Trưởng phòng"))
            {
                var nghiPhep = await _context.NghiPheps
                    .FirstOrDefaultAsync(x => x.MaNP == id);

                if (nghiPhep == null)
                {
                    return NotFound("Không tìm thấy đơn nghỉ phép");
                }

                var truongPhong = await _context.NhanViens
                    .FirstOrDefaultAsync(x => x.MaNV == nguoiDuyet);

                var nhanVien = await _context.NhanViens
                    .FirstOrDefaultAsync(x => x.MaNV == nghiPhep.MaNV);

                if (truongPhong == null || nhanVien == null)
                {
                    return NotFound();
                }

                if (truongPhong.MaPB != nhanVien.MaPB)
                {
                    return Forbid();
                }
            }

            var result = await _nghiPhepService
                .RejectLeaveRequestAsync(id, nguoiDuyet);

            if (result == "NOT_FOUND")
            {
                return NotFound("Không tìm thấy đơn nghỉ phép");
            }

            if (result == "ALREADY_PROCESSED")
            {
                return Conflict("Đơn nghỉ phép đã được xử lý");
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự")]
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