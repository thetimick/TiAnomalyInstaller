// ⠀
// DownloaderService.cs
// TiAnomalyInstaller.Logic.Services
// 
// Created by the_timick on 07.01.2026.
// ⠀

using System.ComponentModel;
using DebounceThrottle;
using Downloader;
using Microsoft.Extensions.Logging;
using TiAnomalyInstaller.Logic.Services.Entities;
using TiAnomalyInstaller.Logic.Services.Providers;
using DownloadProgressChangedEventArgs = Downloader.DownloadProgressChangedEventArgs;

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

    private readonly IUrlProvider _urlProvider;
    private readonly HttpClient _httpClient;

    // LifeCycle

    public DownloaderService(IUrlProvider urlProvider, HttpClient client, ILogger<DownloaderService> logger)
    {
        _urlProvider = urlProvider;
        _httpClient = client;
        
        _throttle = new ThrottleDispatcher(TimeSpan.FromSeconds(1));
        _service.DownloadProgressChanged += ServiceOnDownloadProgressChanged;
        _service.DownloadFileCompleted += ServiceOnDownloadFileCompleted;
        _service.AddLogger(logger);
    }
    
    // Methods
    
    public async Task DownloadFileAsync(string rawUrl, string fileName, CancellationToken token)
    {
        var url = await _urlProvider.ObtainUrl(rawUrl);
        await _service.DownloadFileTaskAsync(url, fileName, token);
        Handler = null;
    }

    public async Task CancelAsync()
    {
        await _service.CancelTaskAsync();
    }

    // Private Methods
    
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
}