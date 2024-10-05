using System;
using System.Windows.Data;

namespace SCIDesktop.converter;

public class EnumToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value == null && parameter == null)
            return true;

        if (value == null || parameter == null)
            return false;

        return value.Equals(parameter);
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is bool boolValue && boolValue)
        {
            return parameter; // Return the enum value or null
        }

        return Binding.DoNothing;
    }
}
