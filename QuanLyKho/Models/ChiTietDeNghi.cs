using System.ComponentModel.DataAnnotations;

namespace QuanLyKho.Models;

public class ChiTietDeNghi
{
    public int Id { get; set; }

    public int DeNghiCapVatTuId { get; set; }
    public DeNghiCapVatTu DeNghiCapVatTu { get; set; } = null!;

    public int VatTuId { get; set; }
    public VatTu VatTu { get; set; } = null!;

    public decimal SoLuongYeuCau { get; set; }
    public decimal SoLuongDaCap { get; set; }

    [MaxLength(500)]
    public string GhiChu { get; set; } = "";
}
