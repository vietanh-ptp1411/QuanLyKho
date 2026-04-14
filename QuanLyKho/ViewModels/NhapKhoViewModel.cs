using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using QuanLyKho.Data;
using QuanLyKho.Helpers;
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

    private List<PhieuNhapKho> _allItems = new();
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _totalPages = 1;
    [ObservableProperty] private int _totalCount;
    [ObservableProperty] private int _pageSize = 15;

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

            _allItems = await query.OrderByDescending(p => p.NgayNhap).ToListAsync();
            CurrentPage = 1;
            ApplyPaging();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi tải dữ liệu: {ex.Message}";
        }
    }

    partial void OnSearchTextChanged(string value) => LoadDataCommand.ExecuteAsync(null);

    [RelayCommand]
    private void NextPage() { if (CurrentPage < TotalPages) { CurrentPage++; ApplyPaging(); } }
    [RelayCommand]
    private void PrevPage() { if (CurrentPage > 1) { CurrentPage--; ApplyPaging(); } }
    [RelayCommand]
    private void FirstPage() { CurrentPage = 1; ApplyPaging(); }
    [RelayCommand]
    private void LastPage() { CurrentPage = TotalPages; ApplyPaging(); }

    private void ApplyPaging()
    {
        var paged = _allItems.Skip((CurrentPage - 1) * PageSize).Take(PageSize);
        DanhSach = new ObservableCollection<PhieuNhapKho>(paged);
        TotalPages = Math.Max(1, (int)Math.Ceiling((double)_allItems.Count / PageSize));
        TotalCount = _allItems.Count;
    }

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

    private bool _isSaving;

    [RelayCommand]
    private async Task Save()
    {
        if (_isSaving) return; // chặn double-click

        // ── Validation ──────────────────────────────────────────────────────────
        if (EditKho == null)
        {
            ErrorMessage = "Vui lòng chọn kho nhập.";
            return;
        }
        if (string.IsNullOrWhiteSpace(EditSoPhieu))
        {
            ErrorMessage = "Vui lòng điền số phiếu.";
            return;
        }

        var validRows = ChiTietRows.Where(r => r.VatTu != null).ToList();
        if (validRows.Count == 0)
        {
            ErrorMessage = "Vui lòng thêm ít nhất một vật tư vào phiếu.";
            return;
        }

        var rowSLZero = validRows.FirstOrDefault(r => r.SoLuong <= 0);
        if (rowSLZero != null)
        {
            ErrorMessage = $"Số lượng của \"{rowSLZero.VatTu!.TenVatTu}\" phải lớn hơn 0.";
            return;
        }

        var rowGiaAm = validRows.FirstOrDefault(r => r.DonGia < 0);
        if (rowGiaAm != null)
        {
            ErrorMessage = $"Đơn giá của \"{rowGiaAm.VatTu!.TenVatTu}\" không được âm.";
            return;
        }
        // ────────────────────────────────────────────────────────────────────────

        _isSaving = true;
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

            phieu.SoPhieu        = EditSoPhieu.Trim();
            phieu.NgayNhap       = EditNgayNhap;
            phieu.NguoiGiaoHang  = EditNguoiGiaoHang.Trim();
            phieu.KhoId          = EditKho.Id;
            phieu.NguoiLapPhieu  = EditNguoiLapPhieu.Trim();
            phieu.ThuKho         = EditThuKho.Trim();
            phieu.KeToanTruong   = EditKeToanTruong.Trim();
            phieu.GiamDoc        = EditGiamDoc.Trim();
            phieu.GhiChu         = EditGhiChu.Trim();
            phieu.TongTien       = TongTien;

            foreach (var row in validRows)
            {
                phieu.ChiTietPhieuNhaps.Add(new ChiTietPhieuNhap
                {
                    VatTuId    = row.VatTu!.Id,
                    SoLuong    = row.SoLuong,
                    DonGia     = row.DonGia,
                    ThanhTien  = row.ThanhTien
                });
            }

            await context.SaveChangesAsync();
            IsEditing = false;
            await LoadData();
        }
        catch (DbUpdateException dbEx)
        {
            ErrorMessage = DbExceptionHelper.GetMessage(dbEx);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi lưu phiếu: {ex.Message}";
        }
        finally
        {
            _isSaving = false;
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
        var confirm = System.Windows.MessageBox.Show(
            $"Bạn có chắc muốn xóa phiếu nhập \"{SelectedPhieu.SoPhieu}\" không?\nThao tác này không thể hoàn tác.",
            "Xác nhận xóa",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);
        if (confirm != System.Windows.MessageBoxResult.Yes) return;
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
        catch (DbUpdateException dbEx)
        {
            ErrorMessage = DbExceptionHelper.GetMessage(dbEx);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi xóa phiếu: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task EditPhieuItem(PhieuNhapKho? item)
    {
        if (item == null) return;
        SelectedPhieu = item;
        await EditPhieu();
    }

    [RelayCommand]
    private async Task DeletePhieuItem(PhieuNhapKho? item)
    {
        if (item == null) return;
        SelectedPhieu = item;
        await DeletePhieu();
    }

    [RelayCommand]
    private void Filter() => LoadDataCommand.ExecuteAsync(null);

    [RelayCommand]
    private async Task Print()
    {
        if (SelectedPhieu == null) return;
        try
        {
            ErrorMessage = "";
            var tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"PhieuNhap_{SelectedPhieu.SoPhieu}_{DateTime.Now:HHmmss}.pdf");

            using var context = await _contextFactory.CreateDbContextAsync();
            var phieu = await context.PhieuNhapKhos
                .Include(p => p.Kho)
                .Include(p => p.ChiTietPhieuNhaps).ThenInclude(ct => ct.VatTu).ThenInclude(v => v.DonViTinh)
                .FirstOrDefaultAsync(p => p.Id == SelectedPhieu.Id);
            if (phieu == null) return;

            await _pdfService.ExportPhieuNhapKho(phieu, tempPath);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = tempPath,
                Verb = "open",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi in phiếu: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ExportPdf()
    {
        if (SelectedPhieu == null) return;

        try
        {
            ErrorMessage = "";
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF (*.pdf)|*.pdf",
                FileName = $"PhieuNhapKho_{SelectedPhieu.SoPhieu}.pdf"
            };
            if (dialog.ShowDialog() != true) return;

            using var context = await _contextFactory.CreateDbContextAsync();
            var phieu = await context.PhieuNhapKhos
                .Include(p => p.Kho)
                .Include(p => p.ChiTietPhieuNhaps).ThenInclude(ct => ct.VatTu).ThenInclude(v => v.DonViTinh)
                .FirstOrDefaultAsync(p => p.Id == SelectedPhieu.Id);
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
        if (SelectedPhieu == null) return;

        try
        {
            ErrorMessage = "";
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel (*.xlsx)|*.xlsx",
                FileName = $"PhieuNhapKho_{SelectedPhieu.SoPhieu}.xlsx"
            };
            if (dialog.ShowDialog() != true) return;

            using var context = await _contextFactory.CreateDbContextAsync();
            var phieu = await context.PhieuNhapKhos
                .Include(p => p.Kho)
                .Include(p => p.ChiTietPhieuNhaps).ThenInclude(ct => ct.VatTu).ThenInclude(v => v.DonViTinh)
                .FirstOrDefaultAsync(p => p.Id == SelectedPhieu.Id);
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
