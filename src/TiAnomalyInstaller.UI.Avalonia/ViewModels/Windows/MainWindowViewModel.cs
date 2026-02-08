// ⠀
// MainWindowViewModel.cs
// TiAnomalyInstaller.UI.Avalonia
// 
// Created by the_timick on 07.02.2026.
// ⠀

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using TiAnomalyInstaller.AppConstants;
using TiAnomalyInstaller.Logic.Services;
using TiAnomalyInstaller.Logic.Services.Entities;
using TiAnomalyInstaller.UI.Avalonia.Components;
using TiAnomalyInstaller.UI.Avalonia.Components.Interfaces;
using TiAnomalyInstaller.UI.Avalonia.Extensions;
using TiAnomalyInstaller.UI.Avalonia.Services;

namespace TiAnomalyInstaller.UI.Avalonia.ViewModels.Windows;

public partial class MainWindowViewModel(
    INavigationService navigationService,
    IStorageService storageService,
    IConfigService configService,
    IHashCheckerService hashCheckerService,
    IInternetAvailabilityService internetAvailabilityService,
    HttpClient client,
    ILogger<MainWindowViewModel> logger
): ObservableObject, ILoadable {
    // ────────────────────────────────────────────────
    // Methods
    // ────────────────────────────────────────────────

    public async Task LoadContentAsync()
    {
        var url = storageService.GetString(StorageServiceKey.ProfileUrl);
        if (string.IsNullOrEmpty(url) || !url.IsValidUrl())
        {
            await navigationService.RouteTo(Enums.PageType.Init);
            return;
        }
        
        try
        {
            await navigationService.RouteTo(Enums.PageType.Loading);
            
            var config = await configService.ObtainRemoteConfigAsync(url, true);
            await PreloadCustomBackgroundImageIfNeededAsync(config);

            await Task.Delay(500);
            await navigationService.RouteTo(Enums.PageType.Main);
        }
        catch(Exception ex)
        {
            await navigationService.RouteTo(Enums.PageType.Init);
            
            if (logger.IsEnabled(LogLevel.Error))
                logger.LogError("{ex}", ex);
        }
    }
}

public partial class MainWindowViewModel
{
    private async Task PreloadCustomBackgroundImageIfNeededAsync(RemoteConfigEntity config)
    {
        try
        {
            var fileName = Constants.Files.BackgroundFileName;
            
            // Если нет URL - используем зашитую картинку
            if (config.Visual.BackgroundImage is not { } url)
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
                return;
            }
            
            // Если файла нет - загружаем
            if (!File.Exists(fileName))
            {
                await File.WriteAllBytesAsync(
                    fileName,
                    await client.GetByteArrayAsync(url)
                );
                return;
            }
            
            // Если файл есть - сверяем хеш
            
            var bytes = await client.GetByteArrayAsync(url);
            
            // Совпадает
            await using var stream = new MemoryStream(bytes);
            if (await hashCheckerService.ComputeStreamHashAsync(stream) is { } hash && await hashCheckerService.OnFileAsync(fileName, hash)) 
                return;
            
            // Не совпадает
            File.Delete(fileName);
            await File.WriteAllBytesAsync(fileName, bytes);
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Error))
                logger.LogError("{ex}", ex);
        }
    }
}