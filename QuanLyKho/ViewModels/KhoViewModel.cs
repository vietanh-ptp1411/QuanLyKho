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

    public KhoViewModel(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
        LoadDataCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task LoadData()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var items = await context.Khos.OrderBy(x => x.MaKho).ToListAsync();
        DanhSach = new ObservableCollection<Kho>(items);
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
    }

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(EditMaKho) || string.IsNullOrWhiteSpace(EditTenKho)) return;

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

    [RelayCommand]
    private void Cancel() => IsEditing = false;

    [RelayCommand]
    private async Task Delete()
    {
        if (SelectedItem == null) return;
        using var context = await _contextFactory.CreateDbContextAsync();
        var entity = await context.Khos.FindAsync(SelectedItem.Id);
        if (entity != null)
        {
            context.Khos.Remove(entity);
            await context.SaveChangesAsync();
        }
        await LoadData();
    }
}
