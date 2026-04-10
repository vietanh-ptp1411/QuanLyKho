using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using QuanLyKho.Data;
using QuanLyKho.Models;
using QuanLyKho.Services;

namespace QuanLyKho.ViewModels;

public partial class NhapKhoViewModel : ObservableObject
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IPdfExportService _pdfService;
    private readonly IExcelExportService _excelService;

    // List view
    [ObservableProperty] private ObservableCollection<PhieuNhapKho> _danhSach = new();
    [ObservableProperty] private PhieuNhapKho? _selectedPhieu;
    [ObservableProperty] private string _searchText = "";
    [ObservableProperty] private DateTime? _filterFromDate;
    [ObservableProperty] private DateTime? _filterToDate;
    [ObservableProperty] private string _errorMessage = "";

    // Form view
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private bool _isNew;
    [ObservableProperty] private string _editSoPhieu = "";
    [ObservableProperty] private DateTime _editNgayNhap = DateTime.Now;
    [ObservableProperty] private string _editNguoiGiaoHang = "";
    [ObservableProperty] private Kho? _editKho;
    [ObservableProperty] private string _editNguoiLapPhieu = "";
    [ObservableProperty] private string _editThuKho = "";
    [ObservableProperty] private string _editKeToanTruong = "";
    [ObservableProperty] private string _editGiamDoc = "";
    [ObservableProperty] private string _editGhiChu = "";
    [ObservableProperty] private ObservableCollection<ChiTietNhapRow> _chiTietRows = new();
    [ObservableProperty] private decimal _tongTien;

    // Lookups
    [ObservableProperty] private ObservableCollection<Kho> _khos = new();
    [ObservableProperty] private ObservableCollection<VatTu> _vatTus = new();

    public NhapKhoViewModel(IDbContextFactory<AppDbContext> contextFactory, IPdfExportService pdfService, IExcelExportService excelService)
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
            ErrorMessage = "";
            using var context = await _contextFactory.CreateDbContextAsync();

            Khos = new ObservableCollection<Kho>(await context.Khos.ToListAsync());
            VatTus = new ObservableCollection<VatTu>(await context.VatTus.Include(v => v.DonViTinh).ToListAsync());

            var query = context.PhieuNhapKhos.Include(p => p.Kho).AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var s = SearchText.Trim().ToLower();
                query = query.Where(p => p.SoPhieu.ToLower().Contains(s) || p.NguoiGiaoHang.ToLower().Contains(s));
            }
            if (FilterFromDate.HasValue)
                query = query.Where(p => p.NgayNhap >= FilterFromDate.Value);
            if (FilterToDate.HasValue)
                query = query.Where(p => p.NgayNhap <= FilterToDate.Value.AddDays(1));

            DanhSach = new ObservableCollection<PhieuNhapKho>(
                await query.OrderByDescending(p => p.NgayNhap).ToListAsync());
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi tải dữ liệu: {ex.Message}";
        }
    }

    partial void OnSearchTextChanged(string value) => LoadDataCommand.ExecuteAsync(null);

    [RelayCommand]
    private async Task AddNew()
    {
        try
        {
            ErrorMessage = "";
            IsNew = true;
            IsEditing = true;
            EditNgayNhap = DateTime.Now;
            EditNguoiGiaoHang = "";
            EditKho = Khos.FirstOrDefault();
            EditNguoiLapPhieu = "";
            EditThuKho = "";
            EditKeToanTruong = "";
            EditGiamDoc = "";
            EditGhiChu = "";
            ChiTietRows = new ObservableCollection<ChiTietNhapRow>();
            TongTien = 0;

            // Auto-generate SoPhieu
            using var context = await _contextFactory.CreateDbContextAsync();
            var today = DateTime.Now.ToString("yyyyMMdd");
            var prefix = $"NK-{today}-";
            var lastPhieu = await context.PhieuNhapKhos
                .Where(p => p.SoPhieu.StartsWith(prefix))
                .OrderByDescending(p => p.SoPhieu)
                .FirstOrDefaultAsync();

            int nextNum = 1;
            if (lastPhieu != null)
            {
                var lastNumStr = lastPhieu.SoPhieu.Replace(prefix, "");
                if (int.TryParse(lastNumStr, out int lastNum)) nextNum = lastNum + 1;
            }
            EditSoPhieu = $"{prefix}{nextNum:D3}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi tạo phiếu mới: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task EditPhieu()
    {
        if (SelectedPhieu == null) return;
        try
        {
            ErrorMessage = "";
            IsNew = false;
            IsEditing = true;

            using var context = await _contextFactory.CreateDbContextAsync();
            var phieu = await context.PhieuNhapKhos
                .Include(p => p.ChiTietPhieuNhaps).ThenInclude(ct => ct.VatTu).ThenInclude(v => v.DonViTinh)
                .Include(p => p.Kho)
                .FirstOrDefaultAsync(p => p.Id == SelectedPhieu.Id);

            if (phieu == null) return;

            EditSoPhieu = phieu.SoPhieu;
            EditNgayNhap = phieu.NgayNhap;
            EditNguoiGiaoHang = phieu.NguoiGiaoHang;
            EditKho = Khos.FirstOrDefault(k => k.Id == phieu.KhoId);
            EditNguoiLapPhieu = phieu.NguoiLapPhieu;
            EditThuKho = phieu.ThuKho;
            EditKeToanTruong = phieu.KeToanTruong;
            EditGiamDoc = phieu.GiamDoc;
            EditGhiChu = phieu.GhiChu;

            ChiTietRows = new ObservableCollection<ChiTietNhapRow>(
                phieu.ChiTietPhieuNhaps.Select(ct => new ChiTietNhapRow(this)
                {
                    VatTu = VatTus.FirstOrDefault(v => v.Id == ct.VatTuId),
                    SoLuong = ct.SoLuong,
                    DonGia = ct.DonGia,
                    ThanhTien = ct.ThanhTien
                }));
            RecalcTongTien();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi mở phiếu: {ex.Message}";
        }
    }

    [RelayCommand]
    private void AddRow()
    {
        ChiTietRows.Add(new ChiTietNhapRow(this));
    }

    [RelayCommand]
    private void RemoveRow(ChiTietNhapRow row)
    {
        ChiTietRows.Remove(row);
        RecalcTongTien();
    }

    public void RecalcTongTien()
    {
        TongTien = ChiTietRows.Sum(r => r.ThanhTien);
    }

    [RelayCommand]
    private async Task Save()
    {
        if (EditKho == null || string.IsNullOrWhiteSpace(EditSoPhieu))
        {
            ErrorMessage = "Vui lòng chọn kho và điền số phiếu.";
            return;
        }

        try
        {
            ErrorMessage = "";
            using var context = await _contextFactory.CreateDbContextAsync();

            PhieuNhapKho phieu;
            if (IsNew)
            {
                phieu = new PhieuNhapKho();
                context.PhieuNhapKhos.Add(phieu);
            }
            else
            {
                phieu = await context.PhieuNhapKhos
                    .Include(p => p.ChiTietPhieuNhaps)
                    .FirstAsync(p => p.Id == SelectedPhieu!.Id);
                phieu.ChiTietPhieuNhaps.Clear();
            }

            phieu.SoPhieu = EditSoPhieu;
            phieu.NgayNhap = EditNgayNhap;
            phieu.NguoiGiaoHang = EditNguoiGiaoHang;
            phieu.KhoId = EditKho.Id;
            phieu.NguoiLapPhieu = EditNguoiLapPhieu;
            phieu.ThuKho = EditThuKho;
            phieu.KeToanTruong = EditKeToanTruong;
            phieu.GiamDoc = EditGiamDoc;
            phieu.GhiChu = EditGhiChu;
            phieu.TongTien = TongTien;

            foreach (var row in ChiTietRows.Where(r => r.VatTu != null))
            {
                phieu.ChiTietPhieuNhaps.Add(new ChiTietPhieuNhap
                {
                    VatTuId = row.VatTu!.Id,
                    SoLuong = row.SoLuong,
                    DonGia = row.DonGia,
                    ThanhTien = row.ThanhTien
                });
            }

            await context.SaveChangesAsync();
            IsEditing = false;
            await LoadData();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi lưu phiếu: {ex.Message}";
        }
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
        ErrorMessage = "";
    }

    [RelayCommand]
    private async Task DeletePhieu()
    {
        if (SelectedPhieu == null) return;
        try
        {
            ErrorMessage = "";
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.PhieuNhapKhos.FindAsync(SelectedPhieu.Id);
            if (entity != null)
            {
                context.PhieuNhapKhos.Remove(entity);
                await context.SaveChangesAsync();
            }
            await LoadData();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi xóa phiếu: {ex.Message}";
        }
    }

    [RelayCommand]
    private void Filter() => LoadDataCommand.ExecuteAsync(null);

    [RelayCommand]
    private async Task ExportPdf()
    {
        if (SelectedPhieu == null && !IsEditing) return;
        var id = IsEditing && !IsNew ? SelectedPhieu!.Id : SelectedPhieu?.Id;
        if (id == null) return;

        try
        {
            ErrorMessage = "";
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF (*.pdf)|*.pdf",
                FileName = $"PhieuNhapKho_{EditSoPhieu}.pdf"
            };
            if (dialog.ShowDialog() != true) return;

            using var context = await _contextFactory.CreateDbContextAsync();
            var phieu = await context.PhieuNhapKhos
                .Include(p => p.Kho)
                .Include(p => p.ChiTietPhieuNhaps).ThenInclude(ct => ct.VatTu).ThenInclude(v => v.DonViTinh)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (phieu != null) await _pdfService.ExportPhieuNhapKho(phieu, dialog.FileName);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi xuất PDF: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ExportExcel()
    {
        if (SelectedPhieu == null && !IsEditing) return;
        var id = IsEditing && !IsNew ? SelectedPhieu!.Id : SelectedPhieu?.Id;
        if (id == null) return;

        try
        {
            ErrorMessage = "";
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel (*.xlsx)|*.xlsx",
                FileName = $"PhieuNhapKho_{EditSoPhieu}.xlsx"
            };
            if (dialog.ShowDialog() != true) return;

            using var context = await _contextFactory.CreateDbContextAsync();
            var phieu = await context.PhieuNhapKhos
                .Include(p => p.Kho)
                .Include(p => p.ChiTietPhieuNhaps).ThenInclude(ct => ct.VatTu).ThenInclude(v => v.DonViTinh)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (phieu != null) await _excelService.ExportPhieuNhapKho(phieu, dialog.FileName);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi xuất Excel: {ex.Message}";
        }
    }
}

public partial class ChiTietNhapRow : ObservableObject
{
    private readonly NhapKhoViewModel _parent;

    [ObservableProperty] private VatTu? _vatTu;
    [ObservableProperty] private decimal _soLuong;
    [ObservableProperty] private decimal _donGia;
    [ObservableProperty] private decimal _thanhTien;

    public ChiTietNhapRow(NhapKhoViewModel parent)
    {
        _parent = parent;
    }

    partial void OnSoLuongChanged(decimal value)
    {
        ThanhTien = value * DonGia;
        _parent.RecalcTongTien();
    }

    partial void OnDonGiaChanged(decimal value)
    {
        ThanhTien = SoLuong * value;
        _parent.RecalcTongTien();
    }
}
