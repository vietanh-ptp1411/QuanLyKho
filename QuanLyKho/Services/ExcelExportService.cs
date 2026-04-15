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

        ws.Cell("A1").Value = "PHIẾU NHẬP KHO";
        ws.Range("A1:D1").Merge().Style.Font.SetBold().Font.SetFontSize(14).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        ws.Cell("A2").Value = $"Số phiếu: {phieu.SoPhieu}";
        ws.Cell("A3").Value = $"Ngày nhập: {phieu.NgayNhap:dd/MM/yyyy}";
        ws.Cell("A4").Value = $"Người giao hàng: {phieu.NguoiGiaoHang}";
        ws.Cell("A5").Value = $"Kho: {phieu.Kho?.TenKho}";
        ws.Cell("A6").Value = "Bộ phận: ";

        int row = 8;
        ws.Cell(row, 1).Value = "STT";
        ws.Cell(row, 2).Value = "Tên vật tư";
        ws.Cell(row, 3).Value = "ĐVT";
        ws.Cell(row, 4).Value = "Số lượng";
        ws.Range(row, 1, row, 4).Style.Font.SetBold().Border.SetOutsideBorder(XLBorderStyleValues.Thin);

        int stt = 1;
        foreach (var ct in phieu.ChiTietPhieuNhaps)
        {
            row++;
            ws.Cell(row, 1).Value = stt++;
            ws.Cell(row, 2).Value = ct.VatTu?.TenVatTu ?? "";
            ws.Cell(row, 3).Value = ct.VatTu?.DonViTinh?.TenDonVi ?? "";
            ws.Cell(row, 4).Value = ct.SoLuong;
            ws.Range(row, 1, row, 4).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        }

        row++;
        ws.Cell(row, 3).Value = "Tổng số lượng:";
        ws.Cell(row, 3).Style.Font.SetBold();
        ws.Cell(row, 4).Value = phieu.ChiTietPhieuNhaps.Sum(c => c.SoLuong);
        ws.Cell(row, 4).Style.Font.SetBold();

        ws.Range(8, 4, row, 4).Style.NumberFormat.Format = "#,##0.##";
        ws.Columns().AdjustToContents();

        workbook.SaveAs(filePath);
        return Task.CompletedTask;
    }

    public Task ExportPhieuXuatKho(PhieuXuatKho phieu, string filePath)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Phiếu Xuất Kho");

        ws.Cell("A1").Value = "PHIẾU XUẤT KHO";
        ws.Range("A1:F1").Merge().Style.Font.SetBold().Font.SetFontSize(14).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        ws.Cell("A2").Value = $"Số phiếu: {phieu.SoPhieu}";
        ws.Cell("A3").Value = $"Ngày xuất: {phieu.NgayXuat:dd/MM/yyyy}";
        ws.Cell("A4").Value = $"Người nhận: {phieu.NguoiNhan}";
        ws.Cell("A5").Value = $"Kho: {phieu.Kho?.TenKho}";
        ws.Cell("A6").Value = $"Mục đích: {phieu.MucDichSuDung}";

        int row = 8;
        ws.Cell(row, 1).Value = "STT";
        ws.Cell(row, 2).Value = "Tên vật tư";
        ws.Cell(row, 3).Value = "ĐVT";
        ws.Cell(row, 4).Value = "Số lượng";
        ws.Cell(row, 5).Value = "Đơn giá";
        ws.Cell(row, 6).Value = "Thành tiền";
        ws.Range(row, 1, row, 6).Style.Font.SetBold().Border.SetOutsideBorder(XLBorderStyleValues.Thin);

        int stt = 1;
        foreach (var ct in phieu.ChiTietPhieuXuats)
        {
            row++;
            ws.Cell(row, 1).Value = stt++;
            ws.Cell(row, 2).Value = ct.VatTu?.TenVatTu ?? "";
            ws.Cell(row, 3).Value = ct.VatTu?.DonViTinh?.TenDonVi ?? "";
            ws.Cell(row, 4).Value = ct.SoLuong;
            ws.Cell(row, 5).Value = ct.DonGia;
            ws.Cell(row, 6).Value = ct.ThanhTien;
            ws.Range(row, 1, row, 6).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        }

        row++;
        ws.Cell(row, 5).Value = "Tổng cộng:";
        ws.Cell(row, 5).Style.Font.SetBold();
        ws.Cell(row, 6).Value = phieu.TongTien;
        ws.Cell(row, 6).Style.Font.SetBold();

        ws.Range(8, 4, row, 6).Style.NumberFormat.Format = "#,##0";
        ws.Columns().AdjustToContents();

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
        var kho = await context.Khos.FindAsync(khoId);
        var startOfMonth = new DateTime(year, month, 1);
        var endOfMonth = startOfMonth.AddMonths(1);
        var vatTus = await context.VatTus.Include(v => v.DonViTinh).OrderBy(v => v.MaVatTu).ToListAsync();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add($"Tồn kho T{month}");

        ws.Cell("A1").Value = $"NHẬP XUẤT TỒN KHO THÁNG {month}/{year}";
        ws.Range("A1:J1").Merge().Style.Font.SetBold().Font.SetFontSize(16).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        ws.Cell("A2").Value = $"Kho: {kho?.TenKho}";
        ws.Range("A2:J2").Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        int row = 4;
        string[] headers = { "STT", "Mã vật tư", "Tên vật tư", "ĐVT", "Tồn đầu", "Nhập", "Xuất", "Tồn CK", "Tồn TT", "Chênh lệch" };
        for (int i = 0; i < headers.Length; i++)
            ws.Cell(row, i + 1).Value = headers[i];
        ws.Range(row, 1, row, 10).Style.Font.SetBold().Border.SetOutsideBorder(XLBorderStyleValues.Thin)
            .Fill.SetBackgroundColor(XLColor.LightGray);

        int stt = 1;
        foreach (var vt in vatTus)
        {
            var nhapTruoc = await context.ChiTietPhieuNhaps
                .Where(ct => ct.VatTuId == vt.Id && ct.PhieuNhapKho.KhoId == khoId && ct.PhieuNhapKho.NgayNhap < startOfMonth)
                .SumAsync(ct => (decimal?)ct.SoLuong) ?? 0;
            var xuatTruoc = await context.ChiTietPhieuXuats
                .Where(ct => ct.VatTuId == vt.Id && ct.PhieuXuatKho.KhoId == khoId && ct.PhieuXuatKho.NgayXuat < startOfMonth)
                .SumAsync(ct => (decimal?)ct.SoLuong) ?? 0;
            var tonDau = nhapTruoc - xuatTruoc;

            var nhapThang = await context.ChiTietPhieuNhaps
                .Where(ct => ct.VatTuId == vt.Id && ct.PhieuNhapKho.KhoId == khoId
                    && ct.PhieuNhapKho.NgayNhap >= startOfMonth && ct.PhieuNhapKho.NgayNhap < endOfMonth)
                .SumAsync(ct => (decimal?)ct.SoLuong) ?? 0;
            var xuatThang = await context.ChiTietPhieuXuats
                .Where(ct => ct.VatTuId == vt.Id && ct.PhieuXuatKho.KhoId == khoId
                    && ct.PhieuXuatKho.NgayXuat >= startOfMonth && ct.PhieuXuatKho.NgayXuat < endOfMonth)
                .SumAsync(ct => (decimal?)ct.SoLuong) ?? 0;
            var tonCK = tonDau + nhapThang - xuatThang;

            row++;
            ws.Cell(row, 1).Value = stt++;
            ws.Cell(row, 2).Value = vt.MaVatTu;
            ws.Cell(row, 3).Value = vt.TenVatTu;
            ws.Cell(row, 4).Value = vt.DonViTinh?.TenDonVi ?? "";
            ws.Cell(row, 5).Value = tonDau;
            ws.Cell(row, 6).Value = nhapThang;
            ws.Cell(row, 7).Value = xuatThang;
            ws.Cell(row, 8).Value = tonCK;
            // Tồn TT, Chênh lệch - để trống
            ws.Range(row, 1, row, 10).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        }

        ws.Range(4, 5, row, 10).Style.NumberFormat.Format = "#,##0";
        ws.Columns().AdjustToContents();
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
}
