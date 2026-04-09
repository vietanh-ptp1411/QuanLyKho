using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using QuanLyKho.Data;

namespace QuanLyKho.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    [ObservableProperty] private int _tongSoVatTu;
    [ObservableProperty] private int _tongSoKho;
    [ObservableProperty] private int _soPhieuNhapThang;
    [ObservableProperty] private int _soPhieuXuatThang;
    [ObservableProperty] private decimal _tongGiaTriNhapThang;
    [ObservableProperty] private decimal _tongGiaTriXuatThang;

    public DashboardViewModel(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
        LoadDataCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task LoadData()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var now = DateTime.Now;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        TongSoVatTu = await context.VatTus.CountAsync();
        TongSoKho = await context.Khos.CountAsync();
        SoPhieuNhapThang = await context.PhieuNhapKhos.CountAsync(p => p.NgayNhap >= startOfMonth);
        SoPhieuXuatThang = await context.PhieuXuatKhos.CountAsync(p => p.NgayXuat >= startOfMonth);
        TongGiaTriNhapThang = await context.PhieuNhapKhos
            .Where(p => p.NgayNhap >= startOfMonth)
            .SumAsync(p => p.TongTien);
        TongGiaTriXuatThang = await context.PhieuXuatKhos
            .Where(p => p.NgayXuat >= startOfMonth)
            .SumAsync(p => p.TongTien);
    }
}
