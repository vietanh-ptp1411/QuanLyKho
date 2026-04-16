using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuanLyKho.Data;
using QuanLyKho.Models;

namespace QuanLyKho.Services;

public class ExcelExportService : IExcelExportService
{
    public Task ExportPhieuNhapKho(PhieuNhapKho phieu, string filePath)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Phiếu Nhập Kho");

        ws.Column(1).Width = 5;
        ws.Column(2).Width = 25;
        ws.Column(3).Width = 12;
        ws.Column(4).Width = 8;
        ws.Column(5).Width = 14;
        ws.Column(6).Width = 18;

        int row = 1;

        // Header
        ws.Cell(row, 1).Value = "CÔNG TY CỔ PHẦN TẬP ĐOÀN SÔNG HỒNG THỦ ĐÔ";
        ws.Range(row, 1, row, 4).Merge();
        ws.Cell(row, 1).Style.Font.SetBold().Font.SetItalic().Font.FontSize = 10;

        ws.Cell(row, 5).Value = "SÔNG HỒNG THỦ ĐÔ";
        ws.Range(row, 5, row, 6).Merge();
        ws.Cell(row, 5).Style.Font.SetBold().Font.FontSize = 11;
        ws.Cell(row, 5).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

        row++;
        ws.Cell(row, 1).Value = "Khu đô thị Bắc Đầm Vạc";
        ws.Range(row, 1, row, 4).Merge();
        ws.Cell(row, 1).Style.Font.SetItalic().Font.FontSize = 9;

        ws.Cell(row, 5).Value = $"Số HĐ: {phieu.SoHopDong}";
        ws.Range(row, 5, row, 6).Merge();
        ws.Cell(row, 5).Style.Font.SetBold().Font.FontSize = 10;
        ws.Cell(row, 5).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

        row++;
        ws.Cell(row, 1).Value = $"Bộ phận: {phieu.BoPhan?.TenBoPhan ?? ""}";
        ws.Range(row, 1, row, 4).Merge();
        ws.Cell(row, 1).Style.Font.SetItalic().Font.FontSize = 9;

        row += 2;

        // Title
        ws.Cell(row, 1).Value = "PHIẾU NHẬP KHO";
        ws.Range(row, 1, row, 6).Merge();
        ws.Cell(row, 1).Style.Font.SetBold().Font.FontSize = 18;
        ws.Cell(row, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        row++;
        ws.Cell(row, 1).Value = $"Ngày {phieu.NgayNhap:dd} tháng {phieu.NgayNhap:MM} năm {phieu.NgayNhap:yyyy}";
        ws.Range(row, 1, row, 6).Merge();
        ws.Cell(row, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        row++;
        ws.Cell(row, 1).Value = $"Số : {phieu.SoPhieu}";
        ws.Range(row, 1, row, 6).Merge();
        ws.Cell(row, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        row += 2;

        // Info
        ws.Cell(row, 1).Value = $"Họ tên người giao hàng: {phieu.NguoiGiaoHang}";
        ws.Range(row, 1, row, 6).Merge();

        row++;
        ws.Cell(row, 1).Value = $"Nhập tại kho: {phieu.Kho?.TenKho ?? ""}";
        ws.Range(row, 1, row, 6).Merge();

        row += 2;
        int tableStartRow = row;

        // Table Header
        ws.Cell(row, 1).Value = "STT";
        ws.Cell(row, 2).Value = "Tên sản phẩm, hàng hóa";
        ws.Cell(row, 3).Value = "Mã vật tư";
        ws.Cell(row, 4).Value = "ĐVT";
        ws.Cell(row, 5).Value = "Số lượng thực nhập";
        ws.Cell(row, 6).Value = "Nhà cung cấp";

        ws.Range(row, 1, row, 6).Style.Font.SetBold()
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
            .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
            .Alignment.SetWrapText(true)
            .Fill.SetBackgroundColor(XLColor.FromHtml("#E0E0E0"));
        ws.Row(row).Height = 35;

        // Table Data
        int stt = 1;
        foreach (var ct in phieu.ChiTietPhieuNhaps)
        {
            row++;
            ws.Cell(row, 1).Value = stt++;
            ws.Cell(row, 2).Value = ct.VatTu?.TenVatTu ?? "";
            ws.Cell(row, 3).Value = ct.VatTu?.MaVatTu ?? "";
            ws.Cell(row, 4).Value = ct.VatTu?.DonViTinh?.TenDonVi ?? "";
            ws.Cell(row, 5).Value = ct.SoLuong;
            ws.Cell(row, 6).Value = ct.NhaCungCap ?? "";

            ws.Cell(row, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell(row, 3).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell(row, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell(row, 5).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
        }

        // Total row
        row++;
        ws.Cell(row, 4).Value = "Tổng số lượng:";
        ws.Cell(row, 4).Style.Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
        ws.Cell(row, 5).Value = phieu.ChiTietPhieuNhaps.Sum(c => c.SoLuong);
        ws.Cell(row, 5).Style.Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

        // Borders
        ws.Range(tableStartRow, 1, row, 6).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin)
            .Border.SetInsideBorder(XLBorderStyleValues.Thin);

        ws.Range(tableStartRow, 5, row, 5).Style.NumberFormat.Format = "#,##0.##";

        row += 2;

        // Date line
        ws.Cell(row, 4).Value = $"Nhập, ngày {phieu.NgayNhap:dd} tháng {phieu.NgayNhap:MM} năm {phieu.NgayNhap:yyyy}";
        ws.Range(row, 4, row, 6).Merge();
        ws.Cell(row, 4).Style.Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        row += 2;

        // Signatures
        ws.Cell(row, 1).Value = "Người lập phiếu";
        ws.Cell(row, 2).Value = "Người giao hàng";
        ws.Cell(row, 3).Value = "Thủ kho";
        ws.Cell(row, 4).Value = "Kế toán trưởng";
        ws.Cell(row, 5).Value = "Giám đốc";
        ws.Range(row, 5, row, 6).Merge();
        ws.Range(row, 1, row, 6).Style.Font.SetBold()
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        row++;
        ws.Cell(row, 1).Value = "(ký ,họ tên)";
        ws.Cell(row, 2).Value = "(ký ,họ tên)";
        ws.Cell(row, 3).Value = "(ký ,họ tên)";
        ws.Cell(row, 4).Value = "(ký ,họ tên)";
        ws.Cell(row, 5).Value = "(ký ,họ tên)";
        ws.Range(row, 5, row, 6).Merge();
        ws.Range(row, 1, row, 6).Style.Font.SetItalic().Font.FontSize = 8;
        ws.Range(row, 1, row, 6).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        // Page setup
        ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
        ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
        ws.PageSetup.Margins.Top = 0.5;
        ws.PageSetup.Margins.Bottom = 0.5;
        ws.PageSetup.Margins.Left = 0.5;
        ws.PageSetup.Margins.Right = 0.5;

        workbook.SaveAs(filePath);
        return Task.CompletedTask;
    }

    public Task ExportPhieuXuatKho(PhieuXuatKho phieu, string filePath)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Phiếu Xuất Kho");

        ws.Column(1).Width = 5;
        ws.Column(2).Width = 22;
        ws.Column(3).Width = 8;
        ws.Column(4).Width = 10;
        ws.Column(5).Width = 16;
        ws.Column(6).Width = 14;
        ws.Column(7).Width = 10;

        int row = 1;

        // Header
        ws.Cell(row, 1).Value = "CÔNG TY CỔ PHẦN TẬP ĐOÀN SÔNG HỒNG THỦ ĐÔ";
        ws.Range(row, 1, row, 4).Merge();
        ws.Cell(row, 1).Style.Font.SetBold().Font.FontSize = 10;

        ws.Cell(row, 5).Value = "SÔNG HỒNG THỦ ĐÔ";
        ws.Range(row, 5, row, 7).Merge();
        ws.Cell(row, 5).Style.Font.SetBold().Font.FontSize = 11;
        ws.Cell(row, 5).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

        row++;
        ws.Cell(row, 1).Value = "Khu đô thị Bắc Đầm Vạc";
        ws.Range(row, 1, row, 4).Merge();
        ws.Cell(row, 1).Style.Font.SetItalic().Font.FontSize = 9;

        row++;
        ws.Cell(row, 1).Value = $"Kho: {phieu.Kho?.TenKho ?? ""}";
        ws.Range(row, 1, row, 4).Merge();
        ws.Cell(row, 1).Style.Font.FontSize = 9;

        row += 2;

        // Title
        ws.Cell(row, 1).Value = "PHIẾU XUẤT KHO KINH DOANH SHTD";
        ws.Range(row, 1, row, 7).Merge();
        ws.Cell(row, 1).Style.Font.SetBold().Font.FontSize = 15;
        ws.Cell(row, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        row++;
        ws.Cell(row, 1).Value = $"Số phiếu xuất: {phieu.SoPhieu}";
        ws.Range(row, 1, row, 7).Merge();
        ws.Cell(row, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        row += 2;

        // Info - 2 columns
        ws.Cell(row, 1).Value = $"Họ tên người xin cấp vật tư : {phieu.NguoiNhan}";
        ws.Range(row, 1, row, 4).Merge();
        ws.Cell(row, 5).Value = $"Ngày tháng xuất: {phieu.NgayXuat:dd/MM/yyyy}";
        ws.Range(row, 5, row, 7).Merge();

        row++;
        ws.Cell(row, 1).Value = $"Bộ Phận: {phieu.BoPhan?.TenBoPhan ?? ""}";
        ws.Range(row, 1, row, 4).Merge();
        ws.Cell(row, 5).Value = $"Mục đích sử dụng: {phieu.MucDichSuDung}";
        ws.Range(row, 5, row, 7).Merge();

        row++;
        ws.Cell(row, 1).Value = $"Nơi nhận: {phieu.NoiNhan}";
        ws.Range(row, 1, row, 4).Merge();

        row += 2;
        int tableStartRow = row;

        // Table Header
        ws.Cell(row, 1).Value = "STT";
        ws.Cell(row, 2).Value = "Tên mặt hàng";
        ws.Cell(row, 3).Value = "Đvt";
        ws.Cell(row, 4).Value = "Số lượng";
        ws.Cell(row, 5).Value = "Mục đích sử dụng";
        ws.Cell(row, 6).Value = "Nơi nhận";
        ws.Cell(row, 7).Value = "Mã vật tư";

        ws.Range(row, 1, row, 7).Style.Font.SetBold()
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
            .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
            .Alignment.SetWrapText(true)
            .Fill.SetBackgroundColor(XLColor.FromHtml("#E0E0E0"));
        ws.Row(row).Height = 30;

        // Table Data
        int stt = 1;
        foreach (var ct in phieu.ChiTietPhieuXuats)
        {
            row++;
            ws.Cell(row, 1).Value = stt++;
            ws.Cell(row, 2).Value = ct.VatTu?.TenVatTu ?? "";
            ws.Cell(row, 3).Value = ct.VatTu?.DonViTinh?.TenDonVi ?? "";
            ws.Cell(row, 4).Value = ct.SoLuong;
            ws.Cell(row, 5).Value = phieu.MucDichSuDung;
            ws.Cell(row, 6).Value = phieu.NoiNhan;
            ws.Cell(row, 7).Value = ct.VatTu?.MaVatTu ?? "";

            ws.Cell(row, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell(row, 3).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell(row, 7).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        }

        // Total row
        row++;
        ws.Cell(row, 3).Value = "Tổng cộng:";
        ws.Cell(row, 3).Style.Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
        ws.Cell(row, 4).Value = phieu.ChiTietPhieuXuats.Sum(c => c.SoLuong);
        ws.Cell(row, 4).Style.Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

        // Borders
        ws.Range(tableStartRow, 1, row, 7).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin)
            .Border.SetInsideBorder(XLBorderStyleValues.Thin);

        ws.Range(tableStartRow, 4, row, 4).Style.NumberFormat.Format = "#,##0.##";

        row += 2;

        // Signatures
        ws.Cell(row, 1).Value = "Thủ kho";
        ws.Range(row, 1, row, 2).Merge();
        ws.Cell(row, 3).Value = "Người nhận hàng";
        ws.Range(row, 3, row, 4).Merge();
        ws.Cell(row, 5).Value = "Kế toán";
        ws.Range(row, 5, row, 6).Merge();
        ws.Cell(row, 7).Value = "Giám đốc";
        ws.Range(row, 1, row, 7).Style.Font.SetBold()
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        row++;
        ws.Cell(row, 1).Value = "(ký ,họ tên)";
        ws.Range(row, 1, row, 2).Merge();
        ws.Cell(row, 3).Value = "(ký ,họ tên)";
        ws.Range(row, 3, row, 4).Merge();
        ws.Cell(row, 5).Value = "(ký ,họ tên)";
        ws.Range(row, 5, row, 6).Merge();
        ws.Cell(row, 7).Value = "(ký ,họ tên)";
        ws.Range(row, 1, row, 7).Style.Font.SetItalic().Font.FontSize = 8;
        ws.Range(row, 1, row, 7).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        // Page setup
        ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
        ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
        ws.PageSetup.Margins.Top = 0.5;
        ws.PageSetup.Margins.Bottom = 0.5;
        ws.PageSetup.Margins.Left = 0.5;
        ws.PageSetup.Margins.Right = 0.5;

        workbook.SaveAs(filePath);
        return Task.CompletedTask;
    }

    public Task ExportDeNghiCapVatTu(DeNghiCapVatTu phieu, string filePath)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Đề nghị cấp VT");

        // Set column widths (8 columns)
        ws.Column(1).Width = 5;
        ws.Column(2).Width = 15;
        ws.Column(3).Width = 15;
        ws.Column(4).Width = 8;
        ws.Column(5).Width = 10;
        ws.Column(6).Width = 10;
        ws.Column(7).Width = 10;
        ws.Column(8).Width = 15;

        int row = 1;

        // === HEADER ===
        ws.Cell(row, 1).Value = "CÔNG TY CP TẬP ĐOÀN\nSÔNG HỒNG THỦ ĐÔ";
        ws.Range(row, 1, row, 3).Merge();
        ws.Cell(row, 1).Style.Font.SetBold().Font.FontSize = 11;
        ws.Cell(row, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
            .Alignment.SetVertical(XLAlignmentVerticalValues.Top).Alignment.SetWrapText(true);

        ws.Cell(row, 4).Value = "CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM\nĐộc lập - Tự do - Hạnh phúc";
        ws.Range(row, 4, row, 8).Merge();
        ws.Cell(row, 4).Style.Font.SetBold().Font.FontSize = 11;
        ws.Cell(row, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
            .Alignment.SetVertical(XLAlignmentVerticalValues.Top).Alignment.SetWrapText(true);
        ws.Row(row).Height = 30;

        row++;
        ws.Cell(row, 4).Value = $"......, ngày {phieu.NgayDeNghi:dd} tháng {phieu.NgayDeNghi:MM} năm {phieu.NgayDeNghi:yyyy}";
        ws.Range(row, 4, row, 8).Merge();
        ws.Cell(row, 4).Style.Font.Italic = true;
        ws.Cell(row, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        row += 2;

        // === TITLE ===
        ws.Cell(row, 1).Value = "GIẤY ĐỀ NGHỊ CẤP VẬT TƯ";
        ws.Range(row, 1, row, 8).Merge();
        ws.Cell(row, 1).Style.Font.SetBold().Font.FontSize = 18;
        ws.Cell(row, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        row += 2;

        // === INFO ===
        ws.Cell(row, 1).Value = $"Họ tên: {phieu.NguoiDeNghi}";
        ws.Range(row, 1, row, 4).Merge();
        ws.Cell(row, 5).Value = $"Chức vụ: {phieu.ChucVu}";
        ws.Range(row, 5, row, 8).Merge();

        row++;
        ws.Cell(row, 1).Value = $"Bộ phận: {phieu.BoPhan?.TenBoPhan ?? ""}";
        ws.Range(row, 1, row, 4).Merge();
        ws.Cell(row, 5).Value = "Số điện thoại:";
        ws.Range(row, 5, row, 8).Merge();

        row++;
        ws.Cell(row, 1).Value = "Xin cấp các loại vật tư sau:";
        ws.Range(row, 1, row, 8).Merge();

        row++;
        int tableStartRow = row;

        // === TABLE HEADER ===
        ws.Cell(row, 1).Value = "STT";
        ws.Cell(row, 2).Value = "Tên vật tư (Quy cách, mẫu\nmã, thông số kỹ thuật...)";
        ws.Range(row, 2, row, 3).Merge();
        ws.Cell(row, 4).Value = "ĐVT";
        ws.Cell(row, 5).Value = "SL\ntồn\nkho";
        ws.Cell(row, 6).Value = "SL\nđề\nnghị";
        ws.Cell(row, 7).Value = "SL\nđược\nduyệt";
        ws.Cell(row, 8).Value = "Ghi\nchú";

        ws.Range(row, 1, row, 8).Style.Font.SetBold()
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
            .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
            .Alignment.SetWrapText(true)
            .Fill.SetBackgroundColor(XLColor.FromHtml("#F3F4F6"));
        ws.Row(row).Height = 45;

        // === TABLE DATA ===
        int stt = 1;
        foreach (var ct in phieu.ChiTietDeNghis)
        {
            row++;
            ws.Cell(row, 1).Value = stt++;
            ws.Cell(row, 2).Value = ct.VatTu?.TenVatTu ?? "";
            ws.Range(row, 2, row, 3).Merge();
            ws.Cell(row, 4).Value = ct.VatTu?.DonViTinh?.TenDonVi ?? "";
            ws.Cell(row, 5).Value = "";
            ws.Cell(row, 6).Value = ct.SoLuongYeuCau;
            if (ct.SoLuongDaCap > 0)
                ws.Cell(row, 7).Value = ct.SoLuongDaCap;
            else
                ws.Cell(row, 7).Value = "";
            ws.Cell(row, 8).Value = ct.GhiChu;

            ws.Cell(row, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell(row, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Range(row, 5, row, 7).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        }

        // Empty rows
        for (int i = 0; i < 2; i++)
        {
            row++;
            ws.Range(row, 2, row, 3).Merge();
            ws.Row(row).Height = 22;
        }

        // Borders
        ws.Range(tableStartRow, 1, row, 8).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin)
            .Border.SetInsideBorder(XLBorderStyleValues.Thin);

        row++;

        // === FOOTER ===
        var totalItems = phieu.ChiTietDeNghis.Count;
        ws.Cell(row, 1).Value = $"(Tổng: {totalItems:D2} loại. Để sử dụng: {phieu.GhiChu})";
        ws.Range(row, 1, row, 8).Merge();
        ws.Cell(row, 1).Style.Font.Italic = true;

        row += 2;

        // === SIGNATURES ===
        ws.Cell(row, 1).Value = "Người duyệt";
        ws.Range(row, 1, row, 2).Merge();
        ws.Cell(row, 3).Value = "Phòng TC-HC";
        ws.Range(row, 3, row, 4).Merge();
        ws.Cell(row, 5).Value = "CB phụ trách";
        ws.Range(row, 5, row, 6).Merge();
        ws.Cell(row, 7).Value = "Người đề nghị";
        ws.Range(row, 7, row, 8).Merge();

        ws.Range(row, 1, row, 8).Style.Font.SetBold()
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        // Page setup
        ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
        ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
        ws.PageSetup.Margins.Top = 0.5;
        ws.PageSetup.Margins.Bottom = 0.5;
        ws.PageSetup.Margins.Left = 0.5;
        ws.PageSetup.Margins.Right = 0.5;

        workbook.SaveAs(filePath);
        return Task.CompletedTask;
    }

    public async Task ExportNhapXuatTonKho(AppDbContext context, string filePath, int khoId, int month, int year)
    {
        var khos = khoId > 0
            ? new List<Models.Kho> { (await context.Khos.FindAsync(khoId))! }
            : await context.Khos.ToListAsync();
        var startOfMonth = new DateTime(year, month, 1);
        var endOfMonth = startOfMonth.AddMonths(1);
        var vatTus = await context.VatTus.Include(v => v.DonViTinh).OrderBy(v => v.MaVatTu).ToListAsync();

        // Collect data per kho
        var rows = new List<(string MaVT, string TenVT, string DVT, decimal TonDau, decimal Nhap, decimal Xuat, decimal TonCK, string Kho)>();
        foreach (var kho in khos)
        {
            foreach (var vt in vatTus)
            {
                var nhapTruoc = await context.ChiTietPhieuNhaps
                    .Where(ct => ct.VatTuId == vt.Id && ct.PhieuNhapKho.KhoId == kho.Id && ct.PhieuNhapKho.NgayNhap < startOfMonth)
                    .SumAsync(ct => (decimal?)ct.SoLuong) ?? 0;
                var xuatTruoc = await context.ChiTietPhieuXuats
                    .Where(ct => ct.VatTuId == vt.Id && ct.PhieuXuatKho.KhoId == kho.Id && ct.PhieuXuatKho.NgayXuat < startOfMonth)
                    .SumAsync(ct => (decimal?)ct.SoLuong) ?? 0;
                var tonDau = nhapTruoc - xuatTruoc;

                var nhapThang = await context.ChiTietPhieuNhaps
                    .Where(ct => ct.VatTuId == vt.Id && ct.PhieuNhapKho.KhoId == kho.Id
                        && ct.PhieuNhapKho.NgayNhap >= startOfMonth && ct.PhieuNhapKho.NgayNhap < endOfMonth)
                    .SumAsync(ct => (decimal?)ct.SoLuong) ?? 0;
                var xuatThang = await context.ChiTietPhieuXuats
                    .Where(ct => ct.VatTuId == vt.Id && ct.PhieuXuatKho.KhoId == kho.Id
                        && ct.PhieuXuatKho.NgayXuat >= startOfMonth && ct.PhieuXuatKho.NgayXuat < endOfMonth)
                    .SumAsync(ct => (decimal?)ct.SoLuong) ?? 0;
                var tonCK = tonDau + nhapThang - xuatThang;

                if (nhapThang > 0 || xuatThang > 0 || tonDau != 0)
                    rows.Add((vt.MaVatTu, vt.TenVatTu, vt.DonViTinh?.TenDonVi ?? "", tonDau, nhapThang, xuatThang, tonCK, kho.TenKho));
            }
        }

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add($"NXT T{month}");

        int row = 1;
        // Header công ty
        ws.Cell(row, 1).Value = "CÔNG TY CỔ PHẦN TẬP ĐOÀN SÔNG HỒNG THỦ ĐÔ";
        ws.Range(row, 1, row, 9).Merge();
        ws.Cell(row, 1).Style.Font.SetBold().Font.FontSize = 11;
        ws.Cell(row, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        row++;
        ws.Cell(row, 1).Value = "Khu đô thị Bắc Đầm Vạc";
        ws.Range(row, 1, row, 9).Merge();
        ws.Cell(row, 1).Style.Font.SetItalic().Font.FontSize = 9;
        ws.Cell(row, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        row += 2;
        ws.Cell(row, 1).Value = $"NHẬP XUẤT TỒN KHO THÁNG {month}/{year}";
        ws.Range(row, 1, row, 9).Merge();
        ws.Cell(row, 1).Style.Font.SetBold().Font.FontSize = 16;
        ws.Cell(row, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        row++;
        ws.Cell(row, 1).Value = $"Kho: {(khoId > 0 ? khos[0].TenKho : "Tất cả")}";
        ws.Range(row, 1, row, 9).Merge();
        ws.Cell(row, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        row += 2;

        // TỔNG CỘNG row
        ws.Cell(row, 1).Value = "TỔNG CỘNG";
        ws.Range(row, 1, row, 4).Merge();
        ws.Cell(row, 1).Style.Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
        ws.Cell(row, 5).Value = rows.Sum(r => r.TonDau);
        ws.Cell(row, 6).Value = rows.Sum(r => r.Nhap);
        ws.Cell(row, 7).Value = rows.Sum(r => r.Xuat);
        ws.Cell(row, 8).Value = rows.Sum(r => r.TonCK);
        ws.Range(row, 5, row, 8).Style.Font.SetBold().NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 6).Style.Font.FontColor = XLColor.FromHtml("#16A34A");
        ws.Cell(row, 7).Style.Font.FontColor = XLColor.FromHtml("#DC2626");
        ws.Cell(row, 8).Style.Font.FontColor = XLColor.FromHtml("#2563EB");
        ws.Range(row, 1, row, 9).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#E8F0FE"))
            .Border.SetOutsideBorder(XLBorderStyleValues.Thin).Border.SetInsideBorder(XLBorderStyleValues.Thin);

        row++;

        // Table header
        string[] headers = { "STT", "Mã VT", "Tên vật tư", "ĐVT", "Tồn đầu kỳ", "Nhập trong kỳ", "Xuất trong kỳ", "Tồn cuối kỳ", "Kho" };
        for (int i = 0; i < headers.Length; i++)
            ws.Cell(row, i + 1).Value = headers[i];
        ws.Range(row, 1, row, 9).Style.Font.SetBold()
            .Border.SetOutsideBorder(XLBorderStyleValues.Thin).Border.SetInsideBorder(XLBorderStyleValues.Thin)
            .Fill.SetBackgroundColor(XLColor.LightGray)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        int tableStartRow = row;

        // Data rows
        int stt = 1;
        foreach (var r in rows)
        {
            row++;
            ws.Cell(row, 1).Value = stt++;
            ws.Cell(row, 2).Value = r.MaVT;
            ws.Cell(row, 3).Value = r.TenVT;
            ws.Cell(row, 4).Value = r.DVT;
            ws.Cell(row, 5).Value = r.TonDau;
            ws.Cell(row, 6).Value = r.Nhap;
            ws.Cell(row, 7).Value = r.Xuat;
            ws.Cell(row, 8).Value = r.TonCK;
            ws.Cell(row, 9).Value = r.Kho;
            ws.Cell(row, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell(row, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell(row, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Range(row, 1, row, 9).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin).Border.SetInsideBorder(XLBorderStyleValues.Thin);
        }

        ws.Range(tableStartRow, 5, row, 8).Style.NumberFormat.Format = "#,##0.00";
        ws.Columns().AdjustToContents();
        ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
        ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
        workbook.SaveAs(filePath);
    }

    public async Task ExportTonKho(AppDbContext context, string filePath)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Tồn Kho");

        ws.Cell("A1").Value = "BÁO CÁO TỒN KHO";
        ws.Range("A1:H1").Merge().Style.Font.SetBold().Font.SetFontSize(14).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        ws.Cell("A2").Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy}";

        int row = 4;
        string[] headers = { "STT", "Kho", "Mã VT", "Tên vật tư", "ĐVT", "SL Nhập", "SL Xuất", "Tồn" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(row, i + 1).Value = headers[i];
        }
        ws.Range(row, 1, row, 8).Style.Font.SetBold().Border.SetOutsideBorder(XLBorderStyleValues.Thin);

        var vatTus = await context.VatTus.Include(v => v.DonViTinh).ToListAsync();
        var khos = await context.Khos.ToListAsync();

        int stt = 1;
        foreach (var kho in khos)
        {
            foreach (var vt in vatTus)
            {
                var nhap = await context.ChiTietPhieuNhaps
                    .Where(ct => ct.VatTuId == vt.Id && ct.PhieuNhapKho.KhoId == kho.Id)
                    .SumAsync(ct => (decimal?)ct.SoLuong) ?? 0;
                var xuat = await context.ChiTietPhieuXuats
                    .Where(ct => ct.VatTuId == vt.Id && ct.PhieuXuatKho.KhoId == kho.Id)
                    .SumAsync(ct => (decimal?)ct.SoLuong) ?? 0;

                if (nhap > 0 || xuat > 0)
                {
                    row++;
                    ws.Cell(row, 1).Value = stt++;
                    ws.Cell(row, 2).Value = kho.TenKho;
                    ws.Cell(row, 3).Value = vt.MaVatTu;
                    ws.Cell(row, 4).Value = vt.TenVatTu;
                    ws.Cell(row, 5).Value = vt.DonViTinh?.TenDonVi ?? "";
                    ws.Cell(row, 6).Value = nhap;
                    ws.Cell(row, 7).Value = xuat;
                    ws.Cell(row, 8).Value = nhap - xuat;
                    ws.Range(row, 1, row, 8).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                }
            }
        }

        ws.Range(4, 6, row, 8).Style.NumberFormat.Format = "#,##0.00";
        ws.Columns().AdjustToContents();

        workbook.SaveAs(filePath);
    }

    public async Task ExportDanhSachNhapKho(AppDbContext context, string filePath, DateTime from, DateTime to)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("DS Nhập Kho");

        ws.Cell("A1").Value = "DANH SÁCH PHIẾU NHẬP KHO";
        ws.Range("A1:F1").Merge().Style.Font.SetBold().Font.SetFontSize(14).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        ws.Cell("A2").Value = $"Từ {from:dd/MM/yyyy} đến {to:dd/MM/yyyy}";

        int row = 4;
        string[] headers = { "STT", "Số phiếu", "Ngày nhập", "Người giao", "Kho", "Tổng tiền" };
        for (int i = 0; i < headers.Length; i++) ws.Cell(row, i + 1).Value = headers[i];
        ws.Range(row, 1, row, 6).Style.Font.SetBold().Border.SetOutsideBorder(XLBorderStyleValues.Thin);

        var phieus = await context.PhieuNhapKhos.Include(p => p.Kho)
            .Where(p => p.NgayNhap >= from && p.NgayNhap <= to.AddDays(1))
            .OrderByDescending(p => p.NgayNhap).ToListAsync();

        int stt = 1;
        foreach (var p in phieus)
        {
            row++;
            ws.Cell(row, 1).Value = stt++;
            ws.Cell(row, 2).Value = p.SoPhieu;
            ws.Cell(row, 3).Value = p.NgayNhap.ToString("dd/MM/yyyy");
            ws.Cell(row, 4).Value = p.NguoiGiaoHang;
            ws.Cell(row, 5).Value = p.Kho?.TenKho ?? "";
            ws.Cell(row, 6).Value = p.TongTien;
            ws.Range(row, 1, row, 6).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        }

        ws.Range(4, 6, row, 6).Style.NumberFormat.Format = "#,##0";
        ws.Columns().AdjustToContents();
        workbook.SaveAs(filePath);
    }

    public async Task ExportDanhSachXuatKho(AppDbContext context, string filePath, DateTime from, DateTime to)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("DS Xuất Kho");

        ws.Cell("A1").Value = "DANH SÁCH PHIẾU XUẤT KHO";
        ws.Range("A1:F1").Merge().Style.Font.SetBold().Font.SetFontSize(14).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        ws.Cell("A2").Value = $"Từ {from:dd/MM/yyyy} đến {to:dd/MM/yyyy}";

        int row = 4;
        string[] headers = { "STT", "Số phiếu", "Ngày xuất", "Người nhận", "Kho", "Tổng tiền" };
        for (int i = 0; i < headers.Length; i++) ws.Cell(row, i + 1).Value = headers[i];
        ws.Range(row, 1, row, 6).Style.Font.SetBold().Border.SetOutsideBorder(XLBorderStyleValues.Thin);

        var phieus = await context.PhieuXuatKhos.Include(p => p.Kho)
            .Where(p => p.NgayXuat >= from && p.NgayXuat <= to.AddDays(1))
            .OrderByDescending(p => p.NgayXuat).ToListAsync();

        int stt = 1;
        foreach (var p in phieus)
        {
            row++;
            ws.Cell(row, 1).Value = stt++;
            ws.Cell(row, 2).Value = p.SoPhieu;
            ws.Cell(row, 3).Value = p.NgayXuat.ToString("dd/MM/yyyy");
            ws.Cell(row, 4).Value = p.NguoiNhan;
            ws.Cell(row, 5).Value = p.Kho?.TenKho ?? "";
            ws.Cell(row, 6).Value = p.TongTien;
            ws.Range(row, 1, row, 6).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        }

        ws.Range(4, 6, row, 6).Style.NumberFormat.Format = "#,##0";
        ws.Columns().AdjustToContents();
        workbook.SaveAs(filePath);
    }

    // ===== BATCH EXPORT - Gộp nhiều phiếu vào 1 file Excel (mỗi phiếu 1 sheet) =====

    public Task ExportMultiPhieuNhapKho(List<PhieuNhapKho> phieus, string filePath)
    {
        using var workbook = new XLWorkbook();
        int idx = 1;
        foreach (var phieu in phieus)
        {
            var ws = workbook.Worksheets.Add($"NK_{idx++}_{phieu.SoPhieu}".Length > 31
                ? $"NK_{idx - 1}" : $"NK_{idx - 1}_{phieu.SoPhieu}");
            RenderPhieuNhapKhoSheet(ws, phieu);
        }
        workbook.SaveAs(filePath);
        return Task.CompletedTask;
    }

    public Task ExportMultiPhieuXuatKho(List<PhieuXuatKho> phieus, string filePath)
    {
        using var workbook = new XLWorkbook();
        int idx = 1;
        foreach (var phieu in phieus)
        {
            var ws = workbook.Worksheets.Add($"XK_{idx++}_{phieu.SoPhieu}".Length > 31
                ? $"XK_{idx - 1}" : $"XK_{idx - 1}_{phieu.SoPhieu}");
            RenderPhieuXuatKhoSheet(ws, phieu);
        }
        workbook.SaveAs(filePath);
        return Task.CompletedTask;
    }

    public Task ExportMultiDeNghiCapVatTu(List<DeNghiCapVatTu> phieus, string filePath)
    {
        using var workbook = new XLWorkbook();
        int idx = 1;
        foreach (var phieu in phieus)
        {
            var ws = workbook.Worksheets.Add($"DN_{idx++}_{phieu.SoPhieu}".Length > 31
                ? $"DN_{idx - 1}" : $"DN_{idx - 1}_{phieu.SoPhieu}");
            RenderDeNghiCapVatTuSheet(ws, phieu);
        }
        workbook.SaveAs(filePath);
        return Task.CompletedTask;
    }

    // Reuse single-phieu rendering logic
    private void RenderPhieuNhapKhoSheet(IXLWorksheet ws, PhieuNhapKho phieu) => ExportPhieuNhapKhoToSheet(ws, phieu);
    private void RenderPhieuXuatKhoSheet(IXLWorksheet ws, PhieuXuatKho phieu) => ExportPhieuXuatKhoToSheet(ws, phieu);
    private void RenderDeNghiCapVatTuSheet(IXLWorksheet ws, DeNghiCapVatTu phieu) => ExportDeNghiCapVatTuToSheet(ws, phieu);

    private void ExportPhieuNhapKhoToSheet(IXLWorksheet ws, PhieuNhapKho phieu)
    {
        ws.Column(1).Width = 5; ws.Column(2).Width = 25; ws.Column(3).Width = 12;
        ws.Column(4).Width = 8; ws.Column(5).Width = 14; ws.Column(6).Width = 18;

        int row = 1;
        ws.Cell(row, 1).Value = "CÔNG TY CỔ PHẦN TẬP ĐOÀN SÔNG HỒNG THỦ ĐÔ";
        ws.Range(row, 1, row, 4).Merge(); ws.Cell(row, 1).Style.Font.SetBold().Font.SetItalic().Font.FontSize = 10;
        ws.Cell(row, 5).Value = "SÔNG HỒNG THỦ ĐÔ"; ws.Range(row, 5, row, 6).Merge();
        ws.Cell(row, 5).Style.Font.SetBold().Font.FontSize = 11; ws.Cell(row, 5).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
        row++; ws.Cell(row, 1).Value = "Khu đô thị Bắc Đầm Vạc"; ws.Range(row, 1, row, 4).Merge(); ws.Cell(row, 1).Style.Font.SetItalic().Font.FontSize = 9;
        ws.Cell(row, 5).Value = $"Số HĐ: {phieu.SoHopDong}"; ws.Range(row, 5, row, 6).Merge();
        ws.Cell(row, 5).Style.Font.SetBold().Font.FontSize = 10; ws.Cell(row, 5).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
        row++; ws.Cell(row, 1).Value = $"Bộ phận: {phieu.BoPhan?.TenBoPhan ?? ""}"; ws.Range(row, 1, row, 4).Merge(); ws.Cell(row, 1).Style.Font.SetItalic().Font.FontSize = 9;
        row += 2;
        ws.Cell(row, 1).Value = "PHIẾU NHẬP KHO"; ws.Range(row, 1, row, 6).Merge();
        ws.Cell(row, 1).Style.Font.SetBold().Font.FontSize = 18; ws.Cell(row, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        row++; ws.Cell(row, 1).Value = $"Ngày {phieu.NgayNhap:dd} tháng {phieu.NgayNhap:MM} năm {phieu.NgayNhap:yyyy}"; ws.Range(row, 1, row, 6).Merge(); ws.Cell(row, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        row++; ws.Cell(row, 1).Value = $"Số : {phieu.SoPhieu}"; ws.Range(row, 1, row, 6).Merge(); ws.Cell(row, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        row += 2;
        ws.Cell(row, 1).Value = $"Họ tên người giao hàng: {phieu.NguoiGiaoHang}"; ws.Range(row, 1, row, 6).Merge();
        row++; ws.Cell(row, 1).Value = $"Nhập tại kho: {phieu.Kho?.TenKho ?? ""}"; ws.Range(row, 1, row, 6).Merge();
        row += 2; int tableStartRow = row;
        ws.Cell(row, 1).Value = "STT"; ws.Cell(row, 2).Value = "Tên sản phẩm, hàng hóa"; ws.Cell(row, 3).Value = "Mã vật tư";
        ws.Cell(row, 4).Value = "ĐVT"; ws.Cell(row, 5).Value = "Số lượng thực nhập"; ws.Cell(row, 6).Value = "Nhà cung cấp";
        ws.Range(row, 1, row, 6).Style.Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center).Alignment.SetWrapText(true).Fill.SetBackgroundColor(XLColor.FromHtml("#E0E0E0"));
        ws.Row(row).Height = 35;
        int stt = 1;
        foreach (var ct in phieu.ChiTietPhieuNhaps) { row++; ws.Cell(row, 1).Value = stt++; ws.Cell(row, 2).Value = ct.VatTu?.TenVatTu ?? ""; ws.Cell(row, 3).Value = ct.VatTu?.MaVatTu ?? ""; ws.Cell(row, 4).Value = ct.VatTu?.DonViTinh?.TenDonVi ?? ""; ws.Cell(row, 5).Value = ct.SoLuong; ws.Cell(row, 6).Value = ct.NhaCungCap ?? ""; ws.Cell(row, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center); }
        row++; ws.Cell(row, 4).Value = "Tổng số lượng:"; ws.Cell(row, 4).Style.Font.SetBold(); ws.Cell(row, 5).Value = phieu.ChiTietPhieuNhaps.Sum(c => c.SoLuong); ws.Cell(row, 5).Style.Font.SetBold();
        ws.Range(tableStartRow, 1, row, 6).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin).Border.SetInsideBorder(XLBorderStyleValues.Thin);
        ws.Range(tableStartRow, 5, row, 5).Style.NumberFormat.Format = "#,##0.##";
        ws.PageSetup.PageOrientation = XLPageOrientation.Portrait; ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
    }

    private void ExportPhieuXuatKhoToSheet(IXLWorksheet ws, PhieuXuatKho phieu)
    {
        ws.Column(1).Width = 5; ws.Column(2).Width = 22; ws.Column(3).Width = 8;
        ws.Column(4).Width = 10; ws.Column(5).Width = 16; ws.Column(6).Width = 14; ws.Column(7).Width = 10;

        int row = 1;
        ws.Cell(row, 1).Value = "CÔNG TY CỔ PHẦN TẬP ĐOÀN SÔNG HỒNG THỦ ĐÔ"; ws.Range(row, 1, row, 4).Merge(); ws.Cell(row, 1).Style.Font.SetBold().Font.FontSize = 10;
        ws.Cell(row, 5).Value = "SÔNG HỒNG THỦ ĐÔ"; ws.Range(row, 5, row, 7).Merge(); ws.Cell(row, 5).Style.Font.SetBold().Font.FontSize = 11; ws.Cell(row, 5).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
        row++; ws.Cell(row, 1).Value = "Khu đô thị Bắc Đầm Vạc"; ws.Range(row, 1, row, 4).Merge(); ws.Cell(row, 1).Style.Font.SetItalic().Font.FontSize = 9;
        row++; ws.Cell(row, 1).Value = $"Kho: {phieu.Kho?.TenKho ?? ""}"; ws.Range(row, 1, row, 4).Merge();
        row += 2;
        ws.Cell(row, 1).Value = "PHIẾU XUẤT KHO KINH DOANH SHTD"; ws.Range(row, 1, row, 7).Merge(); ws.Cell(row, 1).Style.Font.SetBold().Font.FontSize = 15; ws.Cell(row, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        row++; ws.Cell(row, 1).Value = $"Số phiếu xuất: {phieu.SoPhieu}"; ws.Range(row, 1, row, 7).Merge(); ws.Cell(row, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        row += 2;
        ws.Cell(row, 1).Value = $"Họ tên người xin cấp vật tư : {phieu.NguoiNhan}"; ws.Range(row, 1, row, 4).Merge();
        ws.Cell(row, 5).Value = $"Ngày tháng xuất: {phieu.NgayXuat:dd/MM/yyyy}"; ws.Range(row, 5, row, 7).Merge();
        row++; ws.Cell(row, 1).Value = $"Bộ Phận: {phieu.BoPhan?.TenBoPhan ?? ""}"; ws.Range(row, 1, row, 4).Merge();
        ws.Cell(row, 5).Value = $"Mục đích sử dụng: {phieu.MucDichSuDung}"; ws.Range(row, 5, row, 7).Merge();
        row++; ws.Cell(row, 1).Value = $"Nơi nhận: {phieu.NoiNhan}"; ws.Range(row, 1, row, 4).Merge();
        row += 2; int tableStartRow = row;
        ws.Cell(row, 1).Value = "STT"; ws.Cell(row, 2).Value = "Tên mặt hàng"; ws.Cell(row, 3).Value = "Đvt"; ws.Cell(row, 4).Value = "Số lượng";
        ws.Cell(row, 5).Value = "Mục đích sử dụng"; ws.Cell(row, 6).Value = "Nơi nhận"; ws.Cell(row, 7).Value = "Mã vật tư";
        ws.Range(row, 1, row, 7).Style.Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center).Alignment.SetWrapText(true).Fill.SetBackgroundColor(XLColor.FromHtml("#E0E0E0"));
        int stt = 1;
        foreach (var ct in phieu.ChiTietPhieuXuats) { row++; ws.Cell(row, 1).Value = stt++; ws.Cell(row, 2).Value = ct.VatTu?.TenVatTu ?? ""; ws.Cell(row, 3).Value = ct.VatTu?.DonViTinh?.TenDonVi ?? ""; ws.Cell(row, 4).Value = ct.SoLuong; ws.Cell(row, 5).Value = phieu.MucDichSuDung; ws.Cell(row, 6).Value = phieu.NoiNhan; ws.Cell(row, 7).Value = ct.VatTu?.MaVatTu ?? ""; ws.Cell(row, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center); }
        row++; ws.Cell(row, 3).Value = "Tổng cộng:"; ws.Cell(row, 3).Style.Font.SetBold(); ws.Cell(row, 4).Value = phieu.ChiTietPhieuXuats.Sum(c => c.SoLuong); ws.Cell(row, 4).Style.Font.SetBold();
        ws.Range(tableStartRow, 1, row, 7).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin).Border.SetInsideBorder(XLBorderStyleValues.Thin);
        ws.Range(tableStartRow, 4, row, 4).Style.NumberFormat.Format = "#,##0.##";
        ws.PageSetup.PageOrientation = XLPageOrientation.Portrait; ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
    }

    private void ExportDeNghiCapVatTuToSheet(IXLWorksheet ws, DeNghiCapVatTu phieu)
    {
        ws.Column(1).Width = 5; ws.Column(2).Width = 15; ws.Column(3).Width = 15; ws.Column(4).Width = 8;
        ws.Column(5).Width = 10; ws.Column(6).Width = 10; ws.Column(7).Width = 10; ws.Column(8).Width = 15;
        int row = 1;
        ws.Cell(row, 1).Value = "CÔNG TY CP TẬP ĐOÀN\nSÔNG HỒNG THỦ ĐÔ"; ws.Range(row, 1, row, 3).Merge(); ws.Cell(row, 1).Style.Font.SetBold().Font.FontSize = 11; ws.Cell(row, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center).Alignment.SetWrapText(true);
        ws.Cell(row, 4).Value = "CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM\nĐộc lập - Tự do - Hạnh phúc"; ws.Range(row, 4, row, 8).Merge(); ws.Cell(row, 4).Style.Font.SetBold().Font.FontSize = 11; ws.Cell(row, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center).Alignment.SetWrapText(true);
        ws.Row(row).Height = 30;
        row += 2;
        ws.Cell(row, 1).Value = "GIẤY ĐỀ NGHỊ CẤP VẬT TƯ"; ws.Range(row, 1, row, 8).Merge(); ws.Cell(row, 1).Style.Font.SetBold().Font.FontSize = 18; ws.Cell(row, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        row += 2;
        ws.Cell(row, 1).Value = $"Họ tên: {phieu.NguoiDeNghi}"; ws.Range(row, 1, row, 4).Merge(); ws.Cell(row, 5).Value = $"Chức vụ: {phieu.ChucVu}"; ws.Range(row, 5, row, 8).Merge();
        row++; ws.Cell(row, 1).Value = $"Bộ phận: {phieu.BoPhan?.TenBoPhan ?? ""}"; ws.Range(row, 1, row, 4).Merge();
        row++; ws.Cell(row, 1).Value = "Xin cấp các loại vật tư sau:"; ws.Range(row, 1, row, 8).Merge();
        row++; int tableStartRow = row;
        ws.Cell(row, 1).Value = "STT"; ws.Cell(row, 2).Value = "Tên vật tư"; ws.Range(row, 2, row, 3).Merge(); ws.Cell(row, 4).Value = "ĐVT";
        ws.Cell(row, 5).Value = "SL tồn kho"; ws.Cell(row, 6).Value = "SL đề nghị"; ws.Cell(row, 7).Value = "SL được duyệt"; ws.Cell(row, 8).Value = "Ghi chú";
        ws.Range(row, 1, row, 8).Style.Font.SetBold().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center).Alignment.SetWrapText(true).Fill.SetBackgroundColor(XLColor.FromHtml("#F3F4F6"));
        int stt = 1;
        foreach (var ct in phieu.ChiTietDeNghis) { row++; ws.Cell(row, 1).Value = stt++; ws.Cell(row, 2).Value = ct.VatTu?.TenVatTu ?? ""; ws.Range(row, 2, row, 3).Merge(); ws.Cell(row, 4).Value = ct.VatTu?.DonViTinh?.TenDonVi ?? ""; ws.Cell(row, 6).Value = ct.SoLuongYeuCau; if (ct.SoLuongDaCap > 0) ws.Cell(row, 7).Value = ct.SoLuongDaCap; ws.Cell(row, 8).Value = ct.GhiChu; ws.Cell(row, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center); }
        ws.Range(tableStartRow, 1, row, 8).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin).Border.SetInsideBorder(XLBorderStyleValues.Thin);
        ws.PageSetup.PageOrientation = XLPageOrientation.Portrait; ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
    }
}
