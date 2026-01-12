// ⠀
// StringToBoolConverter.cs
// TiAnomalyInstaller.UI.Avalonia
// 
// Created by the_timick on 11.01.2026.
// ⠀

using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace TiAnomalyInstaller.UI.Avalonia.Converters;

public class StringToBoolConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is string str && !string.IsNullOrEmpty(str.Trim());
    }
    
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}