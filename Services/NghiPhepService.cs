using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.API.Data;

namespace QuanLyNhanSu.API.Services
{
    public class NghiPhepService
    {
        private readonly QuanLyNhanSuDbContext _context;

        public NghiPhepService(QuanLyNhanSuDbContext context)
        {
            _context = context;
        }
        public async Task<string> ApproveLeaveRequestAsync(int maNP, int nguoiDuyet)
        {
            var nghiPhep = await _context.NghiPheps
                .FirstOrDefaultAsync(x => x.MaNP == maNP);

            if (nghiPhep == null)
            {
                return "NOT_FOUND";
            }

            if (nghiPhep.TrangThai != "Chờ duyệt")
            {
                return "ALREADY_PROCESSED";
            }

            nghiPhep.TrangThai = "Đã duyệt";
            nghiPhep.NguoiDuyet = nguoiDuyet;

            await _context.SaveChangesAsync();

            return "SUCCESS";
        }
        public async Task<string> RejectLeaveRequestAsync(int maNP, int nguoiDuyet)
        {
            var nghiPhep = await _context.NghiPheps
                .FirstOrDefaultAsync(x => x.MaNP == maNP);

            if (nghiPhep == null)
            {
                return "NOT_FOUND";
            }

            if (nghiPhep.TrangThai != "Chờ duyệt")
            {
                return "ALREADY_PROCESSED";
            }

            nghiPhep.TrangThai = "Từ chối";
            nghiPhep.NguoiDuyet = nguoiDuyet;

            await _context.SaveChangesAsync();

            return "SUCCESS";
        }
    }
}