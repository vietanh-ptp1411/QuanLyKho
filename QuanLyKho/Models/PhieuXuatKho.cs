using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKho.Models;

public class PhieuXuatKho : INotifyPropertyChanged
{
    public int Id { get; set; }

    private bool _isSelected;
    [NotMapped]
    public bool IsSelected
    {
        get => _isSelected;
        set { if (_isSelected != value) { _isSelected = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected))); } }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    [Required, MaxLength(50)]
    public string SoPhieu { get; set; } = "";

    public DateTime NgayXuat { get; set; } = DateTime.Now;

    [MaxLength(200)]
    public string NguoiNhan { get; set; } = "";

    public int KhoId { get; set; }
    public Kho Kho { get; set; } = null!;

    public int? BoPhanId { get; set; }
    public BoPhan? BoPhan { get; set; }

    [MaxLength(500)]
    public string NoiNhan { get; set; } = "";

    [MaxLength(500)]
    public string MucDichSuDung { get; set; } = "";

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

    public ICollection<ChiTietPhieuXuat> ChiTietPhieuXuats { get; set; } = new List<ChiTietPhieuXuat>();
}
