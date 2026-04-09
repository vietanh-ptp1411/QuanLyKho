using Microsoft.EntityFrameworkCore;
using QuanLyKho.Models;

namespace QuanLyKho.Data;

public class AppDbContext : DbContext
{
    public DbSet<NhomVatTu> NhomVatTus => Set<NhomVatTu>();
    public DbSet<DonViTinh> DonViTinhs => Set<DonViTinh>();
    public DbSet<BoPhan> BoPhans => Set<BoPhan>();
    public DbSet<Kho> Khos => Set<Kho>();
    public DbSet<VatTu> VatTus => Set<VatTu>();
    public DbSet<PhieuNhapKho> PhieuNhapKhos => Set<PhieuNhapKho>();
    public DbSet<ChiTietPhieuNhap> ChiTietPhieuNhaps => Set<ChiTietPhieuNhap>();
    public DbSet<PhieuXuatKho> PhieuXuatKhos => Set<PhieuXuatKho>();
    public DbSet<ChiTietPhieuXuat> ChiTietPhieuXuats => Set<ChiTietPhieuXuat>();
    public DbSet<DeNghiCapVatTu> DeNghiCapVatTus => Set<DeNghiCapVatTu>();
    public DbSet<ChiTietDeNghi> ChiTietDeNghis => Set<ChiTietDeNghi>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Unique indexes
        modelBuilder.Entity<NhomVatTu>().HasIndex(x => x.MaNhom).IsUnique();
        modelBuilder.Entity<DonViTinh>().HasIndex(x => x.TenDonVi).IsUnique();
        modelBuilder.Entity<Kho>().HasIndex(x => x.MaKho).IsUnique();
        modelBuilder.Entity<VatTu>().HasIndex(x => x.MaVatTu).IsUnique();
        modelBuilder.Entity<PhieuNhapKho>().HasIndex(x => x.SoPhieu).IsUnique();
        modelBuilder.Entity<PhieuXuatKho>().HasIndex(x => x.SoPhieu).IsUnique();
        modelBuilder.Entity<DeNghiCapVatTu>().HasIndex(x => x.SoPhieu).IsUnique();

        // Cascade delete for line items
        modelBuilder.Entity<ChiTietPhieuNhap>()
            .HasOne(x => x.PhieuNhapKho)
            .WithMany(x => x.ChiTietPhieuNhaps)
            .HasForeignKey(x => x.PhieuNhapKhoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChiTietPhieuXuat>()
            .HasOne(x => x.PhieuXuatKho)
            .WithMany(x => x.ChiTietPhieuXuats)
            .HasForeignKey(x => x.PhieuXuatKhoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChiTietDeNghi>()
            .HasOne(x => x.DeNghiCapVatTu)
            .WithMany(x => x.ChiTietDeNghis)
            .HasForeignKey(x => x.DeNghiCapVatTuId)
            .OnDelete(DeleteBehavior.Cascade);

        // Decimal precision
        modelBuilder.Entity<ChiTietPhieuNhap>(e =>
        {
            e.Property(x => x.SoLuong).HasColumnType("decimal(18,3)");
            e.Property(x => x.DonGia).HasColumnType("decimal(18,2)");
            e.Property(x => x.ThanhTien).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<ChiTietPhieuXuat>(e =>
        {
            e.Property(x => x.SoLuong).HasColumnType("decimal(18,3)");
            e.Property(x => x.DonGia).HasColumnType("decimal(18,2)");
            e.Property(x => x.ThanhTien).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<ChiTietDeNghi>(e =>
        {
            e.Property(x => x.SoLuongYeuCau).HasColumnType("decimal(18,3)");
            e.Property(x => x.SoLuongDaCap).HasColumnType("decimal(18,3)");
        });

        modelBuilder.Entity<PhieuNhapKho>()
            .Property(x => x.TongTien).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<PhieuXuatKho>()
            .Property(x => x.TongTien).HasColumnType("decimal(18,2)");

        // Seed data
        modelBuilder.Entity<DonViTinh>().HasData(
            new DonViTinh { Id = 1, TenDonVi = "Cái" },
            new DonViTinh { Id = 2, TenDonVi = "Kg" },
            new DonViTinh { Id = 3, TenDonVi = "Bộ" },
            new DonViTinh { Id = 4, TenDonVi = "m" },
            new DonViTinh { Id = 5, TenDonVi = "Tấn" },
            new DonViTinh { Id = 6, TenDonVi = "Lít" },
            new DonViTinh { Id = 7, TenDonVi = "Chiếc" },
            new DonViTinh { Id = 8, TenDonVi = "Hộp" },
            new DonViTinh { Id = 9, TenDonVi = "Cuộn" },
            new DonViTinh { Id = 10, TenDonVi = "Thùng" }
        );

        modelBuilder.Entity<Kho>().HasData(
            new Kho { Id = 1, MaKho = "KT", TenKho = "Kho Tổng", DiaChi = "" },
            new Kho { Id = 2, MaKho = "KSC", TenKho = "Kho Sửa Chữa", DiaChi = "" }
        );

        modelBuilder.Entity<NhomVatTu>().HasData(
            new NhomVatTu { Id = 1, MaNhom = "VPP", TenNhom = "Văn phòng phẩm" },
            new NhomVatTu { Id = 2, MaNhom = "DC", TenNhom = "Dụng cụ" },
            new NhomVatTu { Id = 3, MaNhom = "NL", TenNhom = "Nguyên liệu" },
            new NhomVatTu { Id = 4, MaNhom = "PT", TenNhom = "Phụ tùng" }
        );
    }
}
