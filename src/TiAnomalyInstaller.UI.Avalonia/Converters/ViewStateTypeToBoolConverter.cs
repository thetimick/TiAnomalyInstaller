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

namespace TiAnomalyInstaller.UI.Avalonia.Converters;

public class ViewStateTypeToBoolConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ViewStateType type)
        {
            return type switch {
                ViewStateType.None => false,
                ViewStateType.Loading => false,
                ViewStateType.Content => true,
                ViewStateType.Empty => false,
                ViewStateType.Error => false,
                _ => throw new ArgumentOutOfRangeException(typeof(ViewStateType).FullName, type.ToString(), null)
            };
        }

        return value;
    }
    
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}