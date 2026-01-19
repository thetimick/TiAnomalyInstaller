// ⠀
// StartupService.cs
// TiAnomalyInstaller.UI.Avalonia.Reactive
// 
// Created by the_timick on 17.01.2026.
// ⠀

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TiAnomalyInstaller.Logic.Services;

namespace TiAnomalyInstaller.UI.Avalonia.Reactive.Services;

public class StartupService(
    IStorageService storageService,
    ILogger<StartupService> logger
): IHostedService {
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        UnhandledExceptionService.Setup(logger);
        return Task.CompletedTask;
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        storageService.Save();
        return Task.CompletedTask;
    }
}