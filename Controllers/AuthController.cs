using Microsoft.AspNetCore.Mvc;
using QuanLyNhanSu.API.DTOs;
using QuanLyNhanSu.API.Services;

namespace QuanLyNhanSu.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var token = await _authService.LoginAsync(
                request.TenDangNhap,
                request.MatKhau
            );

            if (token == null)
            {
                return Unauthorized(new
                {
                    message = "Tên đăng nhập hoặc mật khẩu không đúng."
                });
            }

            return Ok(new
            {
                token
            });
        }
    }
}
