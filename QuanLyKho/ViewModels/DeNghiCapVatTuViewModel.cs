using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using QuanLyKho.Data;
using QuanLyKho.Helpers;
using QuanLyKho.Models;
using QuanLyKho.Services;

namespace QuanLyKho.ViewModels;

public partial class DeNghiCapVatTuViewModel : ObservableObject
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IPdfExportService _pdfService;
    private readonly IExcelExportService _excelService;

    [ObservableProperty] private ObservableCollection<DeNghiCapVatTu> _danhSach = new();
    [ObservableProperty] private DeNghiCapVatTu? _selectedPhieu;
    [ObservableProperty] private int _filterTrangThai = -1; // -1 = all
    [ObservableProperty] private string _errorMessage = "";

    private List<DeNghiCapVatTu> _allItems = new();
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
    [ObservableProperty] private DateTime _editNgayDeNghi = DateTime.Now;
    [ObservableProperty] private string _editNguoiDeNghi = "";
    [ObservableProperty] private BoPhan? _editBoPhan;
    [ObservableProperty] private string _editChucVu = "";
    [ObservableProperty] private int _editTrangThai;
    [ObservableProperty] private string _editGhiChu = "";
    [ObservableProperty] private ObservableCollection<ChiTietDeNghiRow> _chiTietRows = new();

    [ObservableProperty] private ObservableCollection<BoPhan> _boPhans = new();
    [ObservableProperty] private ObservableCollection<VatTu> _vatTus = new();

    public string[] TrangThaiLabels { get; } = { "Nháp", "Đã duyệt", "Đã cấp", "Từ chối" };

    public DeNghiCapVatTuViewModel(IDbContextFactory<AppDbContext> contextFactory, IPdfExportService pdfService, IExcelExportService excelService)
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

            BoPhans = new ObservableCollection<BoPhan>(await context.BoPhans.ToListAsync());
            VatTus = new ObservableCollection<VatTu>(await context.VatTus.Include(v => v.DonViTinh).ToListAsync());

            var query = context.DeNghiCapVatTus.Include(p => p.BoPhan).AsQueryable();
            if (FilterTrangThai >= 0)
                query = query.Where(p => p.TrangThai == FilterTrangThai);

            _allItems = await query.OrderByDescending(p => p.NgayDeNghi).ToListAsync();
            CurrentPage = 1;
            ApplyPaging();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi tải dữ liệu: {ex.Message}";
        }
    }

    partial void OnFilterTrangThaiChanged(int value) => LoadDataCommand.ExecuteAsync(null);

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
        DanhSach = new ObservableCollection<DeNghiCapVatTu>(paged);
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
            EditNgayDeNghi = DateTime.Now;
            EditNguoiDeNghi = "";
            EditBoPhan = null;
            EditChucVu = "";
            EditTrangThai = 0;
            EditGhiChu = "";
            ChiTietRows = new ObservableCollection<ChiTietDeNghiRow>();

            using var context = await _contextFactory.CreateDbContextAsync();
            var today = DateTime.Now.ToString("yyyyMMdd");
            var prefix = $"DN-{today}-";
            var last = await context.DeNghiCapVatTus
                .Where(p => p.SoPhieu.StartsWith(prefix))
                .OrderByDescending(p => p.SoPhieu)
                .FirstOrDefaultAsync();
            int next = 1;
            if (last != null && int.TryParse(last.SoPhieu.Replace(prefix, ""), out int n)) next = n + 1;
            EditSoPhieu = $"{prefix}{next:D3}";
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
            var phieu = await context.DeNghiCapVatTus
                .Include(p => p.ChiTietDeNghis).ThenInclude(ct => ct.VatTu).ThenInclude(v => v.DonViTinh)
                .Include(p => p.BoPhan)
                .FirstOrDefaultAsync(p => p.Id == SelectedPhieu.Id);
            if (phieu == null) return;

            EditSoPhieu = phieu.SoPhieu;
            EditNgayDeNghi = phieu.NgayDeNghi;
            EditNguoiDeNghi = phieu.NguoiDeNghi;
            EditBoPhan = BoPhans.FirstOrDefault(b => b.Id == phieu.BoPhanId);
            EditChucVu = phieu.ChucVu;
            EditTrangThai = phieu.TrangThai;
            EditGhiChu = phieu.GhiChu;

            ChiTietRows = new ObservableCollection<ChiTietDeNghiRow>(
                phieu.ChiTietDeNghis.Select(ct => new ChiTietDeNghiRow
                {
                    VatTu = VatTus.FirstOrDefault(v => v.Id == ct.VatTuId),
                    SoLuongYeuCau = ct.SoLuongYeuCau,
                    SoLuongDaCap = ct.SoLuongDaCap,
                    GhiChu = ct.GhiChu
                }));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi mở phiếu: {ex.Message}";
        }
    }

    [RelayCommand]
    private void AddRow() => ChiTietRows.Add(new ChiTietDeNghiRow());

    [RelayCommand]
    private void RemoveRow(ChiTietDeNghiRow row) => ChiTietRows.Remove(row);

    private bool _isSaving;

    [RelayCommand]
    private async Task Save()
    {
        if (_isSaving) return;

        // ── Validation ──────────────────────────────────────────────────────────
        if (string.IsNullOrWhiteSpace(EditSoPhieu))
        {
            ErrorMessage = "Vui lòng điền số phiếu.";
            return;
        }
        if (string.IsNullOrWhiteSpace(EditNguoiDeNghi))
        {
            ErrorMessage = "Vui lòng điền người đề nghị.";
            return;
        }

        var validRows = ChiTietRows.Where(r => r.VatTu != null).ToList();
        if (validRows.Count == 0)
        {
            ErrorMessage = "Vui lòng thêm ít nhất một vật tư vào đề nghị.";
            return;
        }

        var rowSLZero = validRows.FirstOrDefault(r => r.SoLuongYeuCau <= 0);
        if (rowSLZero != null)
        {
            ErrorMessage = $"Số lượng yêu cầu của \"{rowSLZero.VatTu!.TenVatTu}\" phải lớn hơn 0.";
            return;
        }
        // ────────────────────────────────────────────────────────────────────────

        _isSaving = true;
        try
        {
            ErrorMessage = "";
            using var context = await _contextFactory.CreateDbContextAsync();

            DeNghiCapVatTu phieu;
            if (IsNew)
            {
                phieu = new DeNghiCapVatTu();
                context.DeNghiCapVatTus.Add(phieu);
            }
            else
            {
                phieu = await context.DeNghiCapVatTus
                    .Include(p => p.ChiTietDeNghis)
                    .FirstAsync(p => p.Id == SelectedPhieu!.Id);
                phieu.ChiTietDeNghis.Clear();
            }

            phieu.SoPhieu      = EditSoPhieu.Trim();
            phieu.NgayDeNghi   = EditNgayDeNghi;
            phieu.NguoiDeNghi  = EditNguoiDeNghi.Trim();
            phieu.BoPhanId     = EditBoPhan?.Id;
            phieu.ChucVu       = EditChucVu.Trim();
            phieu.TrangThai    = EditTrangThai;
            phieu.GhiChu       = EditGhiChu.Trim();

            foreach (var row in validRows)
            {
                phieu.ChiTietDeNghis.Add(new ChiTietDeNghi
                {
                    VatTuId        = row.VatTu!.Id,
                    SoLuongYeuCau  = row.SoLuongYeuCau,
                    SoLuongDaCap   = row.SoLuongDaCap,
                    GhiChu         = row.GhiChu
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
    private async Task Print()
    {
        var selectedIds = DanhSach.Where(p => p.IsSelected).Select(p => p.Id).ToList();
        if (selectedIds.Count == 0 && SelectedPhieu != null) selectedIds.Add(SelectedPhieu.Id);
        if (selectedIds.Count == 0) { ErrorMessage = "Vui lòng chọn ít nhất một phiếu để in."; return; }
        try
        {
            ErrorMessage = "";
            var tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"DeNghi_{DateTime.Now:HHmmss}.pdf");

            using var context = await _contextFactory.CreateDbContextAsync();
            var phieus = await context.DeNghiCapVatTus
                .Include(p => p.BoPhan)
                .Include(p => p.ChiTietDeNghis).ThenInclude(ct => ct.VatTu).ThenInclude(v => v.DonViTinh)
                .Where(p => selectedIds.Contains(p.Id)).OrderBy(p => p.NgayDeNghi).ToListAsync();
            if (phieus.Count == 0) return;

            if (phieus.Count == 1)
                await _pdfService.ExportDeNghiCapVatTu(phieus[0], tempPath);
            else
                await _pdfService.ExportMultiDeNghiCapVatTu(phieus, tempPath);

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
    private async Task DeletePhieu()
    {
        if (SelectedPhieu == null) return;
        var confirm = System.Windows.MessageBox.Show(
            $"Bạn có chắc muốn xóa đề nghị \"{SelectedPhieu.SoPhieu}\" không?\nThao tác này không thể hoàn tác.",
            "Xác nhận xóa",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);
        if (confirm != System.Windows.MessageBoxResult.Yes) return;
        try
        {
            ErrorMessage = "";
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.DeNghiCapVatTus.FindAsync(SelectedPhieu.Id);
            if (entity != null)
            {
                context.DeNghiCapVatTus.Remove(entity);
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
    private async Task EditPhieuItem(DeNghiCapVatTu? item)
    {
        if (item == null) return;
        SelectedPhieu = item;
        await EditPhieu();
    }

    [RelayCommand]
    private async Task DeletePhieuItem(DeNghiCapVatTu? item)
    {
        if (item == null) return;
        SelectedPhieu = item;
        await DeletePhieu();
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
            var name = selectedIds.Count == 1 ? $"DeNghi_{selectedIds[0]}" : $"DeNghi_Gop_{DateTime.Now:yyyyMMdd}";
            var dialog = new Microsoft.Win32.SaveFileDialog { Filter = "PDF (*.pdf)|*.pdf", FileName = $"{name}.pdf" };
            if (dialog.ShowDialog() != true) return;
            using var context = await _contextFactory.CreateDbContextAsync();
            var phieus = await context.DeNghiCapVatTus.Include(p => p.BoPhan)
                .Include(p => p.ChiTietDeNghis).ThenInclude(ct => ct.VatTu).ThenInclude(v => v.DonViTinh)
                .Where(p => selectedIds.Contains(p.Id)).OrderBy(p => p.NgayDeNghi).ToListAsync();
            if (phieus.Count == 1)
                await _pdfService.ExportDeNghiCapVatTu(phieus[0], dialog.FileName);
            else
                await _pdfService.ExportMultiDeNghiCapVatTu(phieus, dialog.FileName);
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
            var name = selectedIds.Count == 1 ? $"DeNghi_{selectedIds[0]}" : $"DeNghi_Gop_{DateTime.Now:yyyyMMdd}";
            var dialog = new Microsoft.Win32.SaveFileDialog { Filter = "Excel (*.xlsx)|*.xlsx", FileName = $"{name}.xlsx" };
            if (dialog.ShowDialog() != true) return;
            using var context = await _contextFactory.CreateDbContextAsync();
            var phieus = await context.DeNghiCapVatTus.Include(p => p.BoPhan)
                .Include(p => p.ChiTietDeNghis).ThenInclude(ct => ct.VatTu).ThenInclude(v => v.DonViTinh)
                .Where(p => selectedIds.Contains(p.Id)).OrderBy(p => p.NgayDeNghi).ToListAsync();
            if (phieus.Count == 1)
                await _excelService.ExportDeNghiCapVatTu(phieus[0], dialog.FileName);
            else
                await _excelService.ExportMultiDeNghiCapVatTu(phieus, dialog.FileName);
        }
        catch (Exception ex) { ErrorMessage = $"Lỗi xuất Excel: {ex.Message}"; }
    }
}

public partial class ChiTietDeNghiRow : ObservableObject
{
    [ObservableProperty] private VatTu? _vatTu;
    [ObservableProperty] private decimal _soLuongYeuCau;
    [ObservableProperty] private decimal _soLuongDaCap;
    [ObservableProperty] private string _ghiChu = "";
}
