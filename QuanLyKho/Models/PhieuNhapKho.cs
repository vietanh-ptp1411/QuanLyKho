using System.ComponentModel.DataAnnotations;

namespace QuanLyKho.Models;

public class PhieuNhapKho
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string SoPhieu { get; set; } = "";

    public DateTime NgayNhap { get; set; } = DateTime.Now;

    [MaxLength(200)]
    public string NguoiGiaoHang { get; set; } = "";

    public int KhoId { get; set; }
    public Kho Kho { get; set; } = null!;

    [MaxLength(200)]
    public string NguoiLapPhieu { get; set; } = "";

    [MaxLength(200)]
    public string ThuKho { get; set; } = "";

    [MaxLength(200)]
    public string KeToanTruong { get; set; } = "";

    [MaxLength(200)]
    public string GiamDoc { get; set; } = "";

    [MaxLength(500)]
    public string GhiChu { get; set; } = "";

    public decimal TongTien { get; set; }

    public DateTime NgayTao { get; set; } = DateTime.Now;

    public ICollection<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; } = new List<ChiTietPhieuNhap>();
}
