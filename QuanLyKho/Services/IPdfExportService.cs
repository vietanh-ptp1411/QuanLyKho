using QuanLyKho.Data;
using QuanLyKho.Models;

namespace QuanLyKho.Services;

public interface IPdfExportService
{
    Task ExportPhieuNhapKho(PhieuNhapKho phieu, string filePath);
    Task ExportPhieuXuatKho(PhieuXuatKho phieu, string filePath);
    Task ExportDeNghiCapVatTu(DeNghiCapVatTu phieu, string filePath);
    Task ExportNhapXuatTonKho(AppDbContext context, string filePath, int khoId, int month, int year);
    Task ExportTonKho(AppDbContext context, string filePath);
    Task ExportDanhSachNhapKho(AppDbContext context, string filePath, DateTime from, DateTime to);
    Task ExportDanhSachXuatKho(AppDbContext context, string filePath, DateTime from, DateTime to);
}
