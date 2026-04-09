using System.ComponentModel.DataAnnotations;

namespace QuanLyKho.Models;

public class Kho
{
    public int Id { get; set; }

    [Required, MaxLength(20)]
    public string MaKho { get; set; } = "";

    [Required, MaxLength(200)]
    public string TenKho { get; set; } = "";

    [MaxLength(500)]
    public string DiaChi { get; set; } = "";

    public ICollection<PhieuNhapKho> PhieuNhapKhos { get; set; } = new List<PhieuNhapKho>();
    public ICollection<PhieuXuatKho> PhieuXuatKhos { get; set; } = new List<PhieuXuatKho>();
}
