using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using QuanLyKho.Data;
using QuanLyKho.Models;

namespace QuanLyKho.ViewModels;

public partial class TonKhoViewModel : ObservableObject
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

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
    [ObservableProperty] private int _pageSize = 25;

    // Tổng cộng
    [ObservableProperty] private decimal _tongTonDau;
    [ObservableProperty] private decimal _tongNhap;
    [ObservableProperty] private decimal _tongXuat;
    [ObservableProperty] private decimal _tongTonCuoi;

    public TonKhoViewModel(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
        _selectedMonth = DateTime.Now.Month;
        _selectedYear = DateTime.Now.Year;
        LoadDataCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task LoadData()
    {
        try
        {
            ErrorMessage = "";
            IsLoading = true;
            using var context = await _contextFactory.CreateDbContextAsync();

            Khos = new ObservableCollection<Kho>(await context.Khos.ToListAsync());
            NhomVatTus = new ObservableCollection<NhomVatTu>(await context.NhomVatTus.ToListAsync());

            // Tính ngày đầu kỳ và cuối kỳ
            var dauKy = new DateTime(SelectedYear, SelectedMonth, 1);
            var cuoiKy = dauKy.AddMonths(1);

            var vatTuQuery = context.VatTus.Include(v => v.DonViTinh).Include(v => v.NhomVatTu).AsQueryable();

            if (FilterNhom != null)
                vatTuQuery = vatTuQuery.Where(v => v.NhomVatTuId == FilterNhom.Id);
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var s = SearchText.Trim().ToLower();
                vatTuQuery = vatTuQuery.Where(v => v.MaVatTu.ToLower().Contains(s) || v.TenVatTu.ToLower().Contains(s));
            }

            var vatTus = await vatTuQuery.ToListAsync();
            var khoList = FilterKho != null ? new[] { FilterKho } : await context.Khos.ToArrayAsync();

            var result = new List<TonKhoItem>();

            foreach (var kho in khoList)
            {
                foreach (var vt in vatTus)
                {
                    // Nhập TRƯỚC kỳ
                    var nhapTruocKy = await context.ChiTietPhieuNhaps
                        .Where(ct => ct.VatTuId == vt.Id && ct.PhieuNhapKho.KhoId == kho.Id
                               && ct.PhieuNhapKho.NgayNhap < dauKy)
                        .SumAsync(ct => (decimal?)ct.SoLuong) ?? 0;

                    // Xuất TRƯỚC kỳ
                    var xuatTruocKy = await context.ChiTietPhieuXuats
                        .Where(ct => ct.VatTuId == vt.Id && ct.PhieuXuatKho.KhoId == kho.Id
                               && ct.PhieuXuatKho.NgayXuat < dauKy)
                        .SumAsync(ct => (decimal?)ct.SoLuong) ?? 0;

                    // Nhập TRONG kỳ
                    var nhapTrongKy = await context.ChiTietPhieuNhaps
                        .Where(ct => ct.VatTuId == vt.Id && ct.PhieuNhapKho.KhoId == kho.Id
                               && ct.PhieuNhapKho.NgayNhap >= dauKy && ct.PhieuNhapKho.NgayNhap < cuoiKy)
                        .SumAsync(ct => (decimal?)ct.SoLuong) ?? 0;

                    // Xuất TRONG kỳ
                    var xuatTrongKy = await context.ChiTietPhieuXuats
                        .Where(ct => ct.VatTuId == vt.Id && ct.PhieuXuatKho.KhoId == kho.Id
                               && ct.PhieuXuatKho.NgayXuat >= dauKy && ct.PhieuXuatKho.NgayXuat < cuoiKy)
                        .SumAsync(ct => (decimal?)ct.SoLuong) ?? 0;

                    var tonDauKy = nhapTruocKy - xuatTruocKy;
                    var tonCuoiKy = tonDauKy + nhapTrongKy - xuatTrongKy;

                    // Chỉ hiện dòng có dữ liệu
                    if (tonDauKy != 0 || nhapTrongKy > 0 || xuatTrongKy > 0)
                    {
                        result.Add(new TonKhoItem
                        {
                            MaVatTu = vt.MaVatTu,
                            TenVatTu = vt.TenVatTu,
                            DonViTinh = vt.DonViTinh?.TenDonVi ?? "",
                            NhomVatTu = vt.NhomVatTu?.TenNhom ?? "",
                            TenKho = kho.TenKho,
                            TonDauKy = tonDauKy,
                            SoLuongNhap = nhapTrongKy,
                            SoLuongXuat = xuatTrongKy,
                            TonCuoi = tonCuoiKy
                        });
                    }
                }
            }

            _allItems = result.OrderBy(x => x.TenKho).ThenBy(x => x.MaVatTu).ToList();

            // Tính tổng (trên toàn bộ, không chỉ trang hiện tại)
            TongTonDau = _allItems.Sum(x => x.TonDauKy);
            TongNhap = _allItems.Sum(x => x.SoLuongNhap);
            TongXuat = _allItems.Sum(x => x.SoLuongXuat);
            TongTonCuoi = _allItems.Sum(x => x.TonCuoi);

            CurrentPage = 1;
            ApplyPaging();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi tải dữ liệu tồn kho: {ex.Message}";
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
