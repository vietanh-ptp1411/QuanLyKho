using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using QuanLyKho.Data;
using QuanLyKho.Models;

namespace QuanLyKho.ViewModels;

public partial class DeNghiCapVatTuViewModel : ObservableObject
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    [ObservableProperty] private ObservableCollection<DeNghiCapVatTu> _danhSach = new();
    [ObservableProperty] private DeNghiCapVatTu? _selectedPhieu;
    [ObservableProperty] private int _filterTrangThai = -1; // -1 = all
    [ObservableProperty] private string _errorMessage = "";

    private List<DeNghiCapVatTu> _allItems = new();
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _totalPages = 1;
    [ObservableProperty] private int _totalCount;
    [ObservableProperty] private int _pageSize = 20;

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

    public DeNghiCapVatTuViewModel(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
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

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(EditSoPhieu))
        {
            ErrorMessage = "Vui lòng điền số phiếu.";
            return;
        }

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

            phieu.SoPhieu = EditSoPhieu;
            phieu.NgayDeNghi = EditNgayDeNghi;
            phieu.NguoiDeNghi = EditNguoiDeNghi;
            phieu.BoPhanId = EditBoPhan?.Id;
            phieu.ChucVu = EditChucVu;
            phieu.TrangThai = EditTrangThai;
            phieu.GhiChu = EditGhiChu;

            foreach (var row in ChiTietRows.Where(r => r.VatTu != null))
            {
                phieu.ChiTietDeNghis.Add(new ChiTietDeNghi
                {
                    VatTuId = row.VatTu!.Id,
                    SoLuongYeuCau = row.SoLuongYeuCau,
                    SoLuongDaCap = row.SoLuongDaCap,
                    GhiChu = row.GhiChu
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
}

public partial class ChiTietDeNghiRow : ObservableObject
{
    [ObservableProperty] private VatTu? _vatTu;
    [ObservableProperty] private decimal _soLuongYeuCau;
    [ObservableProperty] private decimal _soLuongDaCap;
    [ObservableProperty] private string _ghiChu = "";
}
