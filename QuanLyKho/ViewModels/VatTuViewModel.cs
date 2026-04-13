using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using QuanLyKho.Data;
using QuanLyKho.Models;

namespace QuanLyKho.ViewModels;

public partial class VatTuViewModel : ObservableObject
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    [ObservableProperty] private ObservableCollection<VatTu> _danhSach = new();
    [ObservableProperty] private VatTu? _selectedItem;
    [ObservableProperty] private string _searchText = "";
    [ObservableProperty] private NhomVatTu? _filterNhom;

    private List<VatTu> _allItems = new();
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _totalPages = 1;
    [ObservableProperty] private int _totalCount;
    [ObservableProperty] private int _pageSize = 20;
    [ObservableProperty] private ObservableCollection<NhomVatTu> _nhomVatTus = new();
    [ObservableProperty] private ObservableCollection<DonViTinh> _donViTinhs = new();
    [ObservableProperty] private string _errorMessage = "";

    // Edit fields
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private bool _isNew;
    [ObservableProperty] private string _editMaVatTu = "";
    [ObservableProperty] private string _editTenVatTu = "";
    [ObservableProperty] private NhomVatTu? _editNhomVatTu;
    [ObservableProperty] private DonViTinh? _editDonViTinh;
    [ObservableProperty] private string _editGhiChu = "";

    public VatTuViewModel(IDbContextFactory<AppDbContext> contextFactory)
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

            NhomVatTus = new ObservableCollection<NhomVatTu>(await context.NhomVatTus.OrderBy(x => x.TenNhom).ToListAsync());
            DonViTinhs = new ObservableCollection<DonViTinh>(await context.DonViTinhs.OrderBy(x => x.TenDonVi).ToListAsync());

            var query = context.VatTus.Include(x => x.NhomVatTu).Include(x => x.DonViTinh).AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.Trim().ToLower();
                query = query.Where(x => x.MaVatTu.ToLower().Contains(search) || x.TenVatTu.ToLower().Contains(search));
            }

            if (FilterNhom != null)
            {
                query = query.Where(x => x.NhomVatTuId == FilterNhom.Id);
            }

            var items = await query.OrderBy(x => x.MaVatTu).ToListAsync();
            _allItems = items;
            CurrentPage = 1;
            ApplyPaging();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi tải dữ liệu: {ex.Message}";
        }
    }

    partial void OnSearchTextChanged(string value) { CurrentPage = 1; LoadDataCommand.ExecuteAsync(null); }
    partial void OnFilterNhomChanged(NhomVatTu? value) { CurrentPage = 1; LoadDataCommand.ExecuteAsync(null); }

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
        DanhSach = new ObservableCollection<VatTu>(paged);
        TotalPages = Math.Max(1, (int)Math.Ceiling((double)_allItems.Count / PageSize));
        TotalCount = _allItems.Count;
    }

    [RelayCommand]
    private void Add()
    {
        IsNew = true;
        IsEditing = true;
        EditMaVatTu = "";
        EditTenVatTu = "";
        EditNhomVatTu = NhomVatTus.FirstOrDefault();
        EditDonViTinh = DonViTinhs.FirstOrDefault();
        EditGhiChu = "";
    }

    [RelayCommand]
    private void Edit()
    {
        if (SelectedItem == null) return;
        IsNew = false;
        IsEditing = true;
        EditMaVatTu = SelectedItem.MaVatTu;
        EditTenVatTu = SelectedItem.TenVatTu;
        EditNhomVatTu = NhomVatTus.FirstOrDefault(x => x.Id == SelectedItem.NhomVatTuId);
        EditDonViTinh = DonViTinhs.FirstOrDefault(x => x.Id == SelectedItem.DonViTinhId);
        EditGhiChu = SelectedItem.GhiChu;
    }

    [RelayCommand]
    private void EditItem(VatTu? item)
    {
        if (item == null) return;
        SelectedItem = item;
        IsNew = false;
        IsEditing = true;
        EditMaVatTu = item.MaVatTu;
        EditTenVatTu = item.TenVatTu;
        EditNhomVatTu = NhomVatTus.FirstOrDefault(x => x.Id == item.NhomVatTuId);
        EditDonViTinh = DonViTinhs.FirstOrDefault(x => x.Id == item.DonViTinhId);
        EditGhiChu = item.GhiChu;
    }

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(EditMaVatTu) || string.IsNullOrWhiteSpace(EditTenVatTu)
            || EditNhomVatTu == null || EditDonViTinh == null)
        {
            ErrorMessage = "Vui lòng điền đầy đủ thông tin bắt buộc.";
            return;
        }

        try
        {
            ErrorMessage = "";
            using var context = await _contextFactory.CreateDbContextAsync();

            if (IsNew)
            {
                context.VatTus.Add(new VatTu
                {
                    MaVatTu = EditMaVatTu.Trim(),
                    TenVatTu = EditTenVatTu.Trim(),
                    NhomVatTuId = EditNhomVatTu.Id,
                    DonViTinhId = EditDonViTinh.Id,
                    GhiChu = EditGhiChu.Trim()
                });
            }
            else if (SelectedItem != null)
            {
                var entity = await context.VatTus.FindAsync(SelectedItem.Id);
                if (entity != null)
                {
                    entity.MaVatTu = EditMaVatTu.Trim();
                    entity.TenVatTu = EditTenVatTu.Trim();
                    entity.NhomVatTuId = EditNhomVatTu.Id;
                    entity.DonViTinhId = EditDonViTinh.Id;
                    entity.GhiChu = EditGhiChu.Trim();
                }
            }

            await context.SaveChangesAsync();
            IsEditing = false;
            await LoadData();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi lưu dữ liệu: {ex.Message}";
        }
    }

    [RelayCommand]
    private void Cancel() => IsEditing = false;

    [RelayCommand]
    private async Task Delete()
    {
        if (SelectedItem == null) return;
        try
        {
            ErrorMessage = "";
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.VatTus.FindAsync(SelectedItem.Id);
            if (entity != null)
            {
                context.VatTus.Remove(entity);
                await context.SaveChangesAsync();
            }
            await LoadData();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi xóa vật tư: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteItem(VatTu? item)
    {
        if (item == null) return;
        try
        {
            ErrorMessage = "";
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.VatTus.FindAsync(item.Id);
            if (entity != null)
            {
                context.VatTus.Remove(entity);
                await context.SaveChangesAsync();
            }
            await LoadData();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi xóa vật tư: {ex.Message}";
        }
    }
}
