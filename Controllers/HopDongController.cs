using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.API.Data;
using QuanLyNhanSu.API.Models;

namespace QuanLyNhanSu.API.Controllers
{
    [Route("api/hop-dongs")]
    [ApiController]
    public class HopDongController : ControllerBase
    {
        private readonly QuanLyNhanSuDbContext _context;

        public HopDongController(QuanLyNhanSuDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Trưởng phòng,Ban giám đốc")]
        public async Task<ActionResult<IEnumerable<HopDong>>> GetAll()
        {
            IQueryable<HopDong> query = _context.HopDongs;

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

                query =
                    from hd in query
                    join nv in _context.NhanViens
                        on hd.MaNV equals nv.MaNV
                    where nv.MaPB == truongPhong.MaPB
                    select hd;
            }

            var hopDongs = await query
                .OrderByDescending(x => x.NgayBatDau)
                .ToListAsync();

            foreach (var hopDong in hopDongs)
            {
                CapNhatTrangThai(hopDong);
            }

            await _context.SaveChangesAsync();

            return Ok(hopDongs);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<HopDong>>> GetMyContracts()
        {
            var maNVClaim = User.FindFirst("MaNV")?.Value;

            if (maNVClaim == null)
            {
                return Unauthorized();
            }

            int maNV = int.Parse(maNVClaim);

            var hopDongs = await _context.HopDongs
                .Where(x => x.MaNV == maNV)
                .OrderByDescending(x => x.NgayBatDau)
                .ToListAsync();

            foreach (var hopDong in hopDongs)
            {
                CapNhatTrangThai(hopDong);
            }

            await _context.SaveChangesAsync();

            return Ok(hopDongs);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Trưởng phòng,Ban giám đốc")]
        public async Task<ActionResult<HopDong>> GetById(int id)
        {
            var hopDong = await _context.HopDongs.FindAsync(id);

            if (hopDong == null)
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
                    .FirstOrDefaultAsync(x => x.MaNV == hopDong.MaNV);

                if (truongPhong == null || nhanVien == null)
                {
                    return NotFound();
                }

                if (truongPhong.MaPB != nhanVien.MaPB)
                {
                    return Forbid();
                }
            }

            CapNhatTrangThai(hopDong);
            await _context.SaveChangesAsync();

            return Ok(hopDong);
        }

        [HttpPost]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự")]
        public async Task<ActionResult<HopDong>> Create(HopDong hopDong)
        {
            if (hopDong.NgayKetThuc != null &&
                hopDong.NgayKetThuc.Value.Date < hopDong.NgayBatDau.Date)
            {
                return BadRequest(
                    "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu."
                );
            }

            var nhanVienTonTai = await _context.NhanViens
                .AnyAsync(x => x.MaNV == hopDong.MaNV);

            if (!nhanVienTonTai)
            {
                return BadRequest("Nhân viên không tồn tại.");
            }

            var trungThoiGian = await _context.HopDongs
                .AnyAsync(x =>
                    x.MaNV == hopDong.MaNV &&
                    (x.NgayKetThuc == null ||
                     x.NgayKetThuc.Value.Date >= hopDong.NgayBatDau.Date) &&
                    (hopDong.NgayKetThuc == null ||
                     x.NgayBatDau.Date <= hopDong.NgayKetThuc.Value.Date)
                );

            if (trungThoiGian)
            {
                return Conflict(
                    "Nhân viên đang có hợp đồng trùng thời gian. " +
                    "Hãy kết thúc hợp đồng hiện tại trước khi tạo hợp đồng mới."
                );
            }
        

            CapNhatTrangThai(hopDong);

            _context.HopDongs.Add(hopDong);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = hopDong.MaHD },
                hopDong
            );
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự")]
        public async Task<IActionResult> Update(
            int id,
            HopDong hopDong)
        {
            if (id != hopDong.MaHD)
            {
                return BadRequest();
            }

            if (hopDong.NgayKetThuc != null &&
                hopDong.NgayKetThuc.Value.Date < hopDong.NgayBatDau.Date)
            {
                return BadRequest(
                    "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu."
                );
            }

            var hopDongCu = await _context.HopDongs
                .FirstOrDefaultAsync(x => x.MaHD == id);

            if (hopDongCu == null)
            {
                return NotFound();
            }

            var trungThoiGian = await _context.HopDongs
                .AnyAsync(x =>
                    x.MaHD != id &&
                    x.MaNV == hopDong.MaNV &&
                    (x.NgayKetThuc == null ||
                     x.NgayKetThuc.Value.Date >= hopDong.NgayBatDau.Date) &&
                    (hopDong.NgayKetThuc == null ||
                     x.NgayBatDau.Date <= hopDong.NgayKetThuc.Value.Date)
                );

            if (trungThoiGian)
            {
                return Conflict(
                    "Thời gian hợp đồng bị trùng với hợp đồng khác của nhân viên."
                );
            }
            hopDongCu.MaNV = hopDong.MaNV;
            hopDongCu.MaLoaiHD = hopDong.MaLoaiHD;
            hopDongCu.NgayBatDau = hopDong.NgayBatDau;
            hopDongCu.NgayKetThuc = hopDong.NgayKetThuc;
            hopDongCu.LuongCoBan = hopDong.LuongCoBan;

            CapNhatTrangThai(hopDongCu);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự")]
        public async Task<IActionResult> Delete(int id)
        {
            var hopDong = await _context.HopDongs.FindAsync(id);

            if (hopDong == null)
            {
                return NotFound();
            }

            _context.HopDongs.Remove(hopDong);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private void CapNhatTrangThai(HopDong hopDong)
        {
            if (hopDong.NgayKetThuc == null ||
                hopDong.NgayKetThuc.Value.Date >= DateTime.Today)
            {
                hopDong.TrangThai = "Còn hiệu lực";
            }
            else
            {
                hopDong.TrangThai = "Hết hiệu lực";
            }
        }
    }
}