// ⠀
// Program.cs
// TiAnomalyInstaller.UI.Avalonia.Reactive
// 
// Created by the_timick on 16.01.2026.
// ⠀

using System;
using System.Net.Http;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveUI.Avalonia;
using Serilog;
using TiAnomalyInstaller.AppConstants;
using TiAnomalyInstaller.Logic.Orchestrators;
using TiAnomalyInstaller.Logic.Services;
using TiAnomalyInstaller.UI.Avalonia.Reactive.Services;
using TiAnomalyInstaller.UI.Avalonia.Reactive.UI.Windows.Main;
using TiAnomalyInstaller.UI.Avalonia.Reactive.UI.Windows.Startup;

namespace TiAnomalyInstaller.UI.Avalonia.Reactive;

internal static partial class Program
{
    private static IHost _host = null!;
    
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp(args)
            .StartWithClassicDesktopLifetime(args, lifetime =>
            {
                lifetime.Startup += (_, _) => {
                    _host.StartAsync().Wait();
                };
                lifetime.Exit += (_, _) => {
                    _host.StopAsync().Wait();
                };
            });
    }
    
    // ReSharper disable once UnusedMember.Global
    public static AppBuilder BuildAvaloniaApp()
        => BuildAvaloniaApp([]);
    
    private static AppBuilder BuildAvaloniaApp(string[] args)
    {
        BuildHost(args);
        return AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .UseReactiveUI()
            .RegisterReactiveUIViewsFromEntryAssembly();
    }

    private static IHost BuildHost(string[] args)
    {
        _host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                // ─────────────── External ───────────────
                
                services.AddLogging(builder => {
                    Log.Logger = new LoggerConfiguration()
                        .Enrich
                        .FromLogContext()
                        .WriteTo
                        .File(Constants.Files.LogFileName)
                        .CreateLogger();
                    
                    builder.AddSerilog(dispose: true);
                });

                services.AddSingleton<HttpClient>(
                    _ => new HttpClient {
                        Timeout =  TimeSpan.FromSeconds(5)
                    }
                );

                // ─────────────── Internal (Logic) ───────────────

                services.AddHostedService<StartupService>();
                
                services.AddSingleton<IStorageService, StorageService>(
                    provider => new StorageService(
                        Constants.StorageFolder, 
                        provider.GetRequiredService<ILogger<StorageService>>()
                    )
                );
                services.AddSingleton<IInMemoryStorageService, InMemoryStorageService>();
                services.AddSingleton<IConfigService, ConfigService>();
                services.AddSingleton<IConfigServiceV2, ConfigServiceV2>();
                services.AddSingleton<IHashCheckerService, HashCheckerService>();
                services.AddSingleton<IPlayingService, PlayingService>();
                services.AddSingleton<IOrganizerService, OrganizerService>();
                services.AddSingleton<ITransferService, TransferService>();
                services.AddSingleton<IWatcherService, WatcherService>();
                services.AddTransient<IDownloaderService, DownloaderService>();
                services.AddTransient<ISevenZipService, SevenZipService>();
                
                services.AddSingleton<IPlayOrchestrator, PlayOrchestrator>();
                services.AddSingleton<IInstallOrchestrator, InstallOrchestrator>();

                // ─────────────── Internal (UI) ───────────────

                services.AddSingleton<StartupWindow>();
                services.AddSingleton<StartupWindowViewModel>();
                
                services.AddSingleton<MainWindow>();
                services.AddSingleton<MainWindowViewModel>();
            })
            .Build();
        return _host;
    }
}

// Helpers

internal static partial class Program
{
    public static T GetRequiredService<T>() where T : notnull => _host.Services.GetRequiredService<T>();
}