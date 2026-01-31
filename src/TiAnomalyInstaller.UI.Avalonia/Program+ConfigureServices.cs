// ⠀
// Program+ConfigureServices.cs
// TiAnomalyInstaller.UI.Avalonia
// 
// Created by the_timick on 09.01.2026.
// ⠀

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using TiAnomalyInstaller.AppConstants;
using TiAnomalyInstaller.Logic.Orchestrators;
using TiAnomalyInstaller.Logic.Services;
using TiAnomalyInstaller.UI.Avalonia.UI.Windows.Main;

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

        collection.AddHttpClient("default", client => client.Timeout = TimeSpan.FromSeconds(5));
        
        // Internal
        collection.AddHostedService<AppService>();

        collection.AddSingleton<IPlayOrchestrator, PlayOrchestrator>();
        collection.AddSingleton<IInstallOrchestrator, InstallOrchestrator>();
        collection.AddSingleton<IConfigService, ConfigService>();
        collection.AddSingleton<IHashCheckerService, HashCheckerService>();
        collection.AddSingleton<IInMemoryStorageService, InMemoryStorageService>();
        collection.AddSingleton<ITransferService, TransferService>();
        collection.AddSingleton<IOrganizerService, OrganizerService>();
        collection.AddSingleton<IPlayingService, PlayingService>();
        collection.AddSingleton<IWatcherService, WatcherService>();
        collection.AddSingleton<IStorageService, StorageService>(provider => new StorageService(Constants.StorageFolder, provider.GetRequiredService<ILogger<StorageService>>()));
        collection.AddSingleton<IInternetAvailabilityService, InternetAvailabilityService>();
        
        collection.AddTransient<IDownloaderService, DownloaderService>();
        collection.AddTransient<ISevenZipService, SevenZipService>();

        collection.AddSingleton<App>();
        
        collection.AddSingleton<MainWindow>();
        collection.AddSingleton<MainWindowViewModel>();
    }
}