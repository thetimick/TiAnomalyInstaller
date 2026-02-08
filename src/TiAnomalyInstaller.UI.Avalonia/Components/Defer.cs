// ⠀
// Defer.cs
// TiAnomalyInstaller.UI.Avalonia
// 
// Created by the_timick on 27.01.2026.
// ⠀

using System;
using System.Threading.Tasks;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

namespace TiAnomalyInstaller.UI.Avalonia.Components;

public interface IDefer : IDisposable, IAsyncDisposable;

public class Defer(Action action) : IDefer
{
    public void Dispose()
    {
        Task.Run(action).Wait();
    }

    public async ValueTask DisposeAsync()
    {
        await Task.Run(action).ConfigureAwait(false);
    }
}

public static class DeferExtensions
{
    extension(object _)
    {
        public IDefer defer(Action action) =>
            new Defer(action);

        public IDefer defer(Func<Task> func) =>
            new Defer(async () => await Task.Run(func));

        public IDefer defer(Func<ValueTask> func) =>
            new Defer(async () => await Task.Run(func));
    }
}