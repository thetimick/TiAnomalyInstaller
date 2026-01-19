// ⠀
// StringExtensions.cs
// TiAnomalyInstaller.UI.Avalonia.Reactive
// 
// Created by the_timick on 18.01.2026.
// ⠀

namespace TiAnomalyInstaller.UI.Avalonia.Reactive.Extensions;

public static class StringExtensions
{
    public static string WithParams(this string str, params string[] @params)
    {
        return string.Format(str, @params);
    }
}