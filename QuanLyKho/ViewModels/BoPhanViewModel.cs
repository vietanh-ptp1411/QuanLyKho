using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using QuanLyKho.Data;
using QuanLyKho.Models;

namespace QuanLyKho.ViewModels;

public partial class BoPhanViewModel : ObservableObject
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    [ObservableProperty] private ObservableCollection<BoPhan> _danhSach = new();
    [ObservableProperty] private BoPhan? _selectedItem;
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private string _editTenBoPhan = "";
    [ObservableProperty] private bool _isNew;
    [ObservableProperty] private string _errorMessage = "";

    private List<BoPhan> _allItems = new();
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _totalPages = 1;
    [ObservableProperty] private int _totalCount;
    [ObservableProperty] private int _pageSize = 20;

    public BoPhanViewModel(IDbContextFactory<AppDbContext> contextFactory)
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
            var items = await context.BoPhans.OrderBy(x => x.TenBoPhan).ToListAsync();
            _allItems = items;
            CurrentPage = 1;
            ApplyPaging();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi tải dữ liệu: {ex.Message}";
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
        DanhSach = new ObservableCollection<BoPhan>(paged);
        TotalPages = Math.Max(1, (int)Math.Ceiling((double)_allItems.Count / PageSize));
        TotalCount = _allItems.Count;
    }

    [RelayCommand]
    private void Add()
    {
        IsNew = true;
        IsEditing = true;
        EditTenBoPhan = "";
        SelectedItem = null;
        ErrorMessage = "";
    }

    [RelayCommand]
    private void Edit()
    {
        if (SelectedItem == null) return;
        IsNew = false;
        IsEditing = true;
        EditTenBoPhan = SelectedItem.TenBoPhan;
        ErrorMessage = "";
    }

    [RelayCommand]
    private void EditItem(BoPhan? item)
    {
        if (item == null) return;
        SelectedItem = item;
        IsNew = false;
        IsEditing = true;
        EditTenBoPhan = item.TenBoPhan;
        ErrorMessage = "";
    }

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(EditTenBoPhan))
        {
            ErrorMessage = "Vui lòng nhập tên bộ phận.";
            return;
        }

        try
        {
            ErrorMessage = "";
            using var context = await _contextFactory.CreateDbContextAsync();

            if (IsNew)
            {
                context.BoPhans.Add(new BoPhan { TenBoPhan = EditTenBoPhan.Trim() });
            }
            else if (SelectedItem != null)
            {
                var entity = await context.BoPhans.FindAsync(SelectedItem.Id);
                if (entity != null) entity.TenBoPhan = EditTenBoPhan.Trim();
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
    private void Cancel()
    {
        IsEditing = false;
        ErrorMessage = "";
    }

    [RelayCommand]
    private async Task Delete()
    {
        if (SelectedItem == null) return;
        try
        {
            ErrorMessage = "";
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.BoPhans.FindAsync(SelectedItem.Id);
            if (entity != null)
            {
                context.BoPhans.Remove(entity);
                await context.SaveChangesAsync();
            }
            await LoadData();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi xóa bộ phận: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteItem(BoPhan? item)
    {
        if (item == null) return;
        try
        {
            ErrorMessage = "";
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.BoPhans.FindAsync(item.Id);
            if (entity != null)
            {
                context.BoPhans.Remove(entity);
                await context.SaveChangesAsync();
            }
            await LoadData();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi xóa bộ phận: {ex.Message}";
        }
    }
}
