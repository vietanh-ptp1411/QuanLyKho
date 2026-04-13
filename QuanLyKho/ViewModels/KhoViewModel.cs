using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using QuanLyKho.Data;
using QuanLyKho.Models;

namespace QuanLyKho.ViewModels;

public partial class KhoViewModel : ObservableObject
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    [ObservableProperty] private ObservableCollection<Kho> _danhSach = new();
    [ObservableProperty] private Kho? _selectedItem;
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private string _editMaKho = "";
    [ObservableProperty] private string _editTenKho = "";
    [ObservableProperty] private string _editDiaChi = "";
    [ObservableProperty] private bool _isNew;
    [ObservableProperty] private string _errorMessage = "";

    private List<Kho> _allItems = new();
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _totalPages = 1;
    [ObservableProperty] private int _totalCount;
    [ObservableProperty] private int _pageSize = 20;

    public KhoViewModel(IDbContextFactory<AppDbContext> contextFactory)
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
            var items = await context.Khos.OrderByDescending(x => x.Id).ToListAsync();
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
        DanhSach = new ObservableCollection<Kho>(paged);
        TotalPages = Math.Max(1, (int)Math.Ceiling((double)_allItems.Count / PageSize));
        TotalCount = _allItems.Count;
    }

    [RelayCommand]
    private void Add()
    {
        IsNew = true;
        IsEditing = true;
        EditMaKho = "";
        EditTenKho = "";
        EditDiaChi = "";
        SelectedItem = null;
        ErrorMessage = "";
    }

    [RelayCommand]
    private void Edit()
    {
        if (SelectedItem == null) return;
        IsNew = false;
        IsEditing = true;
        EditMaKho = SelectedItem.MaKho;
        EditTenKho = SelectedItem.TenKho;
        EditDiaChi = SelectedItem.DiaChi;
        ErrorMessage = "";
    }

    [RelayCommand]
    private void EditItem(Kho? item)
    {
        if (item == null) return;
        SelectedItem = item;
        IsNew = false;
        IsEditing = true;
        EditMaKho = item.MaKho;
        EditTenKho = item.TenKho;
        EditDiaChi = item.DiaChi;
        ErrorMessage = "";
    }

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(EditMaKho) || string.IsNullOrWhiteSpace(EditTenKho))
        {
            ErrorMessage = "Vui lòng điền mã kho và tên kho.";
            return;
        }

        try
        {
            ErrorMessage = "";
            using var context = await _contextFactory.CreateDbContextAsync();

            if (IsNew)
            {
                context.Khos.Add(new Kho
                {
                    MaKho = EditMaKho.Trim(),
                    TenKho = EditTenKho.Trim(),
                    DiaChi = EditDiaChi.Trim()
                });
            }
            else if (SelectedItem != null)
            {
                var entity = await context.Khos.FindAsync(SelectedItem.Id);
                if (entity != null)
                {
                    entity.MaKho = EditMaKho.Trim();
                    entity.TenKho = EditTenKho.Trim();
                    entity.DiaChi = EditDiaChi.Trim();
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
            var entity = await context.Khos.FindAsync(SelectedItem.Id);
            if (entity != null)
            {
                context.Khos.Remove(entity);
                await context.SaveChangesAsync();
            }
            await LoadData();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi xóa kho: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteItem(Kho? item)
    {
        if (item == null) return;
        try
        {
            ErrorMessage = "";
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Khos.FindAsync(item.Id);
            if (entity != null)
            {
                context.Khos.Remove(entity);
                await context.SaveChangesAsync();
            }
            await LoadData();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi xóa kho: {ex.Message}";
        }
    }
}
