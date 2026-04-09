using System.ComponentModel.DataAnnotations;

namespace QuanLyKho.Models;

public class DeNghiCapVatTu
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string SoPhieu { get; set; } = "";

    public DateTime NgayDeNghi { get; set; } = DateTime.Now;

    [MaxLength(200)]
    public string NguoiDeNghi { get; set; } = "";

    public int? BoPhanId { get; set; }
    public BoPhan? BoPhan { get; set; }

    [MaxLength(200)]
    public string ChucVu { get; set; } = "";

    public int TrangThai { get; set; } // 0=Nháp, 1=Duyệt, 2=Đã cấp, 3=Từ chối

    [MaxLength(500)]
    public string GhiChu { get; set; } = "";

    public DateTime NgayTao { get; set; } = DateTime.Now;

    public ICollection<ChiTietDeNghi> ChiTietDeNghis { get; set; } = new List<ChiTietDeNghi>();
}
