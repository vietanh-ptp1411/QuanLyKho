using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using QuanLyKho.Data;
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
            var items = await context.NhomVatTus.OrderBy(x => x.MaNhom).ToListAsync();
            DanhSach = new ObservableCollection<NhomVatTu>(items);
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
