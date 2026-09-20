using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using QuanLyNhanSu.API.DTOs;

namespace QuanLyNhanSu.API.Controllers
{
    [Route("api/phan-quyens")]
    [ApiController]
    [Authorize]
    public class PhanQuyenController : ControllerBase
    {
        [HttpGet("me")]
        public IActionResult GetMyPermissions()
        {
            var tenQuyen = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrWhiteSpace(tenQuyen))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Không xác định được quyền của người dùng."
                });
            }

            var response = TaoPhanQuyen(tenQuyen);

            if (response == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy cấu hình phân quyền."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Lấy phân quyền thành công.",
                data = response
            });
        }

        private PhanQuyenResponse? TaoPhanQuyen(string tenQuyen)
        {
            return tenQuyen switch
            {
                "Quản trị viên" => QuyenQuanTriVien(),
                "Nhân viên nhân sự" => QuyenNhanSu(),
                "Kế toán" => QuyenKeToan(),
                "Trưởng phòng" => QuyenTruongPhong(),
                "Ban giám đốc" => QuyenBanGiamDoc(),
                "Nhân viên" => QuyenNhanVien(),
                _ => null
            };
        }

        private PhanQuyenResponse QuyenQuanTriVien()
        {
            return new PhanQuyenResponse
            {
                MaQuyen = 1,
                TenQuyen = "Quản trị viên",
                QuyenHan = new List<QuyenChucNangDto>
                {
                    Q("NHAN_VIEN", true, true, true, true),
                    Q("PHONG_BAN", true, true, true, true),
                    Q("CHUC_VU", true, true, true, true),
                    Q("TRINH_DO", true, true, true, true),
                    Q("HOP_DONG", true, true, true, true),
                    Q("CHAM_CONG", true, true, true, true),
                    Q("NGHI_PHEP", true, true, false, true, true),
                    Q("BANG_LUONG", true, false, false, true),
                    Q("KHEN_THUONG_KY_LUAT", true, true, true, true),
                    Q("PHU_CAP", true, true, true, true),
                    Q("TAI_KHOAN", true, true, true, true),
                    Q("QUYEN", true, false, false, false),
                    Q("BAO_CAO", true, false, false, false),
                    Q("AI", true, false, false, false),
                    Q("THONG_BAO", true, false, true, false)
                }
            };
        }

        private PhanQuyenResponse QuyenNhanSu()
        {
            return new PhanQuyenResponse
            {
                MaQuyen = 2,
                TenQuyen = "Nhân viên nhân sự",
                QuyenHan = new List<QuyenChucNangDto>
                {
                    Q("NHAN_VIEN", true, true, true, true),
                    Q("PHONG_BAN", true, true, true, true),
                    Q("CHUC_VU", true, true, true, true),
                    Q("TRINH_DO", true, true, true, true),
                    Q("HOP_DONG", true, true, true, true),
                    Q("CHAM_CONG", true, true, true, true),
                    Q("NGHI_PHEP", true, true, false, true, true),
                    Q("BANG_LUONG", true, false, false, false),
                    Q("KHEN_THUONG_KY_LUAT", true, true, true, true),
                    Q("PHU_CAP", true, true, true, true),
                    Q("TAI_KHOAN", false, false, false, false),
                    Q("QUYEN", false, false, false, false),
                    Q("BAO_CAO", true, false, false, false),
                    Q("AI", true, false, false, false),
                    Q("THONG_BAO", true, false, true, false)
                }
            };
        }

        private PhanQuyenResponse QuyenKeToan()
        {
            return new PhanQuyenResponse
            {
                MaQuyen = 3,
                TenQuyen = "Kế toán",
                QuyenHan = new List<QuyenChucNangDto>
                {
                    Q("NHAN_VIEN", true, false, false, false),
                    Q("PHONG_BAN", true, false, false, false),
                    Q("CHUC_VU", true, false, false, false),
                    Q("TRINH_DO", true, false, false, false),
                    Q("HOP_DONG", true, false, false, false),
                    Q("CHAM_CONG", true, true, true, true),
                    Q("NGHI_PHEP", false, true, false, false),
                    Q("BANG_LUONG", true, false, false, true),
                    Q("KHEN_THUONG_KY_LUAT", true, false, false, false),
                    Q("PHU_CAP", true, true, true, true),
                    Q("TAI_KHOAN", false, false, false, false),
                    Q("QUYEN", false, false, false, false),
                    Q("BAO_CAO", true, false, false, false),
                    Q("AI", true, false, false, false),
                    Q("THONG_BAO", true, false, true, false)
                }
            };
        }

        private PhanQuyenResponse QuyenTruongPhong()
        {
            return new PhanQuyenResponse
            {
                MaQuyen = 4,
                TenQuyen = "Trưởng phòng",
                QuyenHan = new List<QuyenChucNangDto>
                {
                    Q("NHAN_VIEN", true, false, false, false),
                    Q("PHONG_BAN", true, false, false, false),
                    Q("CHUC_VU", true, false, false, false),
                    Q("TRINH_DO", true, false, false, false),
                    Q("HOP_DONG", true, false, false, false),
                    Q("CHAM_CONG", true, false, false, false),
                    Q("NGHI_PHEP", true, true, false, false, true),
                    Q("BANG_LUONG", false, false, false, false),
                    Q("KHEN_THUONG_KY_LUAT", true, false, false, false),
                    Q("PHU_CAP", true, false, false, false),
                    Q("TAI_KHOAN", false, false, false, false),
                    Q("QUYEN", false, false, false, false),
                    Q("BAO_CAO", false, false, false, false),
                    Q("AI", true, false, false, false),
                    Q("THONG_BAO", true, false, true, false)
                }
            };
        }

        private PhanQuyenResponse QuyenBanGiamDoc()
        {
            return new PhanQuyenResponse
            {
                MaQuyen = 5,
                TenQuyen = "Ban giám đốc",
                QuyenHan = new List<QuyenChucNangDto>
                {
                    Q("NHAN_VIEN", true, false, false, false),
                    Q("PHONG_BAN", true, false, false, false),
                    Q("CHUC_VU", true, false, false, false),
                    Q("TRINH_DO", true, false, false, false),
                    Q("HOP_DONG", true, false, false, false),
                    Q("CHAM_CONG", true, false, false, false),
                    Q("NGHI_PHEP", true, false, false, false, false),
                    Q("BANG_LUONG", true, false, false, false),
                    Q("KHEN_THUONG_KY_LUAT", true, false, false, false),
                    Q("PHU_CAP", true, false, false, false),
                    Q("TAI_KHOAN", false, false, false, false),
                    Q("QUYEN", false, false, false, false),
                    Q("BAO_CAO", true, false, false, false),
                    Q("AI", true, false, false, false),
                    Q("THONG_BAO", true, false, true, false)
                }
            };
        }

        private PhanQuyenResponse QuyenNhanVien()
        {
            return new PhanQuyenResponse
            {
                MaQuyen = 6,
                TenQuyen = "Nhân viên",
                QuyenHan = new List<QuyenChucNangDto>
                {
                    Q("NHAN_VIEN", false, false, false, false),
                    Q("PHONG_BAN", false, false, false, false),
                    Q("CHUC_VU", false, false, false, false),
                    Q("TRINH_DO", false, false, false, false),
                    Q("HOP_DONG", false, false, false, false),
                    Q("CHAM_CONG", false, false, false, false),
                    Q("NGHI_PHEP", false, true, false, false),
                    Q("BANG_LUONG", false, false, false, false),
                    Q("KHEN_THUONG_KY_LUAT", false, false, false, false),
                    Q("PHU_CAP", false, false, false, false),
                    Q("TAI_KHOAN", false, false, false, false),
                    Q("QUYEN", false, false, false, false),
                    Q("BAO_CAO", false, false, false, false),
                    Q("AI", false, false, false, false),
                    Q("THONG_BAO", true, false, true, false)
                }
            };
        }

        private QuyenChucNangDto Q(
            string chucNang,
            bool xem,
            bool them,
            bool sua,
            bool xoa,
            bool duyet = false)
        {
            return new QuyenChucNangDto
            {
                ChucNang = chucNang,
                Xem = xem,
                Them = them,
                Sua = sua,
                Xoa = xoa,
                Duyet = duyet
            };
        }
    }
}