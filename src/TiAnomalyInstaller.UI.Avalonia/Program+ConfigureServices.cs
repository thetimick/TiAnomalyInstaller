// ⠀
// Program+ConfigureServices.cs
// TiAnomalyInstaller.UI.Avalonia
// 
// Created by the_timick on 09.01.2026.
// ⠀

using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TiAnomalyInstaller.AppConstants;
using TiAnomalyInstaller.Logic.Orchestrators;
using TiAnomalyInstaller.Logic.Services;
using TiAnomalyInstaller.UI.Avalonia.UI.Windows.Main;
using TiAnomalyInstaller.UI.Avalonia.UI.Windows.Settings;
using TiAnomalyInstaller.UI.Avalonia.UI.Windows.Startup;
using MainWindowViewModel = TiAnomalyInstaller.UI.Avalonia.UI.Windows.Main.MainWindowViewModel;

namespace TiAnomalyInstaller.UI.Avalonia;

public static partial class Program
{
    private static void ConfigureServices(IServiceCollection collection)
    {
        // External
        
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.File(Constants.Files.LogFileName)
            .CreateLogger();
        
        collection.AddLogging(builder => {
            builder.AddSerilog(dispose: true);
        });
        
        collection.AddSingleton<HttpClient>(_ => new HttpClient { Timeout = TimeSpan.FromSeconds(5) });
        
        // Internal
        collection.AddHostedService<HostedService>();

        collection.AddSingleton<IPlayOrchestrator, PlayOrchestrator>();
        collection.AddSingleton<IInstallOrchestrator, InstallOrchestrator>();
        collection.AddSingleton<IConfigService, ConfigService>();
        collection.AddSingleton<IHashCheckerService, HashCheckerService>();
        collection.AddSingleton<IInMemoryStorageService, InMemoryStorageService>();
        collection.AddSingleton<ITransferService, TransferService>();
        collection.AddSingleton<IOrganizerService, OrganizerService>();
        collection.AddSingleton<IPlayingService, PlayingService>();
        collection.AddSingleton<IWatcherService, WatcherService>();
        
        collection.AddTransient<IDownloaderService, DownloaderService>();
        collection.AddTransient<ISevenZipService, SevenZipService>();

        collection.AddSingleton<App>();
        
        collection.AddSingleton<MainWindow>();
        collection.AddSingleton<MainWindowViewModel>();
        
        collection.AddSingleton<SettingsWindow>();
        collection.AddSingleton<SettingsWindowViewModel>();

        collection.AddSingleton<StartupWindow>();
        collection.AddSingleton<StartupWindowViewModel>();
    }
}