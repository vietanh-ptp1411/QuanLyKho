using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using QuanLyKho.Models;
using QuanLyKho.Services;

namespace QuanLyKho.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private MenuItem? _selectedMenuItem;

    public ObservableCollection<MenuItem> MenuItems { get; } = new();

    public MainViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        _navigationService.CurrentViewChanged += () => CurrentView = _navigationService.CurrentView;

        MenuItems.Add(new MenuItem { Title = "Tổng quan", Icon = PackIconKind.ViewDashboard, ViewModelType = typeof(DashboardViewModel) });
        MenuItems.Add(new MenuItem { Title = "Vật tư", Icon = PackIconKind.CubeOutline, ViewModelType = typeof(VatTuViewModel) });
        MenuItems.Add(new MenuItem { Title = "Nhóm vật tư", Icon = PackIconKind.FolderOutline, ViewModelType = typeof(NhomVatTuViewModel) });
        MenuItems.Add(new MenuItem { Title = "Đơn vị tính", Icon = PackIconKind.RulerSquare, ViewModelType = typeof(DonViTinhViewModel) });
        MenuItems.Add(new MenuItem { Title = "Bộ phận", Icon = PackIconKind.AccountGroup, ViewModelType = typeof(BoPhanViewModel) });
        MenuItems.Add(new MenuItem { Title = "Kho", Icon = PackIconKind.Warehouse, ViewModelType = typeof(KhoViewModel) });
        MenuItems.Add(new MenuItem { Title = "Nhập kho", Icon = PackIconKind.PackageDown, ViewModelType = typeof(NhapKhoViewModel) });
        MenuItems.Add(new MenuItem { Title = "Xuất kho", Icon = PackIconKind.PackageUp, ViewModelType = typeof(XuatKhoViewModel) });
        MenuItems.Add(new MenuItem { Title = "Đề nghị cấp VT", Icon = PackIconKind.FileDocumentOutline, ViewModelType = typeof(DeNghiCapVatTuViewModel) });
        MenuItems.Add(new MenuItem { Title = "Tồn kho", Icon = PackIconKind.ChartBox, ViewModelType = typeof(TonKhoViewModel) });
        // Navigate to dashboard by default
        SelectedMenuItem = MenuItems[0];
    }

    partial void OnSelectedMenuItemChanged(MenuItem? value)
    {
        if (value != null)
        {
            _navigationService.NavigateTo(value.ViewModelType);
        }
    }
}
