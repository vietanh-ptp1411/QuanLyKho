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

    public BoPhanViewModel(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
        LoadDataCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task LoadData()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var items = await context.BoPhans.OrderBy(x => x.TenBoPhan).ToListAsync();
        DanhSach = new ObservableCollection<BoPhan>(items);
    }

    [RelayCommand]
    private void Add()
    {
        IsNew = true;
        IsEditing = true;
        EditTenBoPhan = "";
        SelectedItem = null;
    }

    [RelayCommand]
    private void Edit()
    {
        if (SelectedItem == null) return;
        IsNew = false;
        IsEditing = true;
        EditTenBoPhan = SelectedItem.TenBoPhan;
    }

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(EditTenBoPhan)) return;

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

    [RelayCommand]
    private void Cancel() => IsEditing = false;

    [RelayCommand]
    private async Task Delete()
    {
        if (SelectedItem == null) return;
        using var context = await _contextFactory.CreateDbContextAsync();
        var entity = await context.BoPhans.FindAsync(SelectedItem.Id);
        if (entity != null)
        {
            context.BoPhans.Remove(entity);
            await context.SaveChangesAsync();
        }
        await LoadData();
    }
}
