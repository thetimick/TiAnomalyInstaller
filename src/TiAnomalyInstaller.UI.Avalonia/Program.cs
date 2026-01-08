// ⠀
// Program.cs
// TiAnomalyInstaller.UI.Avalonia
// 
// Created by the_timick on 02.01.2026.
// 

using Avalonia;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using TiAnomalyInstaller.AppConstants;
using TiAnomalyInstaller.Logic.Services;
using TiAnomalyInstaller.UI.Avalonia.UI;

namespace TiAnomalyInstaller.UI.Avalonia;

public static class Program
{
    public static readonly IHost AppHost = Host.CreateDefaultBuilder()
        .ConfigureHostConfiguration(ConfigureHostConfiguration)
        .ConfigureServices(ConfigureServices)
        .Build();
    
    [STAThread]
    public static async Task Main(string[] args)
    {
        await AppHost.StartAsync();
        
        AppBuilder
            .Configure(() => AppHost.Services.GetRequiredService<App>())
            .UsePlatformDetect()
            .LogToTrace()
            .StartWithClassicDesktopLifetime(
                args,
                lifetime => {
                    lifetime.ShutdownRequested += async (_, _) => {
                        await AppHost.StopAsync();
                    };
                }
            );
    }
    
    #region Configure
    
    private static void ConfigureHostConfiguration(IConfigurationBuilder builder)
    {
        builder.SetBasePath(AppContext.BaseDirectory);
    }
    
    private static void ConfigureServices(IServiceCollection collection)
    {
        LoggerAssembly.Configure(collection);
        
        collection.AddHostedService<HostedService>();
        
        // External
        collection.AddSingleton<HttpClient>(_ => new HttpClient { Timeout = TimeSpan.FromSeconds(5) });
        
        // Internal
        // Services
        collection.AddSingleton<IConfigService, ConfigService>();
        collection.AddSingleton<IDownloaderService, DownloaderService>();
        collection.AddSingleton<IHashCheckerService, HashCheckerService>();
        collection.AddSingleton<IPlayingService, PlayingService>();
        collection.AddSingleton<ISevenZipService, SevenZipService>();
        collection.AddSingleton<IWatcherService, WatcherService>();

        // App
        collection.AddSingleton<App>();
        
        // UI
        collection.AddSingleton<MainWindow>();
        collection.AddSingleton<MainWindowViewModel>();
    }
    
    #endregion

    #region Helpers

    public static T GetRequiredService<T>() where T : notnull
    {
        return AppHost.Services.GetRequiredService<T>();
    }

    public static IClassicDesktopStyleApplicationLifetime? GetLifetime()
    {
        return GetRequiredService<App>().ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
    }

    #endregion
    
    #region Preview

    [UsedImplicitly]
    private static AppBuilder BuildAvaloniaApp()
    {
        AppHost.StartAsync()
            .Wait();
        return AppBuilder
            .Configure(() => AppHost.Services.GetRequiredService<App>())
            .UsePlatformDetect()
            .LogToTrace();
    }

    #endregion
}

public static class LoggerAssembly
{
    public static void Configure(IServiceCollection services)
    {
        ConfigureLogging();
        services.AddLogging(builder => {
            builder.AddSerilog(dispose: true);
        });
    }

    private static void ConfigureLogging()
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.File(Constants.Files.LogFileName)
            .CreateLogger();
    }
}