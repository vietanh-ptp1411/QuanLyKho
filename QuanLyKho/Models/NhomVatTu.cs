using System.ComponentModel.DataAnnotations;

namespace QuanLyKho.Models;

public class NhomVatTu
{
    public int Id { get; set; }

    [Required, MaxLength(20)]
    public string MaNhom { get; set; } = "";

    [Required, MaxLength(200)]
    public string TenNhom { get; set; } = "";

    public ICollection<VatTu> VatTus { get; set; } = new List<VatTu>();
}
