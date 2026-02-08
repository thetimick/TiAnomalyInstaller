// ⠀
// AppService.cs
// TiAnomalyInstaller.Logic.Services
// 
// Created by the_timick on 07.01.2026.
// ⠀

using Microsoft.Extensions.Hosting;

namespace TiAnomalyInstaller.Logic.Services;

public class AppService(
    IStorageService storageService
): IHostedService {
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        storageService.Save();
        return Task.CompletedTask;
    }
}