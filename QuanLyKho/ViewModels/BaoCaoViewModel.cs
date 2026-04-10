using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using QuanLyKho.Data;
using QuanLyKho.Models;
using QuanLyKho.Services;

namespace QuanLyKho.ViewModels;

// Wrapper có checkbox cho mỗi loại phiếu
public partial class SelectablePhieuNhap : ObservableObject
{
    [ObservableProperty] private bool _isSelected;
    public PhieuNhapKho Phieu { get; set; } = null!;
}

public partial class SelectablePhieuXuat : ObservableObject
{
    [ObservableProperty] private bool _isSelected;
    public PhieuXuatKho Phieu { get; set; } = null!;
}

public partial class SelectableDeNghi : ObservableObject
{
    [ObservableProperty] private bool _isSelected;
    public DeNghiCapVatTu Phieu { get; set; } = null!;
}

public partial class BaoCaoViewModel : ObservableObject
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IPdfExportService _pdfService;
    private readonly IExcelExportService _excelService;

    [ObservableProperty] private int _selectedTab;
    [ObservableProperty] private string _statusMessage = "";

    // --- Tồn kho ---
    [ObservableProperty] private int _selectedMonth = DateTime.Now.Month;
    [ObservableProperty] private int _selectedYear = DateTime.Now.Year;
    [ObservableProperty] private Kho? _selectedKho;
    [ObservableProperty] private ObservableCollection<Kho> _khos = new();

    // --- Phiếu nhập ---
    [ObservableProperty] private ObservableCollection<SelectablePhieuNhap> _phieuNhaps = new();

    // --- Phiếu xuất ---
    [ObservableProperty] private ObservableCollection<SelectablePhieuXuat> _phieuXuats = new();

    // --- Đề nghị ---
    [ObservableProperty] private ObservableCollection<SelectableDeNghi> _deNghis = new();

    public int[] Months { get; } = Enumerable.Range(1, 12).ToArray();
    public int[] Years { get; } = Enumerable.Range(2020, 20).ToArray();

    public BaoCaoViewModel(IDbContextFactory<AppDbContext> contextFactory, IPdfExportService pdfService, IExcelExportService excelService)
    {
        _contextFactory = contextFactory;
        _pdfService = pdfService;
        _excelService = excelService;
        LoadDataCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task LoadData()
    {
        try
        {
            StatusMessage = "";
            using var context = await _contextFactory.CreateDbContextAsync();

            Khos = new ObservableCollection<Kho>(await context.Khos.ToListAsync());
            if (SelectedKho == null) SelectedKho = Khos.FirstOrDefault();

            PhieuNhaps = new ObservableCollection<SelectablePhieuNhap>(
                (await context.PhieuNhapKhos.Include(p => p.Kho).OrderByDescending(p => p.NgayNhap).ToListAsync())
                .Select(p => new SelectablePhieuNhap { Phieu = p }));

            PhieuXuats = new ObservableCollection<SelectablePhieuXuat>(
                (await context.PhieuXuatKhos.Include(p => p.Kho).OrderByDescending(p => p.NgayXuat).ToListAsync())
                .Select(p => new SelectablePhieuXuat { Phieu = p }));

            DeNghis = new ObservableCollection<SelectableDeNghi>(
                (await context.DeNghiCapVatTus.Include(p => p.BoPhan).OrderByDescending(p => p.NgayDeNghi).ToListAsync())
                .Select(p => new SelectableDeNghi { Phieu = p }));
        }
        catch (Exception ex)
        {
            StatusMessage = $"Lỗi tải dữ liệu: {ex.Message}";
        }
    }

    // === TỒN KHO ===
    [RelayCommand]
    private async Task ExportTonKhoPdf()
    {
        if (SelectedKho == null) { StatusMessage = "Vui lòng chọn kho!"; return; }
        try
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF (*.pdf)|*.pdf",
                FileName = $"NhapXuatTon_{SelectedKho.MaKho}_T{SelectedMonth}_{SelectedYear}.pdf"
            };
            if (dialog.ShowDialog() != true) return;

            using var context = await _contextFactory.CreateDbContextAsync();
            await _pdfService.ExportNhapXuatTonKho(context, dialog.FileName, SelectedKho.Id, SelectedMonth, SelectedYear);
            StatusMessage = $"Đã xuất thành công!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Lỗi xuất PDF: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ExportTonKhoExcel()
    {
        if (SelectedKho == null) { StatusMessage = "Vui lòng chọn kho!"; return; }
        try
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel (*.xlsx)|*.xlsx",
                FileName = $"NhapXuatTon_{SelectedKho.MaKho}_T{SelectedMonth}_{SelectedYear}.xlsx"
            };
            if (dialog.ShowDialog() != true) return;

            using var context = await _contextFactory.CreateDbContextAsync();
            await _excelService.ExportNhapXuatTonKho(context, dialog.FileName, SelectedKho.Id, SelectedMonth, SelectedYear);
            StatusMessage = $"Đã xuất thành công!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Lỗi xuất Excel: {ex.Message}";
        }
    }

    // === PHIẾU NHẬP (nhiều phiếu) ===
    [RelayCommand]
    private async Task ExportPhieuNhapPdf()
    {
        var selected = PhieuNhaps.Where(x => x.IsSelected).ToList();
        if (selected.Count == 0) { StatusMessage = "Vui lòng tích chọn ít nhất 1 phiếu!"; return; }

        try
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF (*.pdf)|*.pdf",
                FileName = selected.Count == 1
                    ? $"PhieuNhap_{selected[0].Phieu.SoPhieu}.pdf"
                    : $"PhieuNhap_{selected.Count}_phieu.pdf"
            };
            if (dialog.ShowDialog() != true) return;

            using var context = await _contextFactory.CreateDbContextAsync();
            foreach (var item in selected)
            {
                var phieu = await context.PhieuNhapKhos
                    .Include(p => p.Kho)
                    .Include(p => p.ChiTietPhieuNhaps).ThenInclude(ct => ct.VatTu).ThenInclude(v => v.DonViTinh)
                    .FirstAsync(p => p.Id == item.Phieu.Id);

                var path = selected.Count == 1
                    ? dialog.FileName
                    : dialog.FileName.Replace(".pdf", $"_{phieu.SoPhieu}.pdf");
                await _pdfService.ExportPhieuNhapKho(phieu, path);
            }
            StatusMessage = $"Đã xuất {selected.Count} phiếu nhập thành công!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Lỗi xuất PDF: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ExportPhieuNhapExcel()
    {
        var selected = PhieuNhaps.Where(x => x.IsSelected).ToList();
        if (selected.Count == 0) { StatusMessage = "Vui lòng tích chọn ít nhất 1 phiếu!"; return; }

        try
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel (*.xlsx)|*.xlsx",
                FileName = selected.Count == 1
                    ? $"PhieuNhap_{selected[0].Phieu.SoPhieu}.xlsx"
                    : $"PhieuNhap_{selected.Count}_phieu.xlsx"
            };
            if (dialog.ShowDialog() != true) return;

            using var context = await _contextFactory.CreateDbContextAsync();
            foreach (var item in selected)
            {
                var phieu = await context.PhieuNhapKhos
                    .Include(p => p.Kho)
                    .Include(p => p.ChiTietPhieuNhaps).ThenInclude(ct => ct.VatTu).ThenInclude(v => v.DonViTinh)
                    .FirstAsync(p => p.Id == item.Phieu.Id);

                var path = selected.Count == 1
                    ? dialog.FileName
                    : dialog.FileName.Replace(".xlsx", $"_{phieu.SoPhieu}.xlsx");
                await _excelService.ExportPhieuNhapKho(phieu, path);
            }
            StatusMessage = $"Đã xuất {selected.Count} phiếu nhập thành công!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Lỗi xuất Excel: {ex.Message}";
        }
    }

    // === PHIẾU XUẤT (nhiều phiếu) ===
    [RelayCommand]
    private async Task ExportPhieuXuatPdf()
    {
        var selected = PhieuXuats.Where(x => x.IsSelected).ToList();
        if (selected.Count == 0) { StatusMessage = "Vui lòng tích chọn ít nhất 1 phiếu!"; return; }

        try
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF (*.pdf)|*.pdf",
                FileName = selected.Count == 1
                    ? $"PhieuXuat_{selected[0].Phieu.SoPhieu}.pdf"
                    : $"PhieuXuat_{selected.Count}_phieu.pdf"
            };
            if (dialog.ShowDialog() != true) return;

            using var context = await _contextFactory.CreateDbContextAsync();
            foreach (var item in selected)
            {
                var phieu = await context.PhieuXuatKhos
                    .Include(p => p.Kho).Include(p => p.BoPhan)
                    .Include(p => p.ChiTietPhieuXuats).ThenInclude(ct => ct.VatTu).ThenInclude(v => v.DonViTinh)
                    .FirstAsync(p => p.Id == item.Phieu.Id);

                var path = selected.Count == 1
                    ? dialog.FileName
                    : dialog.FileName.Replace(".pdf", $"_{phieu.SoPhieu}.pdf");
                await _pdfService.ExportPhieuXuatKho(phieu, path);
            }
            StatusMessage = $"Đã xuất {selected.Count} phiếu xuất thành công!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Lỗi xuất PDF: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ExportPhieuXuatExcel()
    {
        var selected = PhieuXuats.Where(x => x.IsSelected).ToList();
        if (selected.Count == 0) { StatusMessage = "Vui lòng tích chọn ít nhất 1 phiếu!"; return; }

        try
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel (*.xlsx)|*.xlsx",
                FileName = selected.Count == 1
                    ? $"PhieuXuat_{selected[0].Phieu.SoPhieu}.xlsx"
                    : $"PhieuXuat_{selected.Count}_phieu.xlsx"
            };
            if (dialog.ShowDialog() != true) return;

            using var context = await _contextFactory.CreateDbContextAsync();
            foreach (var item in selected)
            {
                var phieu = await context.PhieuXuatKhos
                    .Include(p => p.Kho).Include(p => p.BoPhan)
                    .Include(p => p.ChiTietPhieuXuats).ThenInclude(ct => ct.VatTu).ThenInclude(v => v.DonViTinh)
                    .FirstAsync(p => p.Id == item.Phieu.Id);

                var path = selected.Count == 1
                    ? dialog.FileName
                    : dialog.FileName.Replace(".xlsx", $"_{phieu.SoPhieu}.xlsx");
                await _excelService.ExportPhieuXuatKho(phieu, path);
            }
            StatusMessage = $"Đã xuất {selected.Count} phiếu xuất thành công!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Lỗi xuất Excel: {ex.Message}";
        }
    }

    // === ĐỀ NGHỊ CẤP VT (nhiều phiếu) ===
    [RelayCommand]
    private async Task ExportDeNghiPdf()
    {
        var selected = DeNghis.Where(x => x.IsSelected).ToList();
        if (selected.Count == 0) { StatusMessage = "Vui lòng tích chọn ít nhất 1 phiếu!"; return; }

        try
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF (*.pdf)|*.pdf",
                FileName = selected.Count == 1
                    ? $"DeNghi_{selected[0].Phieu.SoPhieu}.pdf"
                    : $"DeNghi_{selected.Count}_phieu.pdf"
            };
            if (dialog.ShowDialog() != true) return;

            using var context = await _contextFactory.CreateDbContextAsync();
            foreach (var item in selected)
            {
                var phieu = await context.DeNghiCapVatTus
                    .Include(p => p.BoPhan)
                    .Include(p => p.ChiTietDeNghis).ThenInclude(ct => ct.VatTu).ThenInclude(v => v.DonViTinh)
                    .FirstAsync(p => p.Id == item.Phieu.Id);

                var path = selected.Count == 1
                    ? dialog.FileName
                    : dialog.FileName.Replace(".pdf", $"_{phieu.SoPhieu}.pdf");
                await _pdfService.ExportDeNghiCapVatTu(phieu, path);
            }
            StatusMessage = $"Đã xuất {selected.Count} phiếu đề nghị thành công!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Lỗi xuất PDF: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ExportDeNghiExcel()
    {
        var selected = DeNghis.Where(x => x.IsSelected).ToList();
        if (selected.Count == 0) { StatusMessage = "Vui lòng tích chọn ít nhất 1 phiếu!"; return; }

        try
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel (*.xlsx)|*.xlsx",
                FileName = selected.Count == 1
                    ? $"DeNghi_{selected[0].Phieu.SoPhieu}.xlsx"
                    : $"DeNghi_{selected.Count}_phieu.xlsx"
            };
            if (dialog.ShowDialog() != true) return;

            using var context = await _contextFactory.CreateDbContextAsync();
            foreach (var item in selected)
            {
                var phieu = await context.DeNghiCapVatTus
                    .Include(p => p.BoPhan)
                    .Include(p => p.ChiTietDeNghis).ThenInclude(ct => ct.VatTu).ThenInclude(v => v.DonViTinh)
                    .FirstAsync(p => p.Id == item.Phieu.Id);

                var path = selected.Count == 1
                    ? dialog.FileName
                    : dialog.FileName.Replace(".xlsx", $"_{phieu.SoPhieu}.xlsx");
                await _excelService.ExportDeNghiCapVatTu(phieu, path);
            }
            StatusMessage = $"Đã xuất {selected.Count} phiếu đề nghị thành công!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Lỗi xuất Excel: {ex.Message}";
        }
    }
}
