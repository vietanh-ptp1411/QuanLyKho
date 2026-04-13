using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace QuanLyKho.Helpers;

/// <summary>
/// Attached behavior cho ComboBox: gõ text sẽ lọc danh sách theo "contains" (không phân biệt hoa thường).
/// Mỗi ComboBox tạo riêng ListCollectionView để không ảnh hưởng nhau trong DataGrid.
/// </summary>
public static class AutoCompleteBehavior
{
    private static readonly ConditionalWeakTable<ComboBox, FilterState> _states = new();

    private class FilterState
    {
        public ListCollectionView? View;
        public bool IsUpdating;
        public PropertyInfo? DisplayProp;
    }

    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(AutoCompleteBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ComboBox cb || (bool)e.NewValue == false) return;

        cb.IsTextSearchEnabled = false;
        cb.StaysOpenOnEdit = true;

        cb.Loaded += ComboBox_Loaded;
        cb.DropDownClosed += ComboBox_DropDownClosed;
        cb.SelectionChanged += ComboBox_SelectionChanged;
        cb.Unloaded += ComboBox_Unloaded;
    }

    private static void ComboBox_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not ComboBox cb) return;

        // Tạo state riêng cho mỗi ComboBox
        var state = _states.GetOrCreateValue(cb);

        // Tạo snapshot items → ListCollectionView riêng
        if (cb.ItemsSource is IEnumerable source)
        {
            var items = new ArrayList();
            foreach (var item in source) items.Add(item);
            state.View = new ListCollectionView(items);

            // Cache property info cho tốc độ
            if (items.Count > 0 && !string.IsNullOrEmpty(cb.DisplayMemberPath))
                state.DisplayProp = items[0]!.GetType().GetProperty(cb.DisplayMemberPath);

            // Gán riêng để không ảnh hưởng ComboBox khác
            state.IsUpdating = true;
            cb.ItemsSource = state.View;
            state.IsUpdating = false;
        }

        // Bắt sự kiện gõ text
        if (cb.Template.FindName("PART_EditableTextBox", cb) is TextBox textBox)
        {
            textBox.TextChanged += (s, args) => OnTextChanged(cb, textBox);
        }
    }

    private static void OnTextChanged(ComboBox cb, TextBox textBox)
    {
        if (!_states.TryGetValue(cb, out var state) || state.IsUpdating || state.View == null) return;

        var text = textBox.Text;

        state.IsUpdating = true;
        try
        {
            if (string.IsNullOrEmpty(text))
            {
                state.View.Filter = null;
            }
            else
            {
                state.View.Filter = item =>
                {
                    string val;
                    if (state.DisplayProp != null)
                        val = state.DisplayProp.GetValue(item)?.ToString() ?? "";
                    else
                        val = item?.ToString() ?? "";
                    return val.Contains(text, StringComparison.OrdinalIgnoreCase);
                };
            }

            if (!cb.IsDropDownOpen && !string.IsNullOrEmpty(text))
                cb.IsDropDownOpen = true;
        }
        finally
        {
            state.IsUpdating = false;
            // Giữ cursor ở cuối text
            textBox.CaretIndex = textBox.Text.Length;
        }
    }

    private static void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox cb) return;
        if (!_states.TryGetValue(cb, out var state) || state.IsUpdating || state.View == null) return;

        // Khi chọn xong → bỏ filter
        state.IsUpdating = true;
        state.View.Filter = null;
        state.IsUpdating = false;
    }

    private static void ComboBox_DropDownClosed(object? sender, EventArgs e)
    {
        if (sender is not ComboBox cb) return;
        if (!_states.TryGetValue(cb, out var state) || state.View == null) return;

        // Bỏ filter khi đóng dropdown
        state.IsUpdating = true;
        state.View.Filter = null;
        state.IsUpdating = false;
    }

    private static void ComboBox_Unloaded(object sender, RoutedEventArgs e)
    {
        if (sender is ComboBox cb)
            _states.Remove(cb);
    }
}
