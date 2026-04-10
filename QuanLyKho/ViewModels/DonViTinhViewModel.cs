using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using QuanLyKho.Data;
using QuanLyKho.Models;

namespace QuanLyKho.ViewModels;

public partial class DonViTinhViewModel : ObservableObject
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    [ObservableProperty] private ObservableCollection<DonViTinh> _danhSach = new();
    [ObservableProperty] private DonViTinh? _selectedItem;
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private string _editTenDonVi = "";
    [ObservableProperty] private bool _isNew;
    [ObservableProperty] private string _errorMessage = "";

    public DonViTinhViewModel(IDbContextFactory<AppDbContext> contextFactory)
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
            var items = await context.DonViTinhs.OrderBy(x => x.TenDonVi).ToListAsync();
            DanhSach = new ObservableCollection<DonViTinh>(items);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi tải dữ liệu: {ex.Message}";
        }
    }

    [RelayCommand]
    private void Add()
    {
        IsNew = true;
        IsEditing = true;
        EditTenDonVi = "";
        SelectedItem = null;
        ErrorMessage = "";
    }

    [RelayCommand]
    private void Edit()
    {
        if (SelectedItem == null) return;
        IsNew = false;
        IsEditing = true;
        EditTenDonVi = SelectedItem.TenDonVi;
        ErrorMessage = "";
    }

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(EditTenDonVi))
        {
            ErrorMessage = "Vui lòng nhập tên đơn vị tính.";
            return;
        }

        try
        {
            ErrorMessage = "";
            using var context = await _contextFactory.CreateDbContextAsync();

            if (IsNew)
            {
                context.DonViTinhs.Add(new DonViTinh { TenDonVi = EditTenDonVi.Trim() });
            }
            else if (SelectedItem != null)
            {
                var entity = await context.DonViTinhs.FindAsync(SelectedItem.Id);
                if (entity != null) entity.TenDonVi = EditTenDonVi.Trim();
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
            var entity = await context.DonViTinhs.FindAsync(SelectedItem.Id);
            if (entity != null)
            {
                context.DonViTinhs.Remove(entity);
                await context.SaveChangesAsync();
            }
            await LoadData();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi xóa đơn vị tính: {ex.Message}";
        }
    }
}
