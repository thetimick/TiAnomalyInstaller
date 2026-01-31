// ⠀
// UrlProvider.cs
// TiAnomalyInstaller.Logic.Services
// 
// Created by the_timick on 31.01.2026.
// ⠀

using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using TiAnomalyInstaller.AppConstants;

namespace TiAnomalyInstaller.Logic.Services.Providers;

public interface IUrlProvider
{
    Task<string> ObtainUrl(string raw);
}

public partial class UrlProvider(HttpClient client, ILogger<UrlProvider> logger): IUrlProvider
{
    public async Task<string> ObtainUrl(string raw)
    {
        if (await ObtainForYandexIfNeeded(raw) is { } yandexDirectUrl)
            return yandexDirectUrl;
        if (ObtainForGoogleIfNeeded(raw) is { } googleDirectUrl)
            return googleDirectUrl;
        
        if (logger.IsEnabled(LogLevel.Information)) 
            logger.LogInformation("Detected Direct url; {direct}", raw);
        return raw;
    }
}

// ────────────────────────────────────────────────
// Yandex
// ────────────────────────────────────────────────

public partial class UrlProvider
{
    private async Task<string?> ObtainForYandexIfNeeded(string raw)
    {
        if (!raw.Contains(Constants.Utils.YandexDiskDomain)) 
            return null;
        
        var resourceUrl = Constants.Utils.YandexDiskResourcesApi
            .Replace("<key>", Uri.EscapeDataString(raw));
        var json = await client.GetStringAsync(resourceUrl);
        var doc = JsonSerializer.Deserialize<JsonElement>(json);
        var url = doc.TryGetProperty("href", out var href)
            ? href.GetString() ?? null
            : null;

        if (logger.IsEnabled(LogLevel.Information)) 
            logger.LogInformation("Detected Yandex Disk url; Converted from {from} to {to}", raw, url);
        return url;
    }
}

// ────────────────────────────────────────────────
// Google
// ────────────────────────────────────────────────

public partial class UrlProvider
{
    [GeneratedRegex(@"(?:/d/|id=)([a-zA-Z0-9_-]{10,})", RegexOptions.Compiled)]
    private static partial Regex GoogleUrlRegex();
    
    private string? ObtainForGoogleIfNeeded(string raw)
    {
        if (!raw.Contains(Constants.Utils.GoogleDriveDomain))
            return null;
        
        var match = GoogleUrlRegex().Match(raw);
        if (!match.Success) 
            return null;
        var url = Constants.Utils.GoogleDriveTemplateUrl
            .Replace("<file_id>", match.Groups[1].Value);
            
        if (logger.IsEnabled(LogLevel.Information)) 
            logger.LogInformation("Detected Google Drive url; Converted from {from} to {to}", raw, url);
        return url;
    }
}