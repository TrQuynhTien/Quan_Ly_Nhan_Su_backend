using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.API.Data;
using QuanLyNhanSu.API.Models;

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
        public async Task<ActionResult> GetAll()
        {
            IQueryable<NhanVien> query = _context.NhanViens;

            if (User.IsInRole("Trưởng phòng"))
            {
                var maNVClaim = User.FindFirst("MaNV")?.Value;

                if (maNVClaim == null)
                    return Unauthorized();

                int maNV = int.Parse(maNVClaim);

                var truongPhong = await _context.NhanViens
                    .FirstOrDefaultAsync(x => x.MaNV == maNV);

                if (truongPhong == null)
                    return NotFound("Không tìm thấy thông tin trưởng phòng.");

                query = query.Where(x => x.MaPB == truongPhong.MaPB);
            }

            var danhSach = await (
                from nv in query

                join pb in _context.PhongBans
                    on nv.MaPB equals pb.MaPB into pbGroup
                from pb in pbGroup.DefaultIfEmpty()

                join cv in _context.ChucVus
                    on nv.MaCV equals cv.MaCV into cvGroup
                from cv in cvGroup.DefaultIfEmpty()

                join td in _context.TrinhDos
                    on nv.MaTD equals td.MaTD into tdGroup
                from td in tdGroup.DefaultIfEmpty()

                select new
                {
                    nv.MaNV,
                    nv.HoTen,
                    nv.GioiTinh,
                    nv.NgaySinh,
                    nv.CCCD,
                    nv.DiaChi,
                    nv.SDT,
                    nv.Email,
                    nv.NgayVaoLam,
                    nv.HinhAnh,

                    nv.MaPB,
                    TenPB = pb != null ? pb.TenPB : null,

                    nv.MaCV,
                    TenCV = cv != null ? cv.TenCV : null,

                    nv.MaTD,
                    TenTD = td != null ? td.TenTD : null,

                    nv.TrangThai
                }
            ).ToListAsync();

            return Ok(danhSach);
        }
        [HttpGet("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Trưởng phòng,Ban giám đốc")]
        public async Task<ActionResult> GetById(int id)
        {
            // Lấy nhân viên trước để kiểm tra tồn tại và phân quyền
            var nhanVien = await _context.NhanViens
                .FirstOrDefaultAsync(x => x.MaNV == id);

            if (nhanVien == null)
            {
                return NotFound();
            }

            // PHÂN QUYỀN CỦA TRƯỞNG PHÒNG
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

                // Trưởng phòng chỉ được xem nhân viên cùng phòng
                if (nhanVien.MaPB != truongPhong.MaPB)
                {
                    return Forbid();
                }
            }

            // Sau khi qua kiểm tra quyền mới lấy thêm tên phòng/chức vụ/trình độ
            var ketQua = await (
                from nv in _context.NhanViens

                join pb in _context.PhongBans
                    on nv.MaPB equals pb.MaPB into pbGroup
                from pb in pbGroup.DefaultIfEmpty()

                join cv in _context.ChucVus
                    on nv.MaCV equals cv.MaCV into cvGroup
                from cv in cvGroup.DefaultIfEmpty()

                join td in _context.TrinhDos
                    on nv.MaTD equals td.MaTD into tdGroup
                from td in tdGroup.DefaultIfEmpty()

                where nv.MaNV == id

                select new
                {
                    nv.MaNV,
                    nv.HoTen,
                    nv.GioiTinh,
                    nv.NgaySinh,
                    nv.CCCD,
                    nv.DiaChi,
                    nv.SDT,
                    nv.Email,
                    nv.NgayVaoLam,
                    nv.HinhAnh,

                    nv.MaPB,
                    TenPB = pb != null ? pb.TenPB : null,

                    nv.MaCV,
                    TenCV = cv != null ? cv.TenCV : null,

                    nv.MaTD,
                    TenTD = td != null ? td.TenTD : null,

                    nv.TrangThai
                }
            ).FirstOrDefaultAsync();

            return Ok(ketQua);
        }
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<NhanVien>> GetMyProfile()
        {
            var maNVClaim = User.FindFirst("MaNV")?.Value;

            if (maNVClaim == null)
            {
                return Unauthorized();
            }

            int maNV = int.Parse(maNVClaim);

            var nhanVien = await _context.NhanViens
                .FirstOrDefaultAsync(x => x.MaNV == maNV);

            if (nhanVien == null)
            {
                return NotFound();
            }

            return Ok(nhanVien);
        }


        [HttpPost]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự")]
        public async Task<ActionResult<NhanVien>> Create(NhanVien nhanVien)
        {
            if (string.IsNullOrWhiteSpace(nhanVien.CCCD))
            {
                return BadRequest(new
                {
                    message = "CCCD không được để trống."
                });
            }

            var cccdDaTonTai = await _context.NhanViens
                .AnyAsync(x => x.CCCD == nhanVien.CCCD);

            if (cccdDaTonTai)
            {
                return Conflict(new
                {
                    message = "CCCD đã tồn tại."
                });
            }

            try
            {
                _context.NhanViens.Add(nhanVien);
                await _context.SaveChangesAsync();

                return Ok(nhanVien);
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    message = "Đã xảy ra lỗi khi thêm nhân viên."
                    
                });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự")]
        public async Task<IActionResult> Update(int id, NhanVien nhanVien)
        {
            if (id != nhanVien.MaNV)
            {
                return BadRequest(new
                {
                    message = "Mã nhân viên không hợp lệ."
                });
            }

            var nhanVienCu = await _context.NhanViens
                .FirstOrDefaultAsync(x => x.MaNV == id);

            if (nhanVienCu == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy nhân viên."
                });
            }

            if (string.IsNullOrWhiteSpace(nhanVien.CCCD))
            {
                return BadRequest(new
                {
                    message = "CCCD không được để trống."
                });
            }

            var cccdDaTonTai = await _context.NhanViens
                .AnyAsync(x =>
                    x.CCCD == nhanVien.CCCD &&
                    x.MaNV != id);

            if (cccdDaTonTai)
            {
                return Conflict(new
                {
                    message = "CCCD đã tồn tại."
                });
            }

            nhanVienCu.HoTen = nhanVien.HoTen;
            nhanVienCu.GioiTinh = nhanVien.GioiTinh;
            nhanVienCu.NgaySinh = nhanVien.NgaySinh;
            nhanVienCu.CCCD = nhanVien.CCCD;
            nhanVienCu.DiaChi = nhanVien.DiaChi;
            nhanVienCu.SDT = nhanVien.SDT;
            nhanVienCu.Email = nhanVien.Email;
            nhanVienCu.NgayVaoLam = nhanVien.NgayVaoLam;
            nhanVienCu.HinhAnh = nhanVien.HinhAnh;
            nhanVienCu.MaPB = nhanVien.MaPB;
            nhanVienCu.MaCV = nhanVien.MaCV;
            nhanVienCu.MaTD = nhanVien.MaTD;
            nhanVienCu.TrangThai = nhanVien.TrangThai;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Cập nhật nhân viên thành công."
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự")]
        public async Task<IActionResult> Delete(int id)
        {
            var nhanVien = await _context.NhanViens.FindAsync(id);

            if (nhanVien == null)
                return NotFound();

            var coTaiKhoan = await _context.TaiKhoans
                .AnyAsync(x => x.MaNV == id);

            if (coTaiKhoan)
            {
                return Conflict(
                    "Không thể xóa nhân viên vì nhân viên đang có tài khoản."
                );
            }

            _context.NhanViens.Remove(nhanVien);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}