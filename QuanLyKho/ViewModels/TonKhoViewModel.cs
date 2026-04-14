using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using QuanLyKho.Data;
using QuanLyKho.Helpers;
using QuanLyKho.Models;
using QuanLyKho.Services;

namespace QuanLyKho.ViewModels;

public partial class TonKhoViewModel : ObservableObject
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IPdfExportService _pdfService;
    private readonly IExcelExportService _excelService;

    [ObservableProperty] private ObservableCollection<TonKhoItem> _danhSach = new();
    [ObservableProperty] private Kho? _filterKho;
    [ObservableProperty] private NhomVatTu? _filterNhom;
    [ObservableProperty] private string _searchText = "";
    [ObservableProperty] private ObservableCollection<Kho> _khos = new();
    [ObservableProperty] private ObservableCollection<NhomVatTu> _nhomVatTus = new();
    [ObservableProperty] private string _errorMessage = "";
    [ObservableProperty] private bool _isLoading;

    // Lọc theo kỳ (tháng/năm)
    [ObservableProperty] private int _selectedMonth;
    [ObservableProperty] private int _selectedYear;
    public ObservableCollection<int> Months { get; } = new(Enumerable.Range(1, 12));
    public ObservableCollection<int> Years { get; } = new(Enumerable.Range(2020, 20));

    // Phân trang
    private List<TonKhoItem> _allItems = new();
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _totalPages = 1;
    [ObservableProperty] private int _totalCount;
    [ObservableProperty] private int _pageSize = 15;

    // Tổng cộng
    [ObservableProperty] private decimal _tongTonDau;
    [ObservableProperty] private decimal _tongNhap;
    [ObservableProperty] private decimal _tongXuat;
    [ObservableProperty] private decimal _tongTonCuoi;

    public TonKhoViewModel(IDbContextFactory<AppDbContext> contextFactory, IPdfExportService pdfService, IExcelExportService excelService)
    {
        _contextFactory = contextFactory;
        _pdfService = pdfService;
        _excelService = excelService;
        _selectedMonth = DateTime.Now.Month;
        _selectedYear = DateTime.Now.Year;
        LoadDataCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task LoadData()
    {
        if (IsLoading) return; // chặn double-trigger
        try
        {
            ErrorMessage = "";
            IsLoading = true;
            using var context = await _contextFactory.CreateDbContextAsync();

            if (Khos.Count == 0)
                Khos = new ObservableCollection<Kho>(await context.Khos.ToListAsync());
            if (NhomVatTus.Count == 0)
                NhomVatTus = new ObservableCollection<NhomVatTu>(await context.NhomVatTus.ToListAsync());

            var dauKy = new DateTime(SelectedYear, SelectedMonth, 1);
            var cuoiKy = dauKy.AddMonths(1);

            // ── Lọc vật tư ──────────────────────────────────────────────────────
            var vatTuQuery = context.VatTus
                .Include(v => v.DonViTinh)
                .Include(v => v.NhomVatTu)
                .AsQueryable();

            if (FilterNhom != null)
                vatTuQuery = vatTuQuery.Where(v => v.NhomVatTuId == FilterNhom.Id);
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var s = SearchText.Trim().ToLower();
                vatTuQuery = vatTuQuery.Where(v => v.MaVatTu.ToLower().Contains(s)
                                                || v.TenVatTu.ToLower().Contains(s));
            }

            var vatTus  = await vatTuQuery.ToListAsync();
            var vatTuIds = vatTus.Select(v => v.Id).ToList();
            var khoList  = FilterKho != null ? new[] { FilterKho } : await context.Khos.ToArrayAsync();
            var khoIds   = khoList.Select(k => k.Id).ToList();

            // ── 4 batch-queries thay thế N×M×4 queries (fix N+1) ────────────────
            var nhapTruoc = await context.ChiTietPhieuNhaps
                .Where(ct => vatTuIds.Contains(ct.VatTuId)
                          && khoIds.Contains(ct.PhieuNhapKho.KhoId)
                          && ct.PhieuNhapKho.NgayNhap < dauKy)
                .GroupBy(ct => new { ct.VatTuId, ct.PhieuNhapKho.KhoId })
                .Select(g => new { g.Key.VatTuId, KhoId = g.Key.KhoId, Total = g.Sum(ct => ct.SoLuong) })
                .ToListAsync();

            var xuatTruoc = await context.ChiTietPhieuXuats
                .Where(ct => vatTuIds.Contains(ct.VatTuId)
                          && khoIds.Contains(ct.PhieuXuatKho.KhoId)
                          && ct.PhieuXuatKho.NgayXuat < dauKy)
                .GroupBy(ct => new { ct.VatTuId, ct.PhieuXuatKho.KhoId })
                .Select(g => new { g.Key.VatTuId, KhoId = g.Key.KhoId, Total = g.Sum(ct => ct.SoLuong) })
                .ToListAsync();

            var nhapTrong = await context.ChiTietPhieuNhaps
                .Where(ct => vatTuIds.Contains(ct.VatTuId)
                          && khoIds.Contains(ct.PhieuNhapKho.KhoId)
                          && ct.PhieuNhapKho.NgayNhap >= dauKy
                          && ct.PhieuNhapKho.NgayNhap < cuoiKy)
                .GroupBy(ct => new { ct.VatTuId, ct.PhieuNhapKho.KhoId })
                .Select(g => new { g.Key.VatTuId, KhoId = g.Key.KhoId, Total = g.Sum(ct => ct.SoLuong) })
                .ToListAsync();

            var xuatTrong = await context.ChiTietPhieuXuats
                .Where(ct => vatTuIds.Contains(ct.VatTuId)
                          && khoIds.Contains(ct.PhieuXuatKho.KhoId)
                          && ct.PhieuXuatKho.NgayXuat >= dauKy
                          && ct.PhieuXuatKho.NgayXuat < cuoiKy)
                .GroupBy(ct => new { ct.VatTuId, ct.PhieuXuatKho.KhoId })
                .Select(g => new { g.Key.VatTuId, KhoId = g.Key.KhoId, Total = g.Sum(ct => ct.SoLuong) })
                .ToListAsync();

            // ── Build lookup O(1) ────────────────────────────────────────────────
            var nhapTruocDict = nhapTruoc.ToDictionary(x => (x.VatTuId, x.KhoId), x => x.Total);
            var xuatTruocDict = xuatTruoc.ToDictionary(x => (x.VatTuId, x.KhoId), x => x.Total);
            var nhapTrongDict = nhapTrong.ToDictionary(x => (x.VatTuId, x.KhoId), x => x.Total);
            var xuatTrongDict = xuatTrong.ToDictionary(x => (x.VatTuId, x.KhoId), x => x.Total);

            // ── Tổng hợp kết quả trong bộ nhớ ───────────────────────────────────
            var result = new List<TonKhoItem>();
            foreach (var kho in khoList)
            {
                foreach (var vt in vatTus)
                {
                    var nTruoc = nhapTruocDict.GetValueOrDefault((vt.Id, kho.Id));
                    var xTruoc = xuatTruocDict.GetValueOrDefault((vt.Id, kho.Id));
                    var nTrong = nhapTrongDict.GetValueOrDefault((vt.Id, kho.Id));
                    var xTrong = xuatTrongDict.GetValueOrDefault((vt.Id, kho.Id));

                    var tonDau  = nTruoc - xTruoc;
                    var tonCuoi = tonDau + nTrong - xTrong;

                    if (tonDau != 0 || nTrong > 0 || xTrong > 0)
                    {
                        result.Add(new TonKhoItem
                        {
                            MaVatTu      = vt.MaVatTu,
                            TenVatTu     = vt.TenVatTu,
                            DonViTinh    = vt.DonViTinh?.TenDonVi ?? "",
                            NhomVatTu    = vt.NhomVatTu?.TenNhom ?? "",
                            TenKho       = kho.TenKho,
                            TonDauKy     = tonDau,
                            SoLuongNhap  = nTrong,
                            SoLuongXuat  = xTrong,
                            TonCuoi      = tonCuoi
                        });
                    }
                }
            }

            _allItems    = result.OrderBy(x => x.TenKho).ThenBy(x => x.MaVatTu).ToList();
            TongTonDau   = _allItems.Sum(x => x.TonDauKy);
            TongNhap     = _allItems.Sum(x => x.SoLuongNhap);
            TongXuat     = _allItems.Sum(x => x.SoLuongXuat);
            TongTonCuoi  = _allItems.Sum(x => x.TonCuoi);
            CurrentPage  = 1;
            ApplyPaging();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi tải dữ liệu tồn kho: {DbExceptionHelper.GetMessage(ex)}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void NextPage() { if (CurrentPage < TotalPages) { CurrentPage++; ApplyPaging(); } }
    [RelayCommand]
    private void PrevPage() { if (CurrentPage > 1) { CurrentPage--; ApplyPaging(); } }
    [RelayCommand]
    private void FirstPage() { CurrentPage = 1; ApplyPaging(); }
    [RelayCommand]
    private void LastPage() { CurrentPage = TotalPages; ApplyPaging(); }

    private void ApplyPaging()
    {
        var paged = _allItems.Skip((CurrentPage - 1) * PageSize).Take(PageSize);
        DanhSach = new ObservableCollection<TonKhoItem>(paged);
        TotalPages = Math.Max(1, (int)Math.Ceiling((double)_allItems.Count / PageSize));
        TotalCount = _allItems.Count;
    }

    [RelayCommand]
    private async Task Print()
    {
        try
        {
            ErrorMessage = "";
            var tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"TonKho_T{SelectedMonth}_{SelectedYear}_{DateTime.Now:HHmmss}.pdf");
            using var context = await _contextFactory.CreateDbContextAsync();
            var khoId = FilterKho?.Id ?? 0;
            await _pdfService.ExportNhapXuatTonKho(context, tempPath, khoId, SelectedMonth, SelectedYear);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = tempPath,
                Verb = "open",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi in báo cáo: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ExportPdf()
    {
        try
        {
            ErrorMessage = "";
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF (*.pdf)|*.pdf",
                FileName = $"TonKho_T{SelectedMonth}_{SelectedYear}.pdf"
            };
            if (dialog.ShowDialog() != true) return;
            using var context = await _contextFactory.CreateDbContextAsync();
            var khoId = FilterKho?.Id ?? 0;
            await _pdfService.ExportNhapXuatTonKho(context, dialog.FileName, khoId, SelectedMonth, SelectedYear);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi xuất PDF: {ex.Message}";
        }
    }

    partial void OnSearchTextChanged(string value) => LoadDataCommand.ExecuteAsync(null);
    partial void OnFilterKhoChanged(Kho? value) => LoadDataCommand.ExecuteAsync(null);
    partial void OnFilterNhomChanged(NhomVatTu? value) => LoadDataCommand.ExecuteAsync(null);
    partial void OnSelectedMonthChanged(int value) => LoadDataCommand.ExecuteAsync(null);
    partial void OnSelectedYearChanged(int value) => LoadDataCommand.ExecuteAsync(null);
}

public class TonKhoItem
{
    public string MaVatTu { get; set; } = "";
    public string TenVatTu { get; set; } = "";
    public string DonViTinh { get; set; } = "";
    public string NhomVatTu { get; set; } = "";
    public string TenKho { get; set; } = "";
    public decimal TonDauKy { get; set; }
    public decimal SoLuongNhap { get; set; }
    public decimal SoLuongXuat { get; set; }
    public decimal TonCuoi { get; set; }
}
