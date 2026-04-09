using System.ComponentModel.DataAnnotations;

namespace QuanLyKho.Models;

public class DonViTinh
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string TenDonVi { get; set; } = "";

    public ICollection<VatTu> VatTus { get; set; } = new List<VatTu>();
}
