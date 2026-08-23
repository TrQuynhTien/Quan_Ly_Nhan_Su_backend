using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.API.Data;
using QuanLyNhanSu.API.Models;
using QuanLyNhanSu.API.Services;

namespace QuanLyNhanSu.API.Services
{
    public class NghiPhepService
    {
        private readonly QuanLyNhanSuDbContext _context;

        public NghiPhepService(QuanLyNhanSuDbContext context)
        {
            _context = context;
        }
        public async Task<bool> ApproveLeaveRequestAsync(int maNP, int nguoiDuyet)
        {
            var nghiPhep = await _context.NghiPheps
                .FirstOrDefaultAsync(x => x.MaNP == maNP);

            if (nghiPhep == null)
            {
                return false;
            }

            nghiPhep.TrangThai = "Đã duyệt";
            nghiPhep.NguoiDuyet = nguoiDuyet;

            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> RejectLeaveRequestAsync(int maNP, int nguoiDuyet)
        {
            var nghiPhep = await _context.NghiPheps
                .FirstOrDefaultAsync(x => x.MaNP == maNP);

            if (nghiPhep == null)
            {
                return false;
            }

            nghiPhep.TrangThai = "Từ chối";
            nghiPhep.NguoiDuyet = nguoiDuyet;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}