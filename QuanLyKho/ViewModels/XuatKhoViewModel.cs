using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using QuanLyKho.Data;
using QuanLyKho.Models;
using QuanLyKho.Services;

namespace QuanLyKho.ViewModels;

public partial class XuatKhoViewModel : ObservableObject
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IPdfExportService _pdfService;
    private readonly IExcelExportService _excelService;

    [ObservableProperty] private ObservableCollection<PhieuXuatKho> _danhSach = new();
    [ObservableProperty] private PhieuXuatKho? _selectedPhieu;
    [ObservableProperty] private string _searchText = "";
    [ObservableProperty] private DateTime? _filterFromDate;
    [ObservableProperty] private DateTime? _filterToDate;

    // Form
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private bool _isNew;
    [ObservableProperty] private string _editSoPhieu = "";
    [ObservableProperty] private DateTime _editNgayXuat = DateTime.Now;
    [ObservableProperty] private string _editNguoiNhan = "";
    [ObservableProperty] private Kho? _editKho;
    [ObservableProperty] private BoPhan? _editBoPhan;
    [ObservableProperty] private string _editNoiNhan = "";
    [ObservableProperty] private string _editMucDichSuDung = "";
    [ObservableProperty] private string _editNguoiLapPhieu = "";
    [ObservableProperty] private string _editThuKho = "";
    [ObservableProperty] private string _editKeToanTruong = "";
    [ObservableProperty] private string _editGiamDoc = "";
    [ObservableProperty] private string _editGhiChu = "";
    [ObservableProperty] private ObservableCollection<ChiTietXuatRow> _chiTietRows = new();
    [ObservableProperty] private decimal _tongTien;
    [ObservableProperty] private string _errorMessage = "";

    [ObservableProperty] private ObservableCollection<Kho> _khos = new();
    [ObservableProperty] private ObservableCollection<BoPhan> _boPhans = new();
    [ObservableProperty] private ObservableCollection<VatTu> _vatTus = new();

    public XuatKhoViewModel(IDbContextFactory<AppDbContext> contextFactory, IPdfExportService pdfService, IExcelExportService excelService)
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
            BoPhans = new ObservableCollection<BoPhan>(await context.BoPhans.ToListAsync());
            VatTus = new ObservableCollection<VatTu>(await context.VatTus.Include(v => v.DonViTinh).ToListAsync());

            var query = context.PhieuXuatKhos.Include(p => p.Kho).Include(p => p.BoPhan).AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var s = SearchText.Trim().ToLower();
                query = query.Where(p => p.SoPhieu.ToLower().Contains(s) || p.NguoiNhan.ToLower().Contains(s));
            }
            if (FilterFromDate.HasValue)
                query = query.Where(p => p.NgayXuat >= FilterFromDate.Value);
            if (FilterToDate.HasValue)
                query = query.Where(p => p.NgayXuat <= FilterToDate.Value.AddDays(1));

            DanhSach = new ObservableCollection<PhieuXuatKho>(
                await query.OrderByDescending(p => p.NgayXuat).ToListAsync());
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
            EditNgayXuat = DateTime.Now;
            EditNguoiNhan = "";
            EditKho = Khos.FirstOrDefault();
            EditBoPhan = null;
            EditNoiNhan = "";
            EditMucDichSuDung = "";
            EditNguoiLapPhieu = "";
            EditThuKho = "";
            EditKeToanTruong = "";
            EditGiamDoc = "";
            EditGhiChu = "";
            ChiTietRows = new ObservableCollection<ChiTietXuatRow>();
            TongTien = 0;

            using var context = await _contextFactory.CreateDbContextAsync();
            var today = DateTime.Now.ToString("yyyyMMdd");
            var prefix = $"XK-{today}-";
            var lastPhieu = await context.PhieuXuatKhos
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
            var phieu = await context.PhieuXuatKhos
                .Include(p => p.ChiTietPhieuXuats).ThenInclude(ct => ct.VatTu).ThenInclude(v => v.DonViTinh)
                .Include(p => p.Kho).Include(p => p.BoPhan)
                .FirstOrDefaultAsync(p => p.Id == SelectedPhieu.Id);

            if (phieu == null) return;

            EditSoPhieu = phieu.SoPhieu;
            EditNgayXuat = phieu.NgayXuat;
            EditNguoiNhan = phieu.NguoiNhan;
            EditKho = Khos.FirstOrDefault(k => k.Id == phieu.KhoId);
            EditBoPhan = BoPhans.FirstOrDefault(b => b.Id == phieu.BoPhanId);
            EditNoiNhan = phieu.NoiNhan;
            EditMucDichSuDung = phieu.MucDichSuDung;
            EditNguoiLapPhieu = phieu.NguoiLapPhieu;
            EditThuKho = phieu.ThuKho;
            EditKeToanTruong = phieu.KeToanTruong;
            EditGiamDoc = phieu.GiamDoc;
            EditGhiChu = phieu.GhiChu;

            ChiTietRows = new ObservableCollection<ChiTietXuatRow>(
                phieu.ChiTietPhieuXuats.Select(ct => new ChiTietXuatRow(this)
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
    private void AddRow() => ChiTietRows.Add(new ChiTietXuatRow(this));

    [RelayCommand]
    private void RemoveRow(ChiTietXuatRow row)
    {
        ChiTietRows.Remove(row);
        RecalcTongTien();
    }

    public void RecalcTongTien() => TongTien = ChiTietRows.Sum(r => r.ThanhTien);

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

            // Validate stock
            foreach (var row in ChiTietRows.Where(r => r.VatTu != null))
            {
                var nhap = await context.ChiTietPhieuNhaps
                    .Where(ct => ct.VatTuId == row.VatTu!.Id && ct.PhieuNhapKho.KhoId == EditKho.Id)
                    .SumAsync(ct => (decimal?)ct.SoLuong) ?? 0;
                var xuat = await context.ChiTietPhieuXuats
                    .Where(ct => ct.VatTuId == row.VatTu!.Id && ct.PhieuXuatKho.KhoId == EditKho.Id)
                    .SumAsync(ct => (decimal?)ct.SoLuong) ?? 0;

                // If editing, add back the old export quantities
                if (!IsNew && SelectedPhieu != null)
                {
                    var oldQty = await context.ChiTietPhieuXuats
                        .Where(ct => ct.PhieuXuatKhoId == SelectedPhieu.Id && ct.VatTuId == row.VatTu!.Id)
                        .SumAsync(ct => (decimal?)ct.SoLuong) ?? 0;
                    xuat -= oldQty;
                }

                var tonKho = nhap - xuat;
                if (row.SoLuong > tonKho)
                {
                    ErrorMessage = $"Vật tư \"{row.VatTu!.TenVatTu}\" tồn kho chỉ còn {tonKho:N2}, không đủ xuất {row.SoLuong:N2}";
                    return;
                }
            }

            PhieuXuatKho phieu;
            if (IsNew)
            {
                phieu = new PhieuXuatKho();
                context.PhieuXuatKhos.Add(phieu);
            }
            else
            {
                phieu = await context.PhieuXuatKhos
                    .Include(p => p.ChiTietPhieuXuats)
                    .FirstAsync(p => p.Id == SelectedPhieu!.Id);
                phieu.ChiTietPhieuXuats.Clear();
            }

            phieu.SoPhieu = EditSoPhieu;
            phieu.NgayXuat = EditNgayXuat;
            phieu.NguoiNhan = EditNguoiNhan;
            phieu.KhoId = EditKho.Id;
            phieu.BoPhanId = EditBoPhan?.Id;
            phieu.NoiNhan = EditNoiNhan;
            phieu.MucDichSuDung = EditMucDichSuDung;
            phieu.NguoiLapPhieu = EditNguoiLapPhieu;
            phieu.ThuKho = EditThuKho;
            phieu.KeToanTruong = EditKeToanTruong;
            phieu.GiamDoc = EditGiamDoc;
            phieu.GhiChu = EditGhiChu;
            phieu.TongTien = TongTien;

            foreach (var row in ChiTietRows.Where(r => r.VatTu != null))
            {
                phieu.ChiTietPhieuXuats.Add(new ChiTietPhieuXuat
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
            var entity = await context.PhieuXuatKhos.FindAsync(SelectedPhieu.Id);
            if (entity != null)
            {
                context.PhieuXuatKhos.Remove(entity);
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
                FileName = $"PhieuXuatKho_{EditSoPhieu}.pdf"
            };
            if (dialog.ShowDialog() != true) return;

            using var context = await _contextFactory.CreateDbContextAsync();
            var phieu = await context.PhieuXuatKhos
                .Include(p => p.Kho).Include(p => p.BoPhan)
                .Include(p => p.ChiTietPhieuXuats).ThenInclude(ct => ct.VatTu).ThenInclude(v => v.DonViTinh)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (phieu != null) await _pdfService.ExportPhieuXuatKho(phieu, dialog.FileName);
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
                FileName = $"PhieuXuatKho_{EditSoPhieu}.xlsx"
            };
            if (dialog.ShowDialog() != true) return;

            using var context = await _contextFactory.CreateDbContextAsync();
            var phieu = await context.PhieuXuatKhos
                .Include(p => p.Kho).Include(p => p.BoPhan)
                .Include(p => p.ChiTietPhieuXuats).ThenInclude(ct => ct.VatTu).ThenInclude(v => v.DonViTinh)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (phieu != null) await _excelService.ExportPhieuXuatKho(phieu, dialog.FileName);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi xuất Excel: {ex.Message}";
        }
    }
}

public partial class ChiTietXuatRow : ObservableObject
{
    private readonly XuatKhoViewModel _parent;

    [ObservableProperty] private VatTu? _vatTu;
    [ObservableProperty] private decimal _soLuong;
    [ObservableProperty] private decimal _donGia;
    [ObservableProperty] private decimal _thanhTien;

    public ChiTietXuatRow(XuatKhoViewModel parent) => _parent = parent;

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
