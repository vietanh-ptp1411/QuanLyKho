using System.Globalization;
using System.Windows.Data;

namespace QuanLyKho.Converters;

public class IndexConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int index)
            return (index + 1).ToString();

        // Hỗ trợ DataGridRow → lấy index từ DataGrid
        if (value is System.Windows.Controls.DataGridRow row)
        {
            var dg = System.Windows.Controls.ItemsControl.ItemsControlFromItemContainer(row) as System.Windows.Controls.DataGrid;
            if (dg != null)
                return (dg.ItemContainerGenerator.IndexFromContainer(row) + 1).ToString();
        }

        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
