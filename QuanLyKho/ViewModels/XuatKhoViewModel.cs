using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using QuanLyKho.Data;
using QuanLyKho.Helpers;
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

    private List<PhieuXuatKho> _allItems = new();
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _totalPages = 1;
    [ObservableProperty] private int _totalCount;
    [ObservableProperty] private int _pageSize = 15;
    [ObservableProperty] private bool _isAllSelected;

    partial void OnIsAllSelectedChanged(bool value)
    {
        foreach (var item in DanhSach) item.IsSelected = value;
    }

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
    [ObservableProperty] private decimal _tongSoLuong;
    [ObservableProperty] private string _errorMessage = "";

    [ObservableProperty] private ObservableCollection<Kho> _khos = new();
    [ObservableProperty] private ObservableCollection<BoPhan> _boPhans = new();
    [ObservableProperty] private ObservableCollection<VatTu> _vatTus = new();

    private List<VatTu> _allVatTus = new();

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
            _allVatTus = await context.VatTus.Include(v => v.DonViTinh).ToListAsync();
            VatTus = new ObservableCollection<VatTu>(_allVatTus);

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

            _allItems = await query.OrderByDescending(p => p.NgayXuat).ToListAsync();
            CurrentPage = 1;
            ApplyPaging();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi tải dữ liệu: {ex.Message}";
        }
    }

    partial void OnSearchTextChanged(string value) => LoadDataCommand.ExecuteAsync(null);

    partial void OnEditKhoChanged(Kho? value) => _ = RefreshAvailableVatTusAsync();

    private async Task RefreshAvailableVatTusAsync()
    {
        try
        {
            if (EditKho == null)
            {
                VatTus = new ObservableCollection<VatTu>(_allVatTus);
                return;
            }

            using var context = await _contextFactory.CreateDbContextAsync();
            int khoId = EditKho.Id;

            var nhapByVatTu = await context.ChiTietPhieuNhaps
                .Where(ct => ct.PhieuNhapKho.KhoId == khoId)
                .GroupBy(ct => ct.VatTuId)
                .Select(g => new { VatTuId = g.Key, SoLuong = g.Sum(x => x.SoLuong) })
                .ToDictionaryAsync(x => x.VatTuId, x => x.SoLuong);

            var xuatByVatTu = await context.ChiTietPhieuXuats
                .Where(ct => ct.PhieuXuatKho.KhoId == khoId)
                .GroupBy(ct => ct.VatTuId)
                .Select(g => new { VatTuId = g.Key, SoLuong = g.Sum(x => x.SoLuong) })
                .ToDictionaryAsync(x => x.VatTuId, x => x.SoLuong);

            var availableIds = nhapByVatTu.Keys
                .Where(id => nhapByVatTu[id] - (xuatByVatTu.TryGetValue(id, out var xv) ? xv : 0m) > 0)
                .ToHashSet();

            foreach (var row in ChiTietRows)
                if (row.VatTu != null) availableIds.Add(row.VatTu.Id);

            VatTus = new ObservableCollection<VatTu>(_allVatTus.Where(v => availableIds.Contains(v.Id)));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi tải danh sách vật tư: {ex.Message}";
        }
    }

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
        DanhSach = new ObservableCollection<PhieuXuatKho>(paged);
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
                    VatTu = _allVatTus.FirstOrDefault(v => v.Id == ct.VatTuId),
                    SoLuong = ct.SoLuong,
                    DonGia = ct.DonGia,
                    ThanhTien = ct.ThanhTien
                }));
            RecalcTongTien();
            await RefreshAvailableVatTusAsync();
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

    public void RecalcTongTien()
    {
        TongTien = ChiTietRows.Sum(r => r.ThanhTien);
        TongSoLuong = ChiTietRows.Sum(r => r.SoLuong);
    }

    private bool _isSaving;

    [RelayCommand]
    private async Task Save()
    {
        if (_isSaving) return;

        // ── Validation ──────────────────────────────────────────────────────────
        if (EditKho == null)
        {
            ErrorMessage = "Vui lòng chọn kho xuất.";
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
        // ────────────────────────────────────────────────────────────────────────

        _isSaving = true;
        try
        {
            ErrorMessage = "";
            using var context = await _contextFactory.CreateDbContextAsync();

            // Kiểm tra tồn kho trước khi xuất
            foreach (var row in validRows)
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

            phieu.SoPhieu        = EditSoPhieu.Trim();
            phieu.NgayXuat       = EditNgayXuat;
            phieu.NguoiNhan      = EditNguoiNhan.Trim();
            phieu.KhoId          = EditKho.Id;
            phieu.BoPhanId       = EditBoPhan?.Id;
            phieu.NoiNhan        = EditNoiNhan.Trim();
            phieu.MucDichSuDung  = EditMucDichSuDung.Trim();
            phieu.NguoiLapPhieu  = EditNguoiLapPhieu.Trim();
            phieu.ThuKho         = EditThuKho.Trim();
            phieu.KeToanTruong   = EditKeToanTruong.Trim();
            phieu.GiamDoc        = EditGiamDoc.Trim();
            phieu.GhiChu         = EditGhiChu.Trim();
            phieu.TongTien       = TongTien;

            foreach (var row in validRows)
            {
                phieu.ChiTietPhieuXuats.Add(new ChiTietPhieuXuat
                {
                    VatTuId   = row.VatTu!.Id,
                    SoLuong   = row.SoLuong,
                    DonGia    = row.DonGia,
                    ThanhTien = row.ThanhTien
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
            $"Bạn có chắc muốn xóa phiếu xuất \"{SelectedPhieu.SoPhieu}\" không?\nThao tác này không thể hoàn tác.",
            "Xác nhận xóa",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);
        if (confirm != System.Windows.MessageBoxResult.Yes) return;
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
    private async Task EditPhieuItem(PhieuXuatKho? item)
    {
        if (item == null) return;
        SelectedPhieu = item;
        await EditPhieu();
    }

    [RelayCommand]
    private async Task DeletePhieuItem(PhieuXuatKho? item)
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
        var selectedIds = DanhSach.Where(p => p.IsSelected).Select(p => p.Id).ToList();
        if (selectedIds.Count == 0 && SelectedPhieu != null) selectedIds.Add(SelectedPhieu.Id);
        if (selectedIds.Count == 0) { ErrorMessage = "Vui lòng chọn ít nhất một phiếu để in."; return; }
        try
        {
            ErrorMessage = "";
            var tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"PhieuXuat_{DateTime.Now:HHmmss}.pdf");

            using var context = await _contextFactory.CreateDbContextAsync();
            var phieus = await context.PhieuXuatKhos
                .Include(p => p.Kho).Include(p => p.BoPhan)
                .Include(p => p.ChiTietPhieuXuats).ThenInclude(ct => ct.VatTu).ThenInclude(v => v.DonViTinh)
                .Where(p => selectedIds.Contains(p.Id)).OrderBy(p => p.NgayXuat).ToListAsync();
            if (phieus.Count == 0) return;

            if (phieus.Count == 1)
                await _pdfService.ExportPhieuXuatKho(phieus[0], tempPath);
            else
                await _pdfService.ExportMultiPhieuXuatKho(phieus, tempPath);

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
        var selectedIds = DanhSach.Where(p => p.IsSelected).Select(p => p.Id).ToList();
        if (selectedIds.Count == 0 && SelectedPhieu != null) selectedIds.Add(SelectedPhieu.Id);
        if (selectedIds.Count == 0) { ErrorMessage = "Vui lòng chọn ít nhất một phiếu để xuất."; return; }
        try
        {
            ErrorMessage = "";
            var name = selectedIds.Count == 1 ? $"PhieuXuatKho_{selectedIds[0]}" : $"PhieuXuatKho_Gop_{DateTime.Now:yyyyMMdd}";
            var dialog = new Microsoft.Win32.SaveFileDialog { Filter = "PDF (*.pdf)|*.pdf", FileName = $"{name}.pdf" };
            if (dialog.ShowDialog() != true) return;
            using var context = await _contextFactory.CreateDbContextAsync();
            var phieus = await context.PhieuXuatKhos.Include(p => p.Kho).Include(p => p.BoPhan)
                .Include(p => p.ChiTietPhieuXuats).ThenInclude(ct => ct.VatTu).ThenInclude(v => v.DonViTinh)
                .Where(p => selectedIds.Contains(p.Id)).OrderBy(p => p.NgayXuat).ToListAsync();
            if (phieus.Count == 1)
                await _pdfService.ExportPhieuXuatKho(phieus[0], dialog.FileName);
            else
                await _pdfService.ExportMultiPhieuXuatKho(phieus, dialog.FileName);
        }
        catch (Exception ex) { ErrorMessage = $"Lỗi xuất PDF: {ex.Message}"; }
    }

    [RelayCommand]
    private async Task ExportExcel()
    {
        var selectedIds = DanhSach.Where(p => p.IsSelected).Select(p => p.Id).ToList();
        if (selectedIds.Count == 0 && SelectedPhieu != null) selectedIds.Add(SelectedPhieu.Id);
        if (selectedIds.Count == 0) { ErrorMessage = "Vui lòng chọn ít nhất một phiếu để xuất."; return; }
        try
        {
            ErrorMessage = "";
            var name = selectedIds.Count == 1 ? $"PhieuXuatKho_{selectedIds[0]}" : $"PhieuXuatKho_Gop_{DateTime.Now:yyyyMMdd}";
            var dialog = new Microsoft.Win32.SaveFileDialog { Filter = "Excel (*.xlsx)|*.xlsx", FileName = $"{name}.xlsx" };
            if (dialog.ShowDialog() != true) return;
            using var context = await _contextFactory.CreateDbContextAsync();
            var phieus = await context.PhieuXuatKhos.Include(p => p.Kho).Include(p => p.BoPhan)
                .Include(p => p.ChiTietPhieuXuats).ThenInclude(ct => ct.VatTu).ThenInclude(v => v.DonViTinh)
                .Where(p => selectedIds.Contains(p.Id)).OrderBy(p => p.NgayXuat).ToListAsync();
            if (phieus.Count == 1)
                await _excelService.ExportPhieuXuatKho(phieus[0], dialog.FileName);
            else
                await _excelService.ExportMultiPhieuXuatKho(phieus, dialog.FileName);
        }
        catch (Exception ex) { ErrorMessage = $"Lỗi xuất Excel: {ex.Message}"; }
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
