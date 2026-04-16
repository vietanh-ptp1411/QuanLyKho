using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKho.Models;

public class DeNghiCapVatTu : INotifyPropertyChanged
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
