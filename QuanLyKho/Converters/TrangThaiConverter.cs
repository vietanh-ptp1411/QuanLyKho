using System.Globalization;
using System.Windows.Data;

namespace QuanLyKho.Converters;

public class TrangThaiConverter : IValueConverter
{
    private static readonly string[] Labels = { "Nháp", "Đã duyệt", "Đã cấp", "Từ chối" };

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int i && i >= 0 && i < Labels.Length) return Labels[i];
        return "?";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
