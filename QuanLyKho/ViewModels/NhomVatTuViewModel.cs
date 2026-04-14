using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using QuanLyKho.Data;
using QuanLyKho.Helpers;
using QuanLyKho.Models;

namespace QuanLyKho.ViewModels;

public partial class NhomVatTuViewModel : ObservableObject
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    [ObservableProperty] private ObservableCollection<NhomVatTu> _danhSach = new();
    [ObservableProperty] private NhomVatTu? _selectedItem;
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private string _editMaNhom = "";
    [ObservableProperty] private string _editTenNhom = "";
    [ObservableProperty] private bool _isNew;
    [ObservableProperty] private string _errorMessage = "";

    private List<NhomVatTu> _allItems = new();
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _totalPages = 1;
    [ObservableProperty] private int _totalCount;
    [ObservableProperty] private int _pageSize = 15;

    public NhomVatTuViewModel(IDbContextFactory<AppDbContext> contextFactory)
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
            var items = await context.NhomVatTus.OrderByDescending(x => x.Id).ToListAsync();
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
        DanhSach = new ObservableCollection<NhomVatTu>(paged);
        TotalPages = Math.Max(1, (int)Math.Ceiling((double)_allItems.Count / PageSize));
        TotalCount = _allItems.Count;
    }

    [RelayCommand]
    private void Add()
    {
        IsNew = true;
        IsEditing = true;
        EditMaNhom = "";
        EditTenNhom = "";
        SelectedItem = null;
        ErrorMessage = "";
    }

    [RelayCommand]
    private void Edit()
    {
        if (SelectedItem == null) return;
        IsNew = false;
        IsEditing = true;
        EditMaNhom = SelectedItem.MaNhom;
        EditTenNhom = SelectedItem.TenNhom;
        ErrorMessage = "";
    }

    [RelayCommand]
    private void EditItem(NhomVatTu? item)
    {
        if (item == null) return;
        SelectedItem = item;
        IsNew = false;
        IsEditing = true;
        EditMaNhom = item.MaNhom;
        EditTenNhom = item.TenNhom;
        ErrorMessage = "";
    }

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(EditMaNhom) || string.IsNullOrWhiteSpace(EditTenNhom))
        {
            ErrorMessage = "Vui lòng điền đầy đủ thông tin.";
            return;
        }

        try
        {
            ErrorMessage = "";
            using var context = await _contextFactory.CreateDbContextAsync();

            if (IsNew)
            {
                var entity = new NhomVatTu { MaNhom = EditMaNhom.Trim(), TenNhom = EditTenNhom.Trim() };
                context.NhomVatTus.Add(entity);
            }
            else if (SelectedItem != null)
            {
                var entity = await context.NhomVatTus.FindAsync(SelectedItem.Id);
                if (entity != null)
                {
                    entity.MaNhom = EditMaNhom.Trim();
                    entity.TenNhom = EditTenNhom.Trim();
                }
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
        var confirm = System.Windows.MessageBox.Show(
            $"Bạn có chắc muốn xóa nhóm vật tư \"{SelectedItem.TenNhom}\" không?\nThao tác này không thể hoàn tác.",
            "Xác nhận xóa",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);
        if (confirm != System.Windows.MessageBoxResult.Yes) return;
        try
        {
            ErrorMessage = "";
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.NhomVatTus.FindAsync(SelectedItem.Id);
            if (entity != null)
            {
                context.NhomVatTus.Remove(entity);
                await context.SaveChangesAsync();
            }
            await LoadData();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi xóa nhóm vật tư: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteItem(NhomVatTu? item)
    {
        if (item == null) return;
        var confirm = System.Windows.MessageBox.Show(
            $"Bạn có chắc muốn xóa nhóm vật tư \"{item.TenNhom}\" không?\nThao tác này không thể hoàn tác.",
            "Xác nhận xóa",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);
        if (confirm != System.Windows.MessageBoxResult.Yes) return;
        try
        {
            ErrorMessage = "";
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.NhomVatTus.FindAsync(item.Id);
            if (entity != null)
            {
                context.NhomVatTus.Remove(entity);
                await context.SaveChangesAsync();
            }
            await LoadData();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi xóa nhóm vật tư: {ex.Message}";
        }
    }
}
