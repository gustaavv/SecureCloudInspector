﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace SCIDesktop.util;

public class SingleSelectionConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int selectedCount)
        {
            return selectedCount == 1;
        }

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}