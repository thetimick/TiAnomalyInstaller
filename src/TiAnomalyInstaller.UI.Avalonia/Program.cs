// ⠀
// Program.cs
// TiAnomalyInstaller.UI.Avalonia
// 
// Created by the_timick on 02.01.2026.
// 

using Avalonia;
using System;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TiAnomalyInstaller.UI.Avalonia;

public static partial class Program
{
    // Private Props
    
    private static IHost _host = null!;
    
    // Main
    
    [STAThread]
    public static async Task Main(string[] args)
    {
        _host = Host.CreateDefaultBuilder(args)
            .ConfigureHostConfiguration(builder => builder.SetBasePath(Environment.CurrentDirectory))
            .ConfigureServices(ConfigureServices)
            .Build();
        
        await _host.StartAsync();
        
        AppBuilder
            .Configure(GetRequiredService<App>)
            .UsePlatformDetect()
            .LogToTrace()
            .StartWithClassicDesktopLifetime(
                args,
                lifetime => {
                    lifetime.ShutdownRequested += async (_, _) => {
                        await _host.StopAsync();
                    };
                }
            );
    }
    
    // Helpers
    
    public static T GetRequiredService<T>() where T : notnull
    {
        return _host.Services.GetRequiredService<T>();
    }

    public static IClassicDesktopStyleApplicationLifetime? GetLifetime()
    {
        return GetRequiredService<App>().ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
    }
    
    // Preview
    
    private static AppBuilder BuildAvaloniaApp()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureHostConfiguration(builder => builder.SetBasePath(Environment.CurrentDirectory))
            .ConfigureServices(ConfigureServices)
            .Build();
        _host.StartAsync()
            .Wait();
        return AppBuilder
            .Configure(() => _host.Services.GetRequiredService<App>())
            .UsePlatformDetect()
            .LogToTrace();
    }
}