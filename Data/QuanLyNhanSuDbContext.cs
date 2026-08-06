using Microsoft.EntityFrameworkCore;
using QuanLyNhanSu.API.Models;
namespace QuanLyNhanSu.API.Data
{
    public class QuanLyNhanSuDbContext : DbContext
    {
        public QuanLyNhanSuDbContext(DbContextOptions<QuanLyNhanSuDbContext> options)
            : base(options)
        {

        }
        public DbSet<PhongBan> PhongBans {get; set;} = null!;
        public DbSet<ChucVu> ChucVus { get; set; } = null!;
        public DbSet<TrinhDo> TrinhDos { get; set; }
        public DbSet<LoaiHopDong> LoaiHopDongs { get; set; }
    }
}