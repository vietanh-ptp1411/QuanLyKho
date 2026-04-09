namespace QuanLyKho.Models;

public class ChiTietPhieuXuat
{
    public int Id { get; set; }

    public int PhieuXuatKhoId { get; set; }
    public PhieuXuatKho PhieuXuatKho { get; set; } = null!;

    public int VatTuId { get; set; }
    public VatTu VatTu { get; set; } = null!;

    public decimal SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien { get; set; }
}
