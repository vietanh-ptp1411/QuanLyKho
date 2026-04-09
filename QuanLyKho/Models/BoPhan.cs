using System.ComponentModel.DataAnnotations;

namespace QuanLyKho.Models;

public class BoPhan
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string TenBoPhan { get; set; } = "";

    public ICollection<PhieuXuatKho> PhieuXuatKhos { get; set; } = new List<PhieuXuatKho>();
    public ICollection<DeNghiCapVatTu> DeNghiCapVatTus { get; set; } = new List<DeNghiCapVatTu>();
}
