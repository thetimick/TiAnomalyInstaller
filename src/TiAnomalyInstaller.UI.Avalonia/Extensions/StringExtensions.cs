// ⠀
// StringExtensions.cs
// TiAnomalyInstaller.UI.Avalonia
// 
// Created by the_timick on 08.02.2026.
// ⠀

using System;

namespace TiAnomalyInstaller.UI.Avalonia.Extensions;

public static class StringExtensions
{
    public static bool IsValidUrl(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}