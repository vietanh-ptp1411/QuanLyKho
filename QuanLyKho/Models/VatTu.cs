using System.ComponentModel.DataAnnotations;

namespace QuanLyKho.Models;

public class VatTu
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string MaVatTu { get; set; } = "";

    [Required, MaxLength(500)]
    public string TenVatTu { get; set; } = "";

    public int NhomVatTuId { get; set; }
    public NhomVatTu NhomVatTu { get; set; } = null!;

    public int DonViTinhId { get; set; }
    public DonViTinh DonViTinh { get; set; } = null!;

    [MaxLength(500)]
    public string GhiChu { get; set; } = "";
}
