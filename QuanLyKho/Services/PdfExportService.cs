using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuanLyKho.Data;
using QuanLyKho.Models;

namespace QuanLyKho.Services;

public class PdfExportService : IPdfExportService
{
    private const string CompanyName = "CÔNG TY CỔ PHẦN TẬP ĐOÀN SÔNG HỒNG THỦ ĐÔ";
    private const string CompanyAddr = "Số 199 Lam Kinh - Thành phố Việt Trì - Phú Thọ";

    public Task ExportPhieuNhapKho(PhieuNhapKho phieu, string filePath)
    {
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(1.5f, Unit.Centimetre);
                page.MarginVertical(1.2f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));
                page.Content().Column(col => RenderPhieuNhapKhoContent(col, phieu));
            });
        }).GeneratePdf(filePath);
        return Task.CompletedTask;
    }

    private void RenderPhieuNhapKhoContent(ColumnDescriptor col, PhieuNhapKho phieu)
    {
        col.Item().Row(row =>
        {
            row.RelativeItem(2).Column(left =>
            {
                left.Item().Text(CompanyName).Bold().Italic().FontSize(10);
                left.Item().Text("Khu đô thị Bắc Đầm Vạc").Italic().FontSize(9);
                left.Item().Text(t => { t.Span("Bộ phận: ").Italic().FontSize(9); t.Span(phieu.BoPhan?.TenBoPhan ?? "").FontSize(9); });
            });
            row.RelativeItem(1).AlignRight().Column(right =>
            {
                right.Item().AlignRight().Text("SÔNG HỒNG THỦ ĐÔ").Bold().FontSize(11);
                right.Item().AlignRight().Text(t => { t.Span("Số HĐ: ").Bold().FontSize(10); t.Span(phieu.SoHopDong).FontSize(10); });
            });
        });
        col.Item().Height(15);
        col.Item().AlignCenter().Text("PHIẾU NHẬP KHO").Bold().FontSize(18);
        col.Item().AlignCenter().Text($"Ngày {phieu.NgayNhap:dd} tháng {phieu.NgayNhap:MM} năm {phieu.NgayNhap:yyyy}").FontSize(10);
        col.Item().AlignCenter().Text($"Số : {phieu.SoPhieu}").FontSize(10);
        col.Item().Height(10);
        col.Item().Text(t => { t.Span("Họ tên người giao hàng: ").Bold(); t.Span(phieu.NguoiGiaoHang); });
        col.Item().Text(t => { t.Span("Nhập tại kho: ").Bold(); t.Span(phieu.Kho?.TenKho ?? ""); });
        col.Item().Height(8);
        col.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(30); columns.RelativeColumn(4); columns.RelativeColumn(1.2f);
                columns.RelativeColumn(0.8f); columns.RelativeColumn(1.2f); columns.RelativeColumn(1.5f);
            });
            table.Header(header =>
            {
                void HC(IContainer c, string text) => c.Border(0.5f).Background("#E0E0E0").Padding(4).AlignCenter().AlignMiddle().Text(text).Bold().FontSize(9);
                HC(header.Cell(), "STT"); HC(header.Cell(), "Tên sản phẩm, hàng hóa"); HC(header.Cell(), "Mã vật tư");
                HC(header.Cell(), "ĐVT"); HC(header.Cell(), "Số lượng\nthực nhập"); HC(header.Cell(), "Nhà cung cấp");
            });
            int stt = 1;
            foreach (var ct in phieu.ChiTietPhieuNhaps)
            {
                table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(stt.ToString()).FontSize(9);
                table.Cell().Border(0.5f).Padding(3).Text(ct.VatTu?.TenVatTu ?? "").FontSize(9);
                table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(ct.VatTu?.MaVatTu ?? "").FontSize(9);
                table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(ct.VatTu?.DonViTinh?.TenDonVi ?? "").FontSize(9);
                table.Cell().Border(0.5f).Padding(3).AlignRight().Text(ct.SoLuong.ToString("N2")).FontSize(9);
                table.Cell().Border(0.5f).Padding(3).Text(ct.NhaCungCap ?? "").FontSize(9);
                stt++;
            }
            table.Cell().ColumnSpan(4).Border(0.5f).Padding(3).AlignRight().Text("Tổng số lượng:").Bold().FontSize(9);
            table.Cell().Border(0.5f).Padding(3).AlignRight().Text(phieu.ChiTietPhieuNhaps.Sum(c => c.SoLuong).ToString("N2")).Bold().FontSize(9);
            table.Cell().Border(0.5f).Padding(3).Text("").FontSize(9);
        });
        col.Item().Height(15);
        col.Item().AlignRight().Text($"Nhập, ngày {phieu.NgayNhap:dd} tháng {phieu.NgayNhap:MM} năm {phieu.NgayNhap:yyyy}").Bold().FontSize(10);
        col.Item().Height(8);
        col.Item().Row(row =>
        {
            void SignBlock(IContainer c, string title) { c.AlignCenter().Column(sc => { sc.Item().AlignCenter().Text(title).Bold().FontSize(9); sc.Item().AlignCenter().Text("(ký ,họ tên)").Italic().FontSize(7); sc.Item().Height(50); }); }
            row.RelativeItem().Element(c => SignBlock(c, "Người lập phiếu"));
            row.RelativeItem().Element(c => SignBlock(c, "Người giao hàng"));
            row.RelativeItem().Element(c => SignBlock(c, "Thủ kho"));
            row.RelativeItem().Element(c => SignBlock(c, "Kế toán trưởng"));
            row.RelativeItem().Element(c => SignBlock(c, "Giám đốc"));
        });
    }

    public Task ExportPhieuXuatKho(PhieuXuatKho phieu, string filePath)
    {
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(1.5f, Unit.Centimetre);
                page.MarginVertical(1.2f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));
                page.Content().Column(col => RenderPhieuXuatKhoContent(col, phieu));
            });
        }).GeneratePdf(filePath);
        return Task.CompletedTask;
    }

    private void RenderPhieuXuatKhoContent(ColumnDescriptor col, PhieuXuatKho phieu)
    {
        col.Item().Row(row =>
        {
            row.RelativeItem(2).Column(left =>
            {
                left.Item().Text(CompanyName).Bold().FontSize(10);
                left.Item().Text("Khu đô thị Bắc Đầm Vạc").Italic().FontSize(9);
                left.Item().Text(t => { t.Span("Kho: ").FontSize(9); t.Span(phieu.Kho?.TenKho ?? "").FontSize(9); });
            });
            row.RelativeItem(1).AlignRight().Column(right =>
            {
                right.Item().AlignRight().Text("SÔNG HỒNG THỦ ĐÔ").Bold().FontSize(11);
            });
        });
        col.Item().Height(12);
        col.Item().AlignCenter().Text("PHIẾU XUẤT KHO KINH DOANH SHTD").Bold().FontSize(15);
        col.Item().AlignCenter().Text($"Số phiếu xuất: {phieu.SoPhieu}").FontSize(10);
        col.Item().Height(8);
        col.Item().Row(row =>
        {
            row.RelativeItem().Column(left =>
            {
                left.Item().Text(t => { t.Span("Họ tên người xin cấp vật tư : ").Bold(); t.Span(phieu.NguoiNhan); });
                left.Item().Text(t => { t.Span("Bộ Phận: ").Bold(); t.Span(phieu.BoPhan?.TenBoPhan ?? ""); });
                left.Item().Text(t => { t.Span("Nơi nhận: ").Bold(); t.Span(phieu.NoiNhan); });
            });
            row.RelativeItem().Column(right =>
            {
                right.Item().Text(t => { t.Span("Ngày tháng xuất: ").Bold(); t.Span(phieu.NgayXuat.ToString("dd/MM/yyyy")); });
                right.Item().Text(t => { t.Span("Mục đích sử dụng: ").Bold(); t.Span(phieu.MucDichSuDung); });
            });
        });
        col.Item().Height(8);
        col.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(30); columns.RelativeColumn(4); columns.RelativeColumn(0.8f);
                columns.RelativeColumn(1.2f); columns.RelativeColumn(2); columns.RelativeColumn(1.5f); columns.RelativeColumn(1);
            });
            table.Header(header =>
            {
                void HC(IContainer c, string text) => c.Border(0.5f).Background("#E0E0E0").Padding(3).AlignCenter().AlignMiddle().Text(text).Bold().FontSize(8);
                HC(header.Cell(), "STT"); HC(header.Cell(), "Tên mặt hàng"); HC(header.Cell(), "Đvt");
                HC(header.Cell(), "Số\nlượng"); HC(header.Cell(), "Mục đích sử dụng"); HC(header.Cell(), "Nơi nhận"); HC(header.Cell(), "Mã vật tư");
            });
            int stt = 1;
            foreach (var ct in phieu.ChiTietPhieuXuats)
            {
                table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(stt.ToString()).FontSize(9);
                table.Cell().Border(0.5f).Padding(3).Text(ct.VatTu?.TenVatTu ?? "").FontSize(9);
                table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(ct.VatTu?.DonViTinh?.TenDonVi ?? "").FontSize(9);
                table.Cell().Border(0.5f).Padding(3).AlignRight().Text(ct.SoLuong.ToString("N2")).FontSize(9);
                table.Cell().Border(0.5f).Padding(3).Text(phieu.MucDichSuDung).FontSize(8);
                table.Cell().Border(0.5f).Padding(3).Text(phieu.NoiNhan).FontSize(8);
                table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(ct.VatTu?.MaVatTu ?? "").FontSize(8);
                stt++;
            }
            table.Cell().ColumnSpan(3).Border(0.5f).Padding(3).AlignRight().Text("Tổng cộng:").Bold().FontSize(9);
            table.Cell().Border(0.5f).Padding(3).AlignRight().Text(phieu.ChiTietPhieuXuats.Sum(c => c.SoLuong).ToString("N2")).Bold().FontSize(9);
            table.Cell().ColumnSpan(3).Border(0.5f).Padding(3).Text("").FontSize(8);
        });
        col.Item().Height(15);
        col.Item().Row(row =>
        {
            void SignBlock(IContainer c, string title) { c.AlignCenter().Column(sc => { sc.Item().AlignCenter().Text(title).Bold().FontSize(9); sc.Item().AlignCenter().Text("(ký ,họ tên)").Italic().FontSize(7); sc.Item().Height(50); }); }
            row.RelativeItem().Element(c => SignBlock(c, "Thủ kho"));
            row.RelativeItem().Element(c => SignBlock(c, "Người nhận hàng"));
            row.RelativeItem().Element(c => SignBlock(c, "Kế toán"));
            row.RelativeItem().Element(c => SignBlock(c, "Giám đốc"));
        });
    }

    public Task ExportDeNghiCapVatTu(DeNghiCapVatTu phieu, string filePath)
    {
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(1.2f, Unit.Centimetre);
                page.MarginVertical(1f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));
                page.Content().Column(col => RenderDeNghiCapVatTuContent(col, phieu));
            });
        }).GeneratePdf(filePath);
        return Task.CompletedTask;
    }

    private void RenderDeNghiCapVatTuContent(ColumnDescriptor col, DeNghiCapVatTu phieu)
    {
        col.Item().Row(row =>
        {
            row.RelativeItem().Column(left => { left.Item().AlignCenter().Text("CÔNG TY CP TẬP ĐOÀN").Bold().FontSize(11); left.Item().AlignCenter().Text("SÔNG HỒNG THỦ ĐÔ").Bold().FontSize(11); });
            row.RelativeItem().Column(right => { right.Item().AlignCenter().Text("CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM").Bold().FontSize(11); right.Item().AlignCenter().Text("Độc lập - Tự do - Hạnh phúc").FontSize(10).Underline(); });
        });
        col.Item().Height(6);
        col.Item().AlignRight().PaddingRight(30).Text($"......, ngày {phieu.NgayDeNghi:dd} tháng {phieu.NgayDeNghi:MM} năm {phieu.NgayDeNghi:yyyy}").Italic().FontSize(10);
        col.Item().Height(15);
        col.Item().AlignCenter().Text("GIẤY ĐỀ NGHỊ CẤP VẬT TƯ").Bold().FontSize(18);
        col.Item().Height(15);
        col.Item().Row(row =>
        {
            row.RelativeItem(1).Text(t => { t.Span("Họ tên: ").FontSize(10); t.Span(phieu.NguoiDeNghi).FontSize(10); });
            row.RelativeItem(1).Text(t => { t.Span("Chức vụ: ").FontSize(10); t.Span(phieu.ChucVu).FontSize(10); });
        });
        col.Item().Row(row =>
        {
            row.RelativeItem(1).Text(t => { t.Span("Bộ phận: ").FontSize(10); t.Span(phieu.BoPhan?.TenBoPhan ?? "").FontSize(10); });
            row.RelativeItem(1).Text(t => { t.Span("Số điện thoại: ").FontSize(10); });
        });
        col.Item().Height(4);
        col.Item().Text("Xin cấp các loại vật tư sau:").FontSize(10);
        col.Item().Height(8);
        col.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(28); columns.RelativeColumn(3.5f); columns.RelativeColumn(0.8f);
                columns.RelativeColumn(0.9f); columns.RelativeColumn(0.9f); columns.RelativeColumn(0.9f); columns.RelativeColumn(1.2f);
            });
            table.Header(header =>
            {
                void HC(IContainer c, string text) => c.Border(0.5f).Background("#F3F4F6").Padding(4).AlignCenter().AlignMiddle().Text(text).Bold().FontSize(9);
                HC(header.Cell(), "STT"); HC(header.Cell(), "Tên vật tư (Quy cách, mẫu\nmã, thông số kỹ thuật...)");
                HC(header.Cell(), "ĐVT"); HC(header.Cell(), "SL\ntồn\nkho"); HC(header.Cell(), "SL\nđề\nnghị"); HC(header.Cell(), "SL\nđược\nduyệt"); HC(header.Cell(), "Ghi\nchú");
            });
            int stt = 1;
            foreach (var ct in phieu.ChiTietDeNghis)
            {
                table.Cell().Border(0.5f).Padding(4).AlignCenter().Text(stt.ToString()).FontSize(9);
                table.Cell().Border(0.5f).Padding(4).Text(ct.VatTu?.TenVatTu ?? "").FontSize(9);
                table.Cell().Border(0.5f).Padding(4).AlignCenter().Text(ct.VatTu?.DonViTinh?.TenDonVi ?? "").FontSize(9);
                table.Cell().Border(0.5f).Padding(4).AlignCenter().Text("").FontSize(9);
                table.Cell().Border(0.5f).Padding(4).AlignCenter().Text(ct.SoLuongYeuCau.ToString("N0")).FontSize(9);
                table.Cell().Border(0.5f).Padding(4).AlignCenter().Text(ct.SoLuongDaCap > 0 ? ct.SoLuongDaCap.ToString("N0") : "").FontSize(9);
                table.Cell().Border(0.5f).Padding(4).Text(ct.GhiChu).FontSize(9);
                stt++;
            }
            for (int i = 0; i < 2; i++) { for (int j = 0; j < 7; j++) table.Cell().Border(0.5f).Padding(4).Height(j == 0 ? 22 : 0).Text(""); }
        });
        col.Item().Height(8);
        col.Item().Text($"(Tổng: {phieu.ChiTietDeNghis.Count:D2} loại. Để sử dụng: {phieu.GhiChu})").Italic().FontSize(9);
        col.Item().Height(20);
        col.Item().Row(row =>
        {
            void SignBlock(IContainer c, string title) { c.AlignCenter().Column(sc => { sc.Item().AlignCenter().Text(title).Bold().FontSize(10); sc.Item().Height(60); }); }
            row.RelativeItem().Element(c => SignBlock(c, "Người duyệt"));
            row.RelativeItem().Element(c => SignBlock(c, "Phòng TC-HC"));
            row.RelativeItem().Element(c => SignBlock(c, "CB phụ trách"));
            row.RelativeItem().Element(c => SignBlock(c, "Người đề nghị"));
        });
    }

    public async Task ExportNhapXuatTonKho(AppDbContext context, string filePath, int khoId, int month, int year)
    {
        var khos = khoId > 0
            ? new List<Kho> { (await context.Khos.FindAsync(khoId))! }
            : await context.Khos.ToListAsync();
        var startOfMonth = new DateTime(year, month, 1);
        var endOfMonth = startOfMonth.AddMonths(1);
        var vatTus = await context.VatTus.Include(v => v.DonViTinh).OrderBy(v => v.MaVatTu).ToListAsync();

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

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.MarginHorizontal(1.5f, Unit.Centimetre);
                page.MarginVertical(1f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Content().Column(col =>
                {
                    // Header công ty
                    col.Item().AlignCenter().Text(CompanyName).Bold().FontSize(11);
                    col.Item().AlignCenter().Text("Khu đô thị Bắc Đầm Vạc").Italic().FontSize(9);
                    col.Item().Height(10);

                    col.Item().AlignCenter().Text($"NHẬP XUẤT TỒN KHO THÁNG {month}/{year}").Bold().FontSize(16);
                    col.Item().AlignCenter().Text($"Kho: {(khoId > 0 ? khos[0].TenKho : "Tất cả")}").FontSize(11);
                    col.Item().Height(10);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);   // STT
                            columns.RelativeColumn(1);    // Mã VT
                            columns.RelativeColumn(3);    // Tên vật tư
                            columns.RelativeColumn(0.7f); // ĐVT
                            columns.RelativeColumn(1);    // Tồn đầu kỳ
                            columns.RelativeColumn(1);    // Nhập trong kỳ
                            columns.RelativeColumn(1);    // Xuất trong kỳ
                            columns.RelativeColumn(1);    // Tồn cuối kỳ
                            columns.RelativeColumn(1.2f); // Kho
                        });

                        // Dòng TỔNG CỘNG
                        table.Header(header =>
                        {
                            void HC(IContainer c, string text) =>
                                c.Border(0.5f).Background("#E0E0E0").Padding(3).AlignCenter().AlignMiddle()
                                 .Text(text).Bold().FontSize(9);

                            // Tổng cộng row
                            header.Cell().ColumnSpan(4).Border(0.5f).Background("#E8F0FE").Padding(3).AlignRight().Text("TỔNG CỘNG").Bold().FontSize(9);
                            header.Cell().Border(0.5f).Background("#E8F0FE").Padding(3).AlignRight().Text(rows.Sum(r => r.TonDau).ToString("N2")).Bold().FontSize(9);
                            header.Cell().Border(0.5f).Background("#E8F0FE").Padding(3).AlignRight().Text(rows.Sum(r => r.Nhap).ToString("N2")).Bold().FontSize(9).FontColor("#16A34A");
                            header.Cell().Border(0.5f).Background("#E8F0FE").Padding(3).AlignRight().Text(rows.Sum(r => r.Xuat).ToString("N2")).Bold().FontSize(9).FontColor("#DC2626");
                            header.Cell().Border(0.5f).Background("#E8F0FE").Padding(3).AlignRight().Text(rows.Sum(r => r.TonCK).ToString("N2")).Bold().FontSize(9).FontColor("#2563EB");
                            header.Cell().Border(0.5f).Background("#E8F0FE").Padding(3).Text("").FontSize(9);

                            // Header row
                            HC(header.Cell(), "STT");
                            HC(header.Cell(), "Mã VT");
                            HC(header.Cell(), "Tên vật tư");
                            HC(header.Cell(), "ĐVT");
                            HC(header.Cell(), "Tồn đầu kỳ");
                            HC(header.Cell(), "Nhập trong kỳ");
                            HC(header.Cell(), "Xuất trong kỳ");
                            HC(header.Cell(), "Tồn cuối kỳ");
                            HC(header.Cell(), "Kho");
                        });

                        int stt = 1;
                        foreach (var r in rows)
                        {
                            table.Cell().Border(0.5f).Padding(2).AlignCenter().Text(stt.ToString());
                            table.Cell().Border(0.5f).Padding(2).AlignCenter().Text(r.MaVT);
                            table.Cell().Border(0.5f).Padding(2).Text(r.TenVT);
                            table.Cell().Border(0.5f).Padding(2).AlignCenter().Text(r.DVT);
                            table.Cell().Border(0.5f).Padding(2).AlignRight().Text(r.TonDau.ToString("N2"));
                            table.Cell().Border(0.5f).Padding(2).AlignRight().Text(r.Nhap.ToString("N2"));
                            table.Cell().Border(0.5f).Padding(2).AlignRight().Text(r.Xuat.ToString("N2"));
                            table.Cell().Border(0.5f).Padding(2).AlignRight().Text(r.TonCK.ToString("N2"));
                            table.Cell().Border(0.5f).Padding(2).Text(r.Kho);
                            stt++;
                        }
                    });
                });
            });
        }).GeneratePdf(filePath);
    }

    public async Task ExportTonKho(AppDbContext context, string filePath)
    {
        var vatTus = await context.VatTus.Include(v => v.DonViTinh).Include(v => v.NhomVatTu).ToListAsync();
        var khos = await context.Khos.ToListAsync();

        var data = new List<(string Kho, string MaVT, string TenVT, string DVT, decimal Nhap, decimal Xuat, decimal Ton)>();

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
                    data.Add((kho.TenKho, vt.MaVatTu, vt.TenVatTu, vt.DonViTinh?.TenDonVi ?? "", nhap, xuat, nhap - xuat));
            }
        }

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.MarginHorizontal(1.5f, Unit.Centimetre);
                page.MarginVertical(1f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Content().Column(col =>
                {
                    col.Item().AlignCenter().Text(CompanyName).Bold().FontSize(11);
                    col.Item().Height(5);
                    col.Item().AlignCenter().Text("BÁO CÁO TỒN KHO").Bold().FontSize(14);
                    col.Item().AlignCenter().Text($"Ngày xuất: {DateTime.Now:dd/MM/yyyy}");
                    col.Item().Height(10);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1.2f);
                        });

                        table.Header(header =>
                        {
                            void HC(IContainer c, string text) =>
                                c.Border(0.5f).Background("#E0E0E0").Padding(2).AlignCenter().Text(text).Bold();

                            HC(header.Cell(), "STT");
                            HC(header.Cell(), "Kho");
                            HC(header.Cell(), "Mã VT");
                            HC(header.Cell(), "Tên vật tư");
                            HC(header.Cell(), "ĐVT");
                            HC(header.Cell(), "SL Nhập");
                            HC(header.Cell(), "SL Xuất");
                            HC(header.Cell(), "Tồn");
                        });

                        int stt = 1;
                        foreach (var d in data.OrderBy(x => x.Kho).ThenBy(x => x.MaVT))
                        {
                            table.Cell().Border(0.5f).Padding(2).AlignCenter().Text(stt.ToString());
                            table.Cell().Border(0.5f).Padding(2).Text(d.Kho);
                            table.Cell().Border(0.5f).Padding(2).Text(d.MaVT);
                            table.Cell().Border(0.5f).Padding(2).Text(d.TenVT);
                            table.Cell().Border(0.5f).Padding(2).AlignCenter().Text(d.DVT);
                            table.Cell().Border(0.5f).Padding(2).AlignRight().Text(d.Nhap.ToString("N2"));
                            table.Cell().Border(0.5f).Padding(2).AlignRight().Text(d.Xuat.ToString("N2"));
                            table.Cell().Border(0.5f).Padding(2).AlignRight().Text(d.Ton.ToString("N2"));
                            stt++;
                        }
                    });
                });
            });
        }).GeneratePdf(filePath);
    }

    public async Task ExportDanhSachNhapKho(AppDbContext context, string filePath, DateTime from, DateTime to)
    {
        var phieus = await context.PhieuNhapKhos
            .Include(p => p.Kho)
            .Where(p => p.NgayNhap >= from && p.NgayNhap <= to.AddDays(1))
            .OrderByDescending(p => p.NgayNhap)
            .ToListAsync();

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.MarginHorizontal(1.5f, Unit.Centimetre);
                page.MarginVertical(1f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Content().Column(col =>
                {
                    col.Item().AlignCenter().Text(CompanyName).Bold().FontSize(11);
                    col.Item().Height(5);
                    col.Item().AlignCenter().Text("DANH SÁCH PHIẾU NHẬP KHO").Bold().FontSize(14);
                    col.Item().AlignCenter().Text($"Từ {from:dd/MM/yyyy} đến {to:dd/MM/yyyy}");
                    col.Item().Height(10);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1.5f);
                        });

                        table.Header(header =>
                        {
                            void HC(IContainer c, string text) =>
                                c.Border(0.5f).Background("#E0E0E0").Padding(2).AlignCenter().Text(text).Bold();
                            HC(header.Cell(), "STT");
                            HC(header.Cell(), "Số phiếu");
                            HC(header.Cell(), "Ngày nhập");
                            HC(header.Cell(), "Người giao");
                            HC(header.Cell(), "Kho");
                            HC(header.Cell(), "Tổng tiền");
                        });

                        int stt = 1;
                        foreach (var p in phieus)
                        {
                            table.Cell().Border(0.5f).Padding(2).AlignCenter().Text(stt.ToString());
                            table.Cell().Border(0.5f).Padding(2).Text(p.SoPhieu);
                            table.Cell().Border(0.5f).Padding(2).Text(p.NgayNhap.ToString("dd/MM/yyyy"));
                            table.Cell().Border(0.5f).Padding(2).Text(p.NguoiGiaoHang);
                            table.Cell().Border(0.5f).Padding(2).Text(p.Kho?.TenKho ?? "");
                            table.Cell().Border(0.5f).Padding(2).AlignRight().Text(p.TongTien.ToString("N0"));
                            stt++;
                        }
                    });
                });
            });
        }).GeneratePdf(filePath);
    }

    public async Task ExportDanhSachXuatKho(AppDbContext context, string filePath, DateTime from, DateTime to)
    {
        var phieus = await context.PhieuXuatKhos
            .Include(p => p.Kho).Include(p => p.BoPhan)
            .Where(p => p.NgayXuat >= from && p.NgayXuat <= to.AddDays(1))
            .OrderByDescending(p => p.NgayXuat)
            .ToListAsync();

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.MarginHorizontal(1.5f, Unit.Centimetre);
                page.MarginVertical(1f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Content().Column(col =>
                {
                    col.Item().AlignCenter().Text(CompanyName).Bold().FontSize(11);
                    col.Item().Height(5);
                    col.Item().AlignCenter().Text("DANH SÁCH PHIẾU XUẤT KHO").Bold().FontSize(14);
                    col.Item().AlignCenter().Text($"Từ {from:dd/MM/yyyy} đến {to:dd/MM/yyyy}");
                    col.Item().Height(10);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1.5f);
                        });

                        table.Header(header =>
                        {
                            void HC(IContainer c, string text) =>
                                c.Border(0.5f).Background("#E0E0E0").Padding(2).AlignCenter().Text(text).Bold();
                            HC(header.Cell(), "STT");
                            HC(header.Cell(), "Số phiếu");
                            HC(header.Cell(), "Ngày xuất");
                            HC(header.Cell(), "Người nhận");
                            HC(header.Cell(), "Kho");
                            HC(header.Cell(), "Tổng tiền");
                        });

                        int stt = 1;
                        foreach (var p in phieus)
                        {
                            table.Cell().Border(0.5f).Padding(2).AlignCenter().Text(stt.ToString());
                            table.Cell().Border(0.5f).Padding(2).Text(p.SoPhieu);
                            table.Cell().Border(0.5f).Padding(2).Text(p.NgayXuat.ToString("dd/MM/yyyy"));
                            table.Cell().Border(0.5f).Padding(2).Text(p.NguoiNhan);
                            table.Cell().Border(0.5f).Padding(2).Text(p.Kho?.TenKho ?? "");
                            table.Cell().Border(0.5f).Padding(2).AlignRight().Text(p.TongTien.ToString("N0"));
                            stt++;
                        }
                    });
                });
            });
        }).GeneratePdf(filePath);
    }

    // ===== BATCH EXPORT - Gộp nhiều phiếu vào 1 file PDF (mỗi phiếu 1 trang) =====

    public Task ExportMultiPhieuNhapKho(List<PhieuNhapKho> phieus, string filePath)
    {
        Document.Create(container =>
        {
            foreach (var phieu in phieus)
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.MarginHorizontal(1.5f, Unit.Centimetre);
                    page.MarginVertical(1.2f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));
                    page.Content().Column(col => RenderPhieuNhapKhoContent(col, phieu));
                });
            }
        }).GeneratePdf(filePath);
        return Task.CompletedTask;
    }

    public Task ExportMultiPhieuXuatKho(List<PhieuXuatKho> phieus, string filePath)
    {
        Document.Create(container =>
        {
            foreach (var phieu in phieus)
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.MarginHorizontal(1.5f, Unit.Centimetre);
                    page.MarginVertical(1.2f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));
                    page.Content().Column(col => RenderPhieuXuatKhoContent(col, phieu));
                });
            }
        }).GeneratePdf(filePath);
        return Task.CompletedTask;
    }

    public Task ExportMultiDeNghiCapVatTu(List<DeNghiCapVatTu> phieus, string filePath)
    {
        Document.Create(container =>
        {
            foreach (var phieu in phieus)
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.MarginHorizontal(1.2f, Unit.Centimetre);
                    page.MarginVertical(1f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));
                    page.Content().Column(col => RenderDeNghiCapVatTuContent(col, phieu));
                });
            }
        }).GeneratePdf(filePath);
        return Task.CompletedTask;
    }
}
