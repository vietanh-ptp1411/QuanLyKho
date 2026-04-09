using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;
using QuanLyKho.Data;
using QuanLyKho.Services;
using QuanLyKho.ViewModels;

namespace QuanLyKho;

public partial class App : Application
{
    private readonly ServiceProvider _serviceProvider;

    public App()
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var services = new ServiceCollection();

        // Database
        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlite("Data Source=quanlykho.db"));

        // Services
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IPdfExportService, PdfExportService>();
        services.AddSingleton<IExcelExportService, ExcelExportService>();

        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<VatTuViewModel>();
        services.AddTransient<NhomVatTuViewModel>();
        services.AddTransient<DonViTinhViewModel>();
        services.AddTransient<BoPhanViewModel>();
        services.AddTransient<KhoViewModel>();
        services.AddTransient<NhapKhoViewModel>();
        services.AddTransient<XuatKhoViewModel>();
        services.AddTransient<DeNghiCapVatTuViewModel>();
        services.AddTransient<TonKhoViewModel>();
        services.AddTransient<BaoCaoViewModel>();

        // MainWindow
        services.AddSingleton<MainWindow>();

        _serviceProvider = services.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Ensure database is created
        var contextFactory = _serviceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        using var context = contextFactory.CreateDbContext();
        context.Database.EnsureCreated();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.DataContext = _serviceProvider.GetRequiredService<MainViewModel>();
        mainWindow.Show();
    }
}
