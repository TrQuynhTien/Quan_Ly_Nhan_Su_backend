using QuanLyNhanSu.API.Data;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace QuanLyNhanSu.API.Services
{
    public class AuthService
    {
        private readonly QuanLyNhanSuDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(
            QuanLyNhanSuDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        public async Task<string?> LoginAsync(string tenDangNhap, string matKhau)
        {
            var taiKhoan = await _context.TaiKhoans
                .FirstOrDefaultAsync(x =>
                    x.TenDangNhap == tenDangNhap);

            if (taiKhoan == null ||
                !BCrypt.Net.BCrypt.Verify(matKhau, taiKhoan.MatKhau))
            {
                return null;
            }

            var quyen = await _context.Quyens
                .FirstOrDefaultAsync(x => x.MaQuyen == taiKhoan.MaQuyen);

            if (quyen == null)
            {
                return null;
            }

            return CreateToken(
                taiKhoan.TenDangNhap,
                quyen.TenQuyen,
                taiKhoan.MaNV
            );
        }
        private string CreateToken(
            string tenDangNhap,
            string tenQuyen,
            int maNV)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, tenDangNhap),
                new Claim(ClaimTypes.Role, tenQuyen),
                new Claim("MaNV", maNV.ToString())
            };

            var key = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(
                    _configuration["Jwt:Key"]!
                )
            );

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}