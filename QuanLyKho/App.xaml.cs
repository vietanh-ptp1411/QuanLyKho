using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

        // Load configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var services = new ServiceCollection();

        // Bind AppSettings
        var appSettings = configuration.Get<AppSettings>() ?? new AppSettings();
        services.AddSingleton(appSettings);

        // Database
        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlite(appSettings.Database.ConnectionString));

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

        try
        {
            // Ensure database is created
            var contextFactory = _serviceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            using var context = contextFactory.CreateDbContext();
            context.Database.EnsureCreated();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.DataContext = _serviceProvider.GetRequiredService<MainViewModel>();
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khởi động ứng dụng:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }
}
