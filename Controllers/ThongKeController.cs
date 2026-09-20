using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyNhanSu.API.Services;

namespace QuanLyNhanSu.API.Controllers
{
    [Route("api/thong-ke")]
    [ApiController]
    public class ThongKeController : ControllerBase
    {
        private readonly ThongKeService _thongKeService;

        public ThongKeController(ThongKeService thongKeService)
        {
            _thongKeService = thongKeService;
        }

        [HttpGet("tong-quan")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Trưởng phòng,Ban giám đốc")]
        public async Task<IActionResult> GetOverview()
        {
            if (User.IsInRole("Trưởng phòng"))
            {
                var maNVClaim = User.FindFirst("MaNV")?.Value;

                if (maNVClaim == null)
                {
                    return Unauthorized();
                }

                int maNV = int.Parse(maNVClaim);

                var result = await _thongKeService
                    .GetOverviewByManagerAsync(maNV);

                return Ok(result);
            }

            var tongQuan = await _thongKeService
                .GetOverviewAsync();

            return Ok(tongQuan);
        }

        [HttpGet("quy-luong")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Ban giám đốc")]
        public async Task<IActionResult> GetPayrollByMonth(
            int thang,
            int nam)
        {
            if (thang < 1 || thang > 12)
            {
                return BadRequest(new
                {
                    message = "Tháng phải từ 1 đến 12."
                });
            }

            if (nam < 2000)
            {
                return BadRequest(new
                {
                    message = "Năm không hợp lệ."
                });
            }

            var result = await _thongKeService
                .GetPayrollByMonthAsync(thang, nam);

            return Ok(result);
        }

        [HttpGet("cham-cong")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Ban giám đốc")]
        public async Task<IActionResult> GetAttendanceByMonth(
            int thang,
            int nam)
        {
            if (thang < 1 || thang > 12)
            {
                return BadRequest(new
                {
                    message = "Tháng phải từ 1 đến 12."
                });
            }

            if (nam < 2000)
            {
                return BadRequest(new
                {
                    message = "Năm không hợp lệ."
                });
            }

            var result = await _thongKeService
                .GetAttendanceByMonthAsync(thang, nam);

            return Ok(result);
        }

        [HttpGet("nghi-phep")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Ban giám đốc")]
        public async Task<IActionResult> GetLeaveByMonth(
            int thang,
            int nam)
        {
            if (thang < 1 || thang > 12)
            {
                return BadRequest(new
                {
                    message = "Tháng phải từ 1 đến 12."
                });
            }

            if (nam < 2000)
            {
                return BadRequest(new
                {
                    message = "Năm không hợp lệ."
                });
            }

            var result = await _thongKeService
                .GetLeaveByMonthAsync(thang, nam);

            return Ok(result);
        }

        [HttpGet("nhan-vien-theo-phong-ban")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Ban giám đốc")]
        public async Task<IActionResult> GetEmployeesByDepartment()
        {
            var result = await _thongKeService
                .GetEmployeesByDepartmentAsync();

            return Ok(result);
        }

        [HttpGet("nhan-vien-theo-chuc-vu")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Ban giám đốc")]
        public async Task<IActionResult> GetEmployeesByPosition()
        {
            var result = await _thongKeService
                .GetEmployeesByPositionAsync();

            return Ok(result);
        }

        [HttpGet("nhan-vien-theo-trang-thai")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Ban giám đốc")]
        public async Task<IActionResult> GetEmployeesByStatus()
        {
            var result = await _thongKeService
                .GetEmployeesByStatusAsync();

            return Ok(result);
        }

        [HttpGet("hop-dong-sap-het-han")]
        [Authorize(Roles = "Quản trị viên,Nhân viên nhân sự,Kế toán,Ban giám đốc")]
        public async Task<IActionResult> GetExpiringContracts()
        {
            var result = await _thongKeService
                .GetExpiringContractsAsync();

            return Ok(result);
        }
    }
}