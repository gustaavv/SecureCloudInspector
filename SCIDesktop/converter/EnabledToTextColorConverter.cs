using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SCIDesktop.converter;

public class EnabledToTextColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var isEnabled = (bool)value;
        return isEnabled ? Brushes.Black : Brushes.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}