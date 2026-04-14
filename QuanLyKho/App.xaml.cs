using System.IO;
using System.Runtime.InteropServices;
using System.Text;
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

            // Tạo shortcut Desktop lần đầu
            CreateDesktopShortcut();

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

    private static void CreateDesktopShortcut()
    {
        try
        {
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var shortcutPath = Path.Combine(desktopPath, "Quản Lý Kho.lnk");
            if (File.Exists(shortcutPath)) return; // Đã có rồi thì bỏ qua

            var exePath = Environment.ProcessPath ?? "";
            if (string.IsNullOrEmpty(exePath)) return;

            // Dùng COM để tạo shortcut
            var shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType == null) return;
            dynamic shell = Activator.CreateInstance(shellType)!;
            dynamic shortcut = shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = exePath;
            shortcut.WorkingDirectory = Path.GetDirectoryName(exePath);
            shortcut.Description = "Quản Lý Kho - Sông Hồng Thủ Đô";
            shortcut.Save();
            Marshal.ReleaseComObject(shortcut);
            Marshal.ReleaseComObject(shell);
        }
        catch
        {
            // Bỏ qua nếu không tạo được shortcut
        }
    }
}
