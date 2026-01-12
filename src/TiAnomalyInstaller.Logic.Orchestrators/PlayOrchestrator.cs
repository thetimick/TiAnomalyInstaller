// ⠀
// PlayOrchestrator.cs
// TiAnomalyInstaller.Logic.Orchestrators
// 
// Created by the_timick on 11.01.2026.
// ⠀

using Microsoft.Extensions.Logging;
using TiAnomalyInstaller.Logic.Orchestrators.Components;
using TiAnomalyInstaller.Logic.Orchestrators.Entities;
using TiAnomalyInstaller.Logic.Services;

namespace TiAnomalyInstaller.Logic.Orchestrators;

public interface IPlayOrchestrator : IOrchestrator<PlayEventArgs>;

public partial class PlayOrchestrator(
    IPlayingService playingService, 
    ILogger<PlayOrchestrator> logger
): IPlayOrchestrator {
    
    public EventHandler<PlayEventArgs>? Handler { get; set; }

    public async Task StartAsync(CancellationToken token = default)
    {
        LogStarted();
        Handler?.Invoke(this, new PlayEventArgs(msg: "Запуск MO2..."));
        
        LogPlayInvoked();
        await playingService.PlayAsync(token);
        
        Handler?.Invoke(this, new PlayEventArgs(isCompleted: true));
        LogCompleted();
    }
}

public partial class PlayOrchestrator
{
    [LoggerMessage(LogLevel.Information, "Play orchestration started")]
    private partial void LogStarted();

    [LoggerMessage(LogLevel.Information, "Calling PlayingService.PlayAsync")]
    private partial void LogPlayInvoked();

    [LoggerMessage(LogLevel.Information, "Play orchestration completed")]
    private partial void LogCompleted();
}