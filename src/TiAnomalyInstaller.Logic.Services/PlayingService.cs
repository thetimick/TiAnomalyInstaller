// ⠀
// PlayingService.cs
// TiAnomalyInstaller.UI.Avalonia
// 
// Created by the_timick on 02.01.2026.
// ⠀

using System.Diagnostics;
using TiAnomalyInstaller.AppConstants;

namespace TiAnomalyInstaller.Logic.Services;

public interface IPlayingService
{
    public bool IsPlayingAvailable { get; }
    public Task PlayAsync(CancellationToken token = default);
}

public class PlayingService: IPlayingService
{
    public bool IsPlayingAvailable => File.Exists(Constants.MO2.PlayingFileName);

    public async Task PlayAsync(CancellationToken token = default)
    {
        if (!IsPlayingAvailable)
            return;
        await Process
            .Start(Constants.MO2.PlayingFileName)
            .WaitForExitAsync(token);
    }
}