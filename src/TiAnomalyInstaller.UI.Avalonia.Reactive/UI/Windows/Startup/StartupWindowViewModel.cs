// ⠀
// StartupWindowViewModel.cs
// TiAnomalyInstaller.UI.Avalonia.Reactive
// 
// Created by the_timick on 18.01.2026.
// ⠀

using System;
using System.IO;
using System.Net.Http;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TiAnomalyInstaller.AppConstants;
using TiAnomalyInstaller.AppConstants.Localization;
using TiAnomalyInstaller.Logic.Services;
using TiAnomalyInstaller.Logic.Services.Entities.ConfigService;
using TiAnomalyInstaller.UI.Avalonia.Reactive.Extensions;
using TiAnomalyInstaller.UI.Avalonia.Reactive.UI.Windows.Main;

namespace TiAnomalyInstaller.UI.Avalonia.Reactive.UI.Windows.Startup;

public partial class StartupWindowViewModel(
    HttpClient client, 
    IStorageService storageService,
    IConfigServiceV2 configService,
    IHashCheckerService hashCheckerService,
    ILogger<StartupWindowViewModel> logger,
    IServiceProvider provider
): ReactiveObject, IActivatableViewModel {
    // ────────────────────────────────────────────────
    // Props
    // ────────────────────────────────────────────────

    // ─────────────── Public ───────────────
    
    [Reactive]
    public partial string ProfileTitle { get; set; } = Strings.sw_profile_new;
    [Reactive]
    public partial string ProgressTitle { get; set; } = Strings.mw_common_wait;
    [Reactive]
    public partial string ProfileUrl { get; set; } = string.Empty;
    
    public ViewModelActivator Activator { get; } = new();
    
    // ────────────────────────────────────────────────
    // Lifecycle
    // ────────────────────────────────────────────────

    [ReactiveCommand]
    private async Task OnLoaded()
    {
        SetupBinding();
        LoadStorage();

        if (string.IsNullOrEmpty(ProfileUrl))
        {
            ProgressTitle = string.Empty;
        }
        else
        {
            ProgressTitle = Strings.mw_common_wait;
            
            var config = await configService.ObtainRemoteConfigAsync(ProfileUrl, false);
            await PreloadCustomBackgroundImageIfNeededAsync(config.CustomBackgroundImageUrl);
            
            // Для плавности
            await Task.Delay(500);

            provider.GetRequiredService<MainWindow>().Show();
            provider.GetRequiredService<StartupWindow>().Close();
        }
    }

    // ────────────────────────────────────────────────
    // Commands
    // ────────────────────────────────────────────────

    [ReactiveCommand]
    private async Task Save()
    {
        if (string.IsNullOrEmpty(ProfileUrl))
            return;
        
        var config = await configService.ObtainRemoteConfigAsync(ProfileUrl, false);
        await PreloadCustomBackgroundImageIfNeededAsync(config.CustomBackgroundImageUrl);
        ProfileTitle = config.Title;
        
        SaveStorage();
        
        provider.GetRequiredService<MainWindow>()
            .Show();
        provider.GetRequiredService<StartupWindow>()
            .Close();
    }

    [ReactiveCommand]
    private void Exit()
    {
        provider.GetRequiredService<StartupWindow>()
            .Close();
    }
}

// ────────────────────────────────────────────────
// Private Methods
// ────────────────────────────────────────────────

public partial class StartupWindowViewModel
{
    private void SetupBinding()
    {
        this.WhenActivated(disposable => {
            OnLoadedCommand.ThrownExceptions
                .Merge(SaveCommand.ThrownExceptions)
                .Merge(ExitCommand.ThrownExceptions)
                .Subscribe(ex =>
                {
                    MessageBoxManager
                        .GetMessageBoxStandard(
                            Strings.alert_error_title, 
                            Strings.alert_error_text.WithParams(ex.ToString()),
                            ButtonEnum.Ok,
                            Icon.Error
                        )
                        .ShowWindowDialogAsync(Program.GetRequiredService<StartupWindow>());
                })
                .DisposeWith(disposable);
        });
    }

    private void LoadStorage()
    {
        ProfileTitle = storageService.GetString(StorageServiceKey.Profile) ?? Strings.sw_profile_new;
        ProfileUrl = storageService.GetString(StorageServiceKey.ProfileUrl) ?? string.Empty;
    }

    private void SaveStorage()
    {
        storageService.Set(StorageServiceKey.Profile, ProfileTitle);
        storageService.Set(StorageServiceKey.ProfileUrl, ProfileUrl);
        storageService.Save();
    }
    
    private async Task PreloadCustomBackgroundImageIfNeededAsync(string? backgroundImageUrl)
    {
        try
        {
            var fileName = Constants.Files.CustomBackgroundImageFileName;
            
            // Если нет URL - используем зашитую картинку
            if (backgroundImageUrl == null)
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
                    await client.GetByteArrayAsync(backgroundImageUrl)
                );
                return;
            }
            
            // Если файл есть - сверяем хеш
            
            var bytes = await client.GetByteArrayAsync(backgroundImageUrl);
            
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