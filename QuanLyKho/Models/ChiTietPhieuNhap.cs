namespace QuanLyKho.Models;

public class ChiTietPhieuNhap
{
    public int Id { get; set; }

    public int PhieuNhapKhoId { get; set; }
    public PhieuNhapKho PhieuNhapKho { get; set; } = null!;

    public int VatTuId { get; set; }
    public VatTu VatTu { get; set; } = null!;

    public decimal SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien { get; set; }

    [System.ComponentModel.DataAnnotations.MaxLength(200)]
    public string NhaCungCap { get; set; } = "";
}
