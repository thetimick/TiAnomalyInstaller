// ⠀
// IOrchestrator.cs
// TiAnomalyInstaller.Logic.Orchestrators
// 
// Created by the_timick on 11.01.2026.
// ⠀

namespace TiAnomalyInstaller.Logic.Orchestrators.Components;

public interface IOrchestrator<out T>
{
    public event EventHandler<T>? Handler;
    Task StartAsync(CancellationToken token = default);
}