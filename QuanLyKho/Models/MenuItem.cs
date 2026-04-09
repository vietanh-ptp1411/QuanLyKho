using MaterialDesignThemes.Wpf;

namespace QuanLyKho.Models;

public class MenuItem
{
    public string Title { get; set; } = "";
    public PackIconKind Icon { get; set; }
    public Type ViewModelType { get; set; } = null!;
}
