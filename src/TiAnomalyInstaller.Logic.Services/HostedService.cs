// ⠀
// HostedService.cs
// TiAnomalyInstaller.Logic.Services
// 
// Created by the_timick on 07.01.2026.
// ⠀

using Microsoft.Extensions.Hosting;

namespace TiAnomalyInstaller.Logic.Services;

public class HostedService(
    IConfigService configService,
    IDownloaderService downloaderService
): IHostedService {
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
    
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await downloaderService.CancelAsync();
        configService.SaveLocalConfig();
    }
}