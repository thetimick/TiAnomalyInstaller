// ⠀
// DownloaderService.cs
// TiAnomalyInstaller.Logic.Services
// 
// Created by the_timick on 07.01.2026.
// ⠀

using System.ComponentModel;
using System.Text.Json;
using System.Text.RegularExpressions;
using DebounceThrottle;
using Downloader;
using Microsoft.Extensions.Logging;
using TiAnomalyInstaller.AppConstants;
using TiAnomalyInstaller.Logic.Services.Entities;

namespace TiAnomalyInstaller.Logic.Services;

public interface IDownloaderService
{
    public IProgress<DownloaderProgressEntity>? Handler { get; set; }
    public Task DownloadFileAsync(string rawUrl, string fileName, CancellationToken token = default);
    public Task CancelAsync();
}

public partial class DownloaderService: IDownloaderService
{
    // Props
    
    public IProgress<DownloaderProgressEntity>? Handler { get; set; }
    
    // Private Props

    private readonly ThrottleDispatcher _throttle;
    
    private readonly DownloadService _service = new(
        new DownloadConfiguration {
            ChunkCount = 8,
            ParallelDownload = true,
            Timeout = 5000
        }
    );

    private readonly HttpClient _httpClient = new();

    // LifeCycle

    public DownloaderService(ILogger<DownloaderService> logger)
    {
        _throttle = new ThrottleDispatcher(TimeSpan.FromSeconds(1));
        _service.DownloadProgressChanged += ServiceOnDownloadProgressChanged;
        _service.DownloadFileCompleted += ServiceOnDownloadFileCompleted;
        _service.AddLogger(logger);
    }
    
    // Methods
    
    public async Task DownloadFileAsync(string rawUrl, string fileName, CancellationToken token)
    {
        var url = await ObtainUrlAsync(rawUrl);
        await _service.DownloadFileTaskAsync(url, fileName, token);
        Handler = null;
    }

    public async Task CancelAsync()
    {
        await _service.CancelTaskAsync();
    }

    // Private Methods
    
    private async Task<string?> ObtainUrlAsync(string rawUrl)
    {
        // Если URL от Yandex - получаем ссылку
        // ReSharper disable once InvertIf
        if (rawUrl.Contains(Constants.Utils.YandexDiskDomain))
        {
            var url = Constants.Utils.YandexDiskResourcesApi
                .Replace("<key>", Uri.EscapeDataString(rawUrl));
            var json = await _httpClient.GetStringAsync(url);
            var doc = JsonSerializer.Deserialize<JsonElement>(json);
            return doc.TryGetProperty("href", out var href)
                ? href.GetString()
                : null;
        }
        
        // Если URL от Google - формируем ссылку
        // ReSharper disable once InvertIf
        if (rawUrl.Contains(Constants.Utils.GoogleDriveDomain))
        {
            var match = GoogleDriveUrlRegex().Match(rawUrl);
            if (!match.Success) 
                return rawUrl;
            var id = match.Groups[1].Value;
            return Constants.Utils.GoogleDriveTemplateUrl
                .Replace("<file_id>", id);
        }
        
        return rawUrl;
    }
    
    private void ServiceOnDownloadProgressChanged(object? sender, DownloadProgressChangedEventArgs e)
    {
        _throttle.Throttle(() => {
            Handler?.Report(
                new DownloaderProgressEntity(
                    e.ProgressPercentage,
                    e.AverageBytesPerSecondSpeed,
                    e.ReceivedBytesSize,
                    e.TotalBytesToReceive,
                    (false, null)
                )
            );
        });
    }
    
    private void ServiceOnDownloadFileCompleted(object? sender, AsyncCompletedEventArgs e)
    {
        _throttle.Throttle(() => {
            Handler?.Report(
                new DownloaderProgressEntity(0, 0, 0, 0, (true, e.Error))
            );
        });
    }

    [GeneratedRegex(@"(?:/d/|id=)([a-zA-Z0-9_-]{10,})", RegexOptions.Compiled)]
    private static partial Regex GoogleDriveUrlRegex();
}