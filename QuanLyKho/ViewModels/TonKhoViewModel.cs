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

    public TonKhoViewModel(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
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
                    var nhap = await context.ChiTietPhieuNhaps
                        .Where(ct => ct.VatTuId == vt.Id && ct.PhieuNhapKho.KhoId == kho.Id)
                        .SumAsync(ct => (decimal?)ct.SoLuong) ?? 0;

                    var xuat = await context.ChiTietPhieuXuats
                        .Where(ct => ct.VatTuId == vt.Id && ct.PhieuXuatKho.KhoId == kho.Id)
                        .SumAsync(ct => (decimal?)ct.SoLuong) ?? 0;

                    if (nhap > 0 || xuat > 0)
                    {
                        result.Add(new TonKhoItem
                        {
                            MaVatTu = vt.MaVatTu,
                            TenVatTu = vt.TenVatTu,
                            DonViTinh = vt.DonViTinh?.TenDonVi ?? "",
                            NhomVatTu = vt.NhomVatTu?.TenNhom ?? "",
                            TenKho = kho.TenKho,
                            SoLuongNhap = nhap,
                            SoLuongXuat = xuat,
                            TonCuoi = nhap - xuat
                        });
                    }
                }
            }

            DanhSach = new ObservableCollection<TonKhoItem>(result.OrderBy(x => x.TenKho).ThenBy(x => x.MaVatTu));
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

    partial void OnSearchTextChanged(string value) => LoadDataCommand.ExecuteAsync(null);
    partial void OnFilterKhoChanged(Kho? value) => LoadDataCommand.ExecuteAsync(null);
    partial void OnFilterNhomChanged(NhomVatTu? value) => LoadDataCommand.ExecuteAsync(null);
}

public class TonKhoItem
{
    public string MaVatTu { get; set; } = "";
    public string TenVatTu { get; set; } = "";
    public string DonViTinh { get; set; } = "";
    public string NhomVatTu { get; set; } = "";
    public string TenKho { get; set; } = "";
    public decimal SoLuongNhap { get; set; }
    public decimal SoLuongXuat { get; set; }
    public decimal TonCuoi { get; set; }
}
