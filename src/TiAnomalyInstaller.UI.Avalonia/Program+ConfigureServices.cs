// ⠀
// Program+ConfigureServices.cs
// TiAnomalyInstaller.UI.Avalonia
// 
// Created by the_timick on 09.01.2026.
// ⠀

using System;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using TiAnomalyInstaller.AppConstants;
using TiAnomalyInstaller.Logic.Orchestrators;
using TiAnomalyInstaller.Logic.Services;
using TiAnomalyInstaller.Logic.Services.Providers;
using TiAnomalyInstaller.Logic.Services.Services;
using TiAnomalyInstaller.Logic.Services.Services.SevenZip;
using TiAnomalyInstaller.UI.Avalonia.Services;
using TiAnomalyInstaller.UI.Avalonia.UI.Pages;
using TiAnomalyInstaller.UI.Avalonia.UI.Windows.Main;
using TiAnomalyInstaller.UI.Avalonia.ViewModels.Pages;
using TiAnomalyInstaller.UI.Avalonia.ViewModels.Windows;
using InitPageViewModel = TiAnomalyInstaller.UI.Avalonia.ViewModels.Pages.InitPageViewModel;
using MainPageViewModel = TiAnomalyInstaller.UI.Avalonia.ViewModels.Pages.MainPage.MainPageViewModel;

namespace TiAnomalyInstaller.UI.Avalonia;

public static partial class Program
{
    private static void ConfigureServices(IServiceCollection collection)
    {
        // External
        
        Log.Logger = new LoggerConfiguration()
            .Enrich
            .FromLogContext()
            .WriteTo
            .File(
                Constants.Files.LogFileName,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
            )
            .CreateLogger();
        
        collection.AddLogging(builder => {
            builder.AddSerilog(dispose: true);
        });

        collection.AddHttpClient("default", client => client.Timeout = TimeSpan.FromSeconds(5));
        
        // Internal
        collection.AddHostedService<AppService>();
        
        // Services
        collection.AddSingleton<IPlayOrchestrator, PlayOrchestrator>();
        collection.AddSingleton<IInstallOrchestrator, InstallOrchestrator>();
        collection.AddSingleton<IConfigService, ConfigService>();
        collection.AddSingleton<IHashCheckerService, HashCheckerService>();
        collection.AddSingleton<IInMemoryStorageService, InMemoryStorageService>();
        collection.AddSingleton<ITransferService, TransferService>();
        collection.AddSingleton<IOrganizerService, OrganizerService>();
        collection.AddSingleton<IPlayingService, PlayingService>();
        collection.AddSingleton<IWatcherService, WatcherService>();
        collection.AddSingleton<IStorageService, StorageService>(provider => 
            new StorageService(Constants.StorageFolder, provider.GetRequiredService<ILogger<StorageService>>())
        );
        collection.AddSingleton<IInternetAvailabilityService, InternetAvailabilityService>();
        collection.AddSingleton<ISharpSevenZipExtractorFactory, SharpSevenZipExtractorFactory>();
        collection.AddSingleton<ISevenZipService, SevenZipService>();
        collection.AddSingleton<ICleanupService, CleanupService>();
        collection.AddSingleton<ILinkService, LinkService>();
        
        collection.AddTransient<IDownloaderService, DownloaderService>();

        // Providers
        collection.AddSingleton<IUrlProvider, UrlProvider>();
        
        // UI
        collection.AddSingleton<INavigationPageFactory, NavigationPageFactory>();
        collection.AddSingleton<INavigationService, NavigationService>();
        
        collection.AddSingleton<App>();
        
        collection.AddSingleton<MainWindow>();
        collection.AddSingleton<MainWindowViewModel>();
        
        collection.AddScoped<LoadingPage>();
        collection.AddScoped<LoadingPageViewModel>();
        
        collection.AddScoped<InitPage>();
        collection.AddScoped<InitPageViewModel>();
        
        collection.AddScoped<MainPage>();
        collection.AddScoped<MainPageViewModel>();
    }
}