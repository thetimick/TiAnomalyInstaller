// ⠀
// Program.cs
// TiAnomalyInstaller.UI.Avalonia
// 
// Created by the_timick on 02.01.2026.
// 

using Avalonia;
using System;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TiAnomalyInstaller.UI.Avalonia;

public static partial class Program
{
    // ────────────────────────────────────────────────
    // Private Props
    // ────────────────────────────────────────────────
    
    private static IHost _host = null!;
    
    // ────────────────────────────────────────────────
    // Lifecycle
    // ────────────────────────────────────────────────
    
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp(args)
            .StartWithClassicDesktopLifetime(
                args,
                lifetime => {
                    lifetime.Exit += async (_, _) => {
                        await _host.StopAsync();
                    };
                }
            );
    }

    // ────────────────────────────────────────────────
    // Helpers
    // ────────────────────────────────────────────────
    
    public static T GetRequiredService<T>() where T : notnull
    {
        return _host.Services.GetRequiredService<T>();
    }

    public static IClassicDesktopStyleApplicationLifetime? GetLifetime()
    {
        return GetRequiredService<App>().ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
    }
    
    // ReSharper disable once UnusedMember.Global
    public static AppBuilder BuildAvaloniaApp()
        => BuildAvaloniaApp([]);
    
    private static AppBuilder BuildAvaloniaApp(string[] args)
    {
        _host = Host.CreateDefaultBuilder(args)
            .ConfigureHostConfiguration(builder => builder.SetBasePath(Environment.CurrentDirectory))
            .ConfigureServices(ConfigureServices)
            .Build();
        _host.StartAsync().Wait();
        return AppBuilder
            .Configure(() => _host.Services.GetRequiredService<App>())
            .UsePlatformDetect()
            .LogToTrace();
    }
}