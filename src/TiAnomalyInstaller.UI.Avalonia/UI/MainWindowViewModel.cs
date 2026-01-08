// ⠀
// MainWindowViewModel.cs
// TiAnomalyInstaller.UI.Avalonia
// 
// Created by the_timick on 02.01.2026.
// ⠀

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Humanizer;
using Microsoft.Extensions.Logging;
using TiAnomalyInstaller.AppConstants;
using TiAnomalyInstaller.Logic.Services;
using TiAnomalyInstaller.Logic.Services.Entities;
using TiAnomalyInstaller.Logic.Services.Entities.ConfigService;
using TiAnomalyInstaller.UI.Avalonia.Components;

namespace TiAnomalyInstaller.UI.Avalonia.UI;

public partial class MainWindowViewModel(
    IConfigService configService, 
    IDownloaderService downloaderService,
    IHashCheckerService hashCheckerService,
    IPlayingService playingService,
    ISevenZipService zipService,
    IWatcherService watcherService,
    ILogger<MainWindowViewModel> logger
): ObservableObject {
    // Public Props
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TapOnPlayButtonCommand))]
    [NotifyCanExecuteChangedFor(nameof(TapOnInstallButtonCommand))]
    [NotifyCanExecuteChangedFor(nameof(TapOnCheckupButtonCommand))]
    public partial ViewStateType ViewStateType { get; set; }
    
    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string InstallButtonTitle { get; set; } = "Обновить";
    [ObservableProperty]
    public partial string CancelButtonTitle { get; set; } = "Выход";
    
    [ObservableProperty]
    public partial string ProgressTitle { get; set; } = string.Empty;
    [ObservableProperty]
    public partial double ProgressPercentage { get; set; } = 100;
    
    // Private Props

    /// <summary>
    /// Игра установлена и готова к запуску
    /// </summary>
    private bool IsPlayingAvailable => watcherService.FolderStates.Values.All(exists => exists) &&
                                       playingService.IsPlayingAvailable &&
                                       ViewStateType == ViewStateType.Content;
    
    /// <summary>
    /// Доступна новая версия
    /// </summary>
    private bool IsNewVersionAvailable => IsPlayingAvailable && 
                                          configService.LocalCached?.ParsedVersion != configService.RemoteCached?.ParsedVersion;

    private bool IsInstallButtonEnabled => !IsPlayingAvailable || IsNewVersionAvailable;
    
    private IClassicDesktopStyleApplicationLifetime? _lifetime;

    private CancellationTokenSource? _tokenSource;
    
    // Public Methods
    
    public void Loaded()
    {
        try
        {
            _lifetime = Program.GetLifetime();
            Title = configService.GetLocalConfig().Title;
            
            SetLoadingState("Получение конфигурации...");
            
            Task.Factory.StartNew(LoadedAsync);
            
            watcherService.FolderAppeared += (_, _) => Dispatcher.UIThread.Invoke(UpdateTitles);
            watcherService.FolderDisappeared  += (_, _) => Dispatcher.UIThread.Invoke(UpdateTitles);
            watcherService.Start(
                Constants.CurrentDirectory, 
                Constants.Vanilla, Constants.Organizer
            );
        }
        catch (Exception ex)
        {
            Task.Factory.StartNew(() => ShowErrorWithExitAsync(ex));
        }
    }

    private async Task LoadedAsync()
    {
        try
        {
            await configService.ObtainRemoteConfig();
            await Dispatcher.UIThread.InvokeAsync(SetNormalState);
        }
        catch (Exception ex)
        {
            await ShowErrorWithExitAsync(ex);
        }
    }

    // Commands
    
    [RelayCommand(CanExecute = nameof(IsPlayingAvailable))]
    private async Task TapOnPlayButton()
    {
        logger.LogInformation("Tap on Play button");
        SetLoadingState("Запуск MO2...");
        
        _tokenSource = new CancellationTokenSource();

        try
        {
            _lifetime?.MainWindow?.WindowState = WindowState.Minimized;
            await playingService.PlayAsync(_tokenSource.Token);
            _lifetime?.MainWindow?.WindowState = WindowState.Normal;
        }
        catch (OperationCanceledException ex)
        {
            LogError(ex);
            await ShowInfoAsync("Вы отменили операцию");
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
        
        SetNormalState();
    }

    [RelayCommand(CanExecute = nameof(IsInstallButtonEnabled))]
    private async Task TapOnInstallButton()
    {
        logger.LogInformation("Tap on Install button");
        SetLoadingState();

        _tokenSource = new CancellationTokenSource();

        try
        {
            var config = await configService.ObtainRemoteConfig();
           
            // Загружаем хеш для архивов
            var checksums = await hashCheckerService.LoadHashFromUrlAsync(
                config.Hash.ArchiveChecksumsUrl, 
                Constants.StorageDownloadFolder, 
                Constants.Files.Hash.ChecksumsType.Archives, 
                _tokenSource.Token
            );
            
            // Загружаем архивы
            foreach (var archive in config.Archives)
            {
                _tokenSource.Token.ThrowIfCancellationRequested();
                var path = Path.Combine(Constants.StorageDownloadFolder, archive.FileName);
                await DownloadAsync(archive, checksums[path], _tokenSource.Token);
            }
            
            // Распаковываем архивы
            foreach (var archive in config.Archives)
            {
                _tokenSource.Token.ThrowIfCancellationRequested();
                await UnpackAsync(archive, _tokenSource.Token);
            }
            
            // Правим конфиг MO2
            Organizer.Setup(config.Profile);
            
            // Правим версию в локальном конфиге
            if (configService.RemoteCached?.Version is { } newVersion)
            {
                configService.LocalCached?.Version = newVersion;
                configService.SaveLocalConfig();
            }
        }
        catch (OperationCanceledException ex)
        {
            LogError(ex);
            await ShowInfoAsync("Вы отменили операцию");
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }

        SetNormalState();
    }

    [RelayCommand(CanExecute = nameof(IsNewVersionAvailable))]
    private async Task TapOnCheckupButton()
    {
        logger.LogInformation("Tap on Checkup button");
        SetLoadingState();

        if (_tokenSource != null)
            await _tokenSource.CancelAsync();
        _tokenSource = new CancellationTokenSource();
        
        try
        {
            var config = await configService.ObtainRemoteConfig();
            
            // Запускаем проверку файлов
            hashCheckerService.Handler = new Progress<double>(percentage => {
                ProgressTitle = $"Проверка файлов ({percentage:F} %)";
                ProgressPercentage = percentage;
            });
            
            var report = await hashCheckerService.OnFolderAsync(
                config.Hash.GameChecksumsUrl,
                Constants.CurrentDirectory,
                Constants.Files.Hash.ChecksumsType.Game,
                _tokenSource.Token
            );
            
            hashCheckerService.Handler = null;
            
            // Показываем репорт
            await ShowInfoAsync(
                $"""
                  Проверка файлов завершена!
                  
                  ✅ Успешно: {report.Complete.Count}
                  ⚠️ Ошибки: {report.Error.Count}
                  📁 Отсутствуют: {report.NotFound.Count}
                  """
            );
            
            // Чистим файлы
            File.Delete(Constants.Files.Hash.GetPath(Constants.Files.Hash.ChecksumsType.Game, Constants.Files.Hash.FileType.Archive));
            File.Delete(Constants.Files.Hash.GetPath(Constants.Files.Hash.ChecksumsType.Game, Constants.Files.Hash.FileType.Text));
        }
        catch (OperationCanceledException ex)
        {
            LogError(ex);
            await ShowInfoAsync("Вы отменили операцию");
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }

        SetNormalState();
    }
    
    [RelayCommand]
    private async Task TapOnCloseButton()
    {
        if (_tokenSource is not null)
            await _tokenSource.CancelAsync();
        
        if (ViewStateType != ViewStateType.Loading)
            _lifetime?.TryShutdown();
    }
}

// Private Methods

public partial class MainWindowViewModel
{
        private async Task DownloadAsync(RemoteConfigEntity.ArchiveEntity archive, string checksum, CancellationToken token)
    {
        var fileName = Path.Combine(Constants.StorageDownloadFolder, archive.FileName);
        if (File.Exists(fileName))
        {
            ProgressTitle = $"Проверка {archive.FileName}";
            if (await hashCheckerService.OnFileAsync(fileName, checksum, token))
                return;
            File.Delete(fileName);
        }
        
        downloaderService.Handler = new Progress<ProgressEntity>(
            entity => {
                if (entity.Result.IsCompleted)
                {
                    ProgressTitle = $"Загрузка {archive.FileName} завешена";
                    ProgressPercentage = 100;
                    return;
                }
                
                var received = new ByteSize(entity.ReceivedBytesSize).ToString();
                var total = new ByteSize(entity.TotalBytesToReceive).ToString();
                var speed = new ByteSize(entity.AverageBytesPerSecondSpeed).ToString();
                var eta = (int)Math.Ceiling((entity.TotalBytesToReceive - entity.ReceivedBytesSize) / entity.AverageBytesPerSecondSpeed);
                
                ProgressTitle = $@"Загрузка {archive.FileName} ({entity.ProgressPercentage:F} % \ {received} из {total}, {speed}/сек \ {TimeSpan.FromSeconds(eta):hh\:mm\:ss})";
                ProgressPercentage = entity.ProgressPercentage;
            }
        );
        
        await downloaderService.DownloadFileAsync(
            archive.Url,
            fileName,
            token
        );
    }

    private async Task UnpackAsync(RemoteConfigEntity.ArchiveEntity archive, CancellationToken token)
    {
        var fileName = Path.Combine(Constants.StorageDownloadFolder, archive.FileName);
        if (!File.Exists(fileName))
            throw new FileNotFoundException(fileName);
        
        var directory = string.Empty;
        directory = archive.Type switch {
            RemoteConfigEntity.ArchiveEntity.ArchiveType.Vanilla => Constants.VanillaFolderName,
            RemoteConfigEntity.ArchiveEntity.ArchiveType.Organizer => Constants.OrganizerFolderName,
            _ => directory
        };

        zipService.Handler = new Progress<byte>(progress => {
            ProgressTitle = $"Распаковка {archive.FileName} ({progress} %)";
            ProgressPercentage = progress;
        });
        
        await zipService.ToFolderAsync(
            fileName, 
            directory,
            token
        );
    }
    
    private void SetLoadingState(string msg = "Пожалуйста, подождите...")
    {
        ViewStateType = ViewStateType.Loading;
        CancelButtonTitle = "Отмена";
        ProgressTitle = msg;
        ProgressPercentage = 0;
    }
    
    private void SetNormalState()
    {
        ViewStateType = ViewStateType.Content;
        CancelButtonTitle = "Выход";
        ProgressPercentage = 100;
        
        UpdateTitles();
    }

    private void UpdateTitles()
    {
        InstallButtonTitle = IsPlayingAvailable || IsNewVersionAvailable ? "Обновить" : "Установить";
        
        if (IsNewVersionAvailable)
            ProgressTitle = $"Доступно обновление! {configService.LocalCached?.Version} => {configService.RemoteCached?.Version}";
        else if (IsPlayingAvailable)
            ProgressTitle = "Готово к запуску";
        else
            ProgressTitle = "Ожидание установки";

        TapOnPlayButtonCommand.NotifyCanExecuteChanged();
        TapOnInstallButtonCommand.NotifyCanExecuteChanged();
    }
}