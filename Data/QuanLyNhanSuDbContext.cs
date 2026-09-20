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
        public DbSet<TrinhDo> TrinhDos { get; set; } = null!;
        public DbSet<LoaiHopDong> LoaiHopDongs { get; set; } = null!;
        public DbSet<NhanVien> NhanViens { get; set; } = null!;
        public DbSet<HopDong> HopDongs { get; set; } = null!;
        public DbSet<LoaiCa> LoaiCas { get; set; } = null!;
        public DbSet<ChamCong> ChamCongs { get; set; } = null!;
        public DbSet<LoaiNghiPhep> LoaiNghiPheps { get; set; } = null!;
        public DbSet<NghiPhep> NghiPheps { get; set; } = null!;
        public DbSet<PhuCap> PhuCaps { get; set; } = null!;
        public DbSet<NhanVienPhuCap> NhanVienPhuCaps { get; set; } = null!;
        public DbSet<BangLuong> BangLuongs { get; set; } = null!;
        public DbSet<KhenThuongKyLuat> KhenThuongKyLuats { get; set; } = null!;
        public DbSet<Quyen> Quyens { get; set; } = null!;
        public DbSet<TaiKhoan> TaiKhoans { get; set; } = null!;
        public DbSet<ThongBao> ThongBaos { get; set; } = null!;
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<NhanVienPhuCap>()
                .HasKey(x => new { x.MaNV, x.MaPC });
            modelBuilder.Entity<BangLuong>()
                .HasIndex(x => new { x.MaNV, x.Thang, x.Nam })
                .IsUnique();
            modelBuilder.Entity<TaiKhoan>()
                .HasIndex(x => x.MaNV)
                .IsUnique();
            modelBuilder.Entity<NghiPhep>()
                .HasOne<NhanVien>()
                .WithMany()
                .HasForeignKey(x => x.NguoiDuyet)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<NhanVien>()
                .HasOne<PhongBan>()
                .WithMany()
                .HasForeignKey(x => x.MaPB)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NhanVien>()
                .HasOne<ChucVu>()
                .WithMany()
                .HasForeignKey(x => x.MaCV)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NhanVien>()
                .HasOne<TrinhDo>()
                .WithMany()
                .HasForeignKey(x => x.MaTD)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<HopDong>()
                .HasOne<NhanVien>()
                .WithMany()
                .HasForeignKey(x => x.MaNV)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HopDong>()
                .HasOne<LoaiHopDong>()
                .WithMany()
                .HasForeignKey(x => x.MaLoaiHD)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ChamCong>()
                .HasOne<NhanVien>()
                .WithMany()
                .HasForeignKey(x => x.MaNV)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChamCong>()
                .HasOne<LoaiCa>()
                .WithMany()
                .HasForeignKey(x => x.MaCa)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<NghiPhep>()
                .HasOne<NhanVien>()
                .WithMany()
                .HasForeignKey(x => x.MaNV)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NghiPhep>()
                .HasOne<LoaiNghiPhep>()
                .WithMany()
                .HasForeignKey(x => x.MaLoaiNP)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<NhanVienPhuCap>()
                .HasOne<NhanVien>()
                .WithMany()
                .HasForeignKey(x => x.MaNV)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NhanVienPhuCap>()
                .HasOne<PhuCap>()
                .WithMany()
                .HasForeignKey(x => x.MaPC)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<BangLuong>()
                .HasOne<NhanVien>()
                .WithMany()
                .HasForeignKey(x => x.MaNV)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<KhenThuongKyLuat>()
                .HasOne<NhanVien>()
                .WithMany()
                .HasForeignKey(x => x.MaNV)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<TaiKhoan>()
                .HasOne<NhanVien>()
                .WithMany()
                .HasForeignKey(x => x.MaNV)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaiKhoan>()
                .HasOne<Quyen>()
                .WithMany()
                .HasForeignKey(x => x.MaQuyen)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<BangLuong>()
                .Property(x => x.LuongCoBan)
                .HasPrecision(18, 2);

            modelBuilder.Entity<BangLuong>()
                .Property(x => x.TongPhuCap)
                .HasPrecision(18, 2);

            modelBuilder.Entity<BangLuong>()
                .Property(x => x.TongThuong)
                .HasPrecision(18, 2);

            modelBuilder.Entity<BangLuong>()
                .Property(x => x.TongKhauTru)
                .HasPrecision(18, 2);

            modelBuilder.Entity<BangLuong>()
                .Property(x => x.TongLuong)
                .HasPrecision(18, 2);

            modelBuilder.Entity<BangLuong>()
                .Property(x => x.SoNgayCong)
                .HasPrecision(5, 2);

            modelBuilder.Entity<ChamCong>()
                .Property(x => x.SoGioLam)
                .HasPrecision(5, 2);

            modelBuilder.Entity<HopDong>()
                .Property(x => x.LuongCoBan)
                .HasPrecision(18, 2);

            modelBuilder.Entity<KhenThuongKyLuat>()
                .Property(x => x.SoTien)
                .HasPrecision(18, 2);

            modelBuilder.Entity<LoaiCa>()
                .Property(x => x.SoGioQuyDinh)
                .HasPrecision(5, 2);

            modelBuilder.Entity<PhuCap>()
                .Property(x => x.SoTien)
                .HasPrecision(18, 2);
        }
    }
}