// ⠀
// ViewStateTypeToBoolConverter.cs
// TiAnomalyInstaller.UI.Avalonia
// 
// Created by the_timick on 02.01.2026.
// ⠀

using System;
using System.Globalization;
using Avalonia.Data.Converters;
using TiAnomalyInstaller.UI.Avalonia.Components;
using TiAnomalyInstaller.UI.Avalonia.UI.Windows.Main;

namespace TiAnomalyInstaller.UI.Avalonia.Converters;

public class ViewStateTypeToBoolConverter: IValueConverter
{
    public enum ConverterType
    {
        Normal,
        Invert
    }
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not MainWindowViewModel.MainWindowViewModelType type)
            return value;
        var converterType = ConverterType.Normal;
        if (parameter is ConverterType cType)
            converterType = cType;
        return type switch {
            MainWindowViewModel.MainWindowViewModelType.None
                => converterType != ConverterType.Normal,
            _ => converterType == ConverterType.Normal
        };
    }
    
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}