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

    public NhomVatTuViewModel(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
        LoadDataCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task LoadData()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var items = await context.NhomVatTus.OrderBy(x => x.MaNhom).ToListAsync();
        DanhSach = new ObservableCollection<NhomVatTu>(items);
    }

    [RelayCommand]
    private void Add()
    {
        IsNew = true;
        IsEditing = true;
        EditMaNhom = "";
        EditTenNhom = "";
        SelectedItem = null;
    }

    [RelayCommand]
    private void Edit()
    {
        if (SelectedItem == null) return;
        IsNew = false;
        IsEditing = true;
        EditMaNhom = SelectedItem.MaNhom;
        EditTenNhom = SelectedItem.TenNhom;
    }

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(EditMaNhom) || string.IsNullOrWhiteSpace(EditTenNhom)) return;

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

    [RelayCommand]
    private void Cancel()
    {
        IsEditing = false;
    }

    [RelayCommand]
    private async Task Delete()
    {
        if (SelectedItem == null) return;
        using var context = await _contextFactory.CreateDbContextAsync();
        var entity = await context.NhomVatTus.FindAsync(SelectedItem.Id);
        if (entity != null)
        {
            context.NhomVatTus.Remove(entity);
            await context.SaveChangesAsync();
        }
        await LoadData();
    }
}
