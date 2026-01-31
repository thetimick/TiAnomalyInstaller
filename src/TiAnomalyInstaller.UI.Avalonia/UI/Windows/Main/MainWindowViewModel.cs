// ⠀
// MainWindowViewModel.cs
// TiAnomalyInstaller.UI.Avalonia
// 
// Created by the_timick on 02.01.2026.
// ⠀

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Humanizer;
using Microsoft.Extensions.Logging;
using TiAnomalyInstaller.AppConstants;
using TiAnomalyInstaller.AppConstants.Localization;
using TiAnomalyInstaller.Logic.Orchestrators;
using TiAnomalyInstaller.Logic.Orchestrators.Entities;
using TiAnomalyInstaller.Logic.Services;
using TiAnomalyInstaller.Logic.Services.Entities;
using TiAnomalyInstaller.UI.Avalonia.Extensions;

namespace TiAnomalyInstaller.UI.Avalonia.UI.Windows.Main;

public partial class MainWindowViewModel(
    IStorageService storageService,
    IConfigService configService,
    IInMemoryStorageService inMemoryStorageService,
    IPlayingService playingService,
    IWatcherService watcherService,
    IPlayOrchestrator playOrchestrator,
    IInstallOrchestrator installOrchestrator,
    ILogger<MainWindowViewModel> logger
): ObservableObject {
    // ────────────────────────────────────────────────
    // Props
    // ────────────────────────────────────────────────

    // ─────────────── Public ───────────────

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TapOnPlayButtonCommand))]
    [NotifyCanExecuteChangedFor(nameof(TapOnInstallButtonCommand))]
    [NotifyCanExecuteChangedFor(nameof(TapOnCloseButtonCommand))]
    public partial MainWindowViewModelType ViewModelType { get; set; } = MainWindowViewModelType.None;
    
    [ObservableProperty]
    public partial Bitmap BackgroundImageFileName { get; set; } = new(
        AssetLoader.Open(new Uri("avares://TiAnomalyInstaller.UI.Avalonia/Resources/Assets/background_720.jpg"))
    );
    
    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;
    [ObservableProperty]
    public partial string InstallButtonTitle { get; set; } = Strings.mw_button_update;
    [ObservableProperty]
    public partial string CancelButtonTitle { get; set; } = Strings.mw_button_exit;

    [ObservableProperty]
    public partial string? MainProgressBarTitle { get; set; } = null;
    
    [ObservableProperty]
    public partial ObservableCollection<ProgressBarEntity> ProgressBarList { get; set; } = [];
    
     // ─────────────── Private ───────────────

    /// <summary>
    /// Игра установлена и готова к запуску
    /// </summary>
    private bool IsPlayingAvailable => ViewModelType == MainWindowViewModelType.PlayAvailable;
    
    /// <summary>
    /// Доступна новая версия
    /// </summary>
    private bool IsNewVersionAvailable => ViewModelType == MainWindowViewModelType.UpdateAvailable;

    private bool IsInstallButtonEnabled => !IsPlayingAvailable || IsNewVersionAvailable;
    private bool IsCancelButtonEnabled => ViewModelType != MainWindowViewModelType.Cancelling;
    
    private IClassicDesktopStyleApplicationLifetime? _lifetime;
    private RemoteConfigEntity _config = null!;

    private CancellationTokenSource? _tokenSource;
    
    // ────────────────────────────────────────────────
    // Lifecycle
    // ────────────────────────────────────────────────
    
    public void Loaded()
    {
        try
        {
            _lifetime = Program.GetLifetime();
            
            // Глобальная ошибка при инициализации
            if (inMemoryStorageService.GetValue<Exception>(InMemoryStorageKey.GlobalError) is { } ex)
                throw ex;
            
            _config = configService.Cached ?? throw new ArgumentNullException();
            
            SetupWatcherService();
            UpdateViewModelType();

            if (_config.Visual.BackgroundImage is not null)
                BackgroundImageFileName = new Bitmap(Constants.Files.BackgroundFileName);
            Title = _config.Metadata.Title;
        }
        catch (Exception ex)
        {
            Task.Factory.StartNew(() => ShowErrorWithExitAsync(ex));
        }
    }

    // ────────────────────────────────────────────────
    // Commands
    // ────────────────────────────────────────────────
    
    [RelayCommand(CanExecute = nameof(IsPlayingAvailable))]
    private async Task TapOnPlayButton()
    {
        try
        {
            _tokenSource = new CancellationTokenSource();
            
            UpdateViewModelType();
            
            playOrchestrator.Handler += Handler;
            await playOrchestrator.StartAsync(_tokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            await ShowInfoAsync(Strings.mw_alert_operation_cancelled);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
        finally
        {
            playOrchestrator.Handler -= Handler;
            _tokenSource = null;
            ProgressBarList.Clear();
            UpdateViewModelType();
        }

        return;

        void Handler(object? sender, PlayEventArgs args)
        {
            _lifetime?.MainWindow?.WindowState = args.IsCompleted ? WindowState.Normal : WindowState.Minimized;
            MainProgressBarTitle = args.Message;
        }
    }
    
    [RelayCommand(CanExecute = nameof(IsInstallButtonEnabled))]
    private async Task TapOnInstallButton()
    {
        try
        {
            _tokenSource = new CancellationTokenSource();
            UpdateViewModelType();
            
            installOrchestrator.Handler += Handler;
            await installOrchestrator.StartAsync(_tokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            LogInfo(Strings.mw_alert_operation_cancelled);
            await ShowInfoAsync(Strings.mw_alert_operation_cancelled);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
        finally
        {
            installOrchestrator.Handler -= Handler;
            _tokenSource = null;
            ProgressBarList.Clear();
            UpdateViewModelType();
        }
        
        return;

        void Handler(object? sender, InstallEventArgs args) =>
            Dispatcher.UIThread.Invoke(() => HandlerOnMain(args));

        void HandlerOnMain(InstallEventArgs args)
        {
            switch (args.Type)
            {
                case InstallEventArgs.InstallType.Hash:
                case InstallEventArgs.InstallType.Download:
                case InstallEventArgs.InstallType.Unpack:
                case InstallEventArgs.InstallType.Merge:
                    var entity = new ProgressBarEntity(args.Identifier, args.Title, args.Value, args.IsIndeterminate);
                    if (ProgressBarList.FindIndex(e => e.Identifier == args.Identifier) is { } index)
                        ProgressBarList[index] = entity;
                    else
                        ProgressBarList.Add(entity);
                break;
                
                case InstallEventArgs.InstallType.Complete:
                    if (ProgressBarList.FindIndex(e => e.Identifier == args.Identifier) is { } rIndex)
                        ProgressBarList.RemoveAt(rIndex);
                break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    [RelayCommand]
    private void TapOnCreateShortcutMenuButton(ShortcutType type)
    { }
    
    [RelayCommand]
    private void TapOnOpenFolderMenuButton(FolderType folder)
    {
        switch (folder)
        {
            case FolderType.Vanilla:
                Process.Start("explorer.exe", Constants.VanillaFolderName);
            break;
            case FolderType.Organizer:
                Process.Start("explorer.exe", Constants.OrganizerFolderName);
            break;
            case FolderType.Storage:
                Process.Start("explorer.exe", Constants.StorageFolder);
            break;
            default:
                throw new ArgumentOutOfRangeException(nameof(folder), folder, null);
        }
    }
    
    [RelayCommand]
    private void TapOnDeleteMenuButton(DeleteType type)
    { }
    
    [RelayCommand(CanExecute = nameof(IsCancelButtonEnabled))]
    private async Task TapOnCloseButton()
    {
        if (_tokenSource is not null)
        {
            UpdateViewModelType(MainWindowViewModelType.Cancelling);
            
            await _tokenSource.CancelAsync();
            _tokenSource = null;
            return;
        }
        
        if (ViewModelType is not (MainWindowViewModelType.InProgress or MainWindowViewModelType.Cancelling))
            _lifetime?.TryShutdown();
    }
}

// ────────────────────────────────────────────────
// Private Methods
// ────────────────────────────────────────────────

public partial class MainWindowViewModel
{
    private void SetupWatcherService()
    {
        watcherService.FolderAppeared += (_, _) => Dispatcher.UIThread.Invoke(UpdateTitles);
        watcherService.FolderDisappeared += (_, _) => Dispatcher.UIThread.Invoke(UpdateTitles);
        watcherService.Start(
            Constants.CurrentDirectory,
            Constants.Vanilla, Constants.Organizer
        );
    }
    
    private void UpdateViewModelType(MainWindowViewModelType? type = null)
    {
        var oldValue = ViewModelType;
        
        try
        {
            if (type.HasValue)
            {
                ViewModelType = type.Value;
            }
            else if (_tokenSource != null)
            {
                ViewModelType = MainWindowViewModelType.InProgress;
            }
            else
            {
                var folderExists = watcherService.FolderStates.Values.All(exists => exists);
                var versionIsCurrent = storageService.GetString(StorageServiceKey.Version) == _config.Metadata.LatestVersion;
                var playingAvailable = playingService.IsPlayingAvailable;

                ViewModelType = folderExists && playingAvailable
                    ? versionIsCurrent 
                        ? MainWindowViewModelType.PlayAvailable 
                        : MainWindowViewModelType.UpdateAvailable
                    : MainWindowViewModelType.InstallAvailable;
            }
        }
        finally
        {
            UpdateTitles();
            LogInfo($"ViewModelType changed from {oldValue} to {ViewModelType}");
        }
    }
    
    private void UpdateTitles()
    {
        InstallButtonTitle = IsPlayingAvailable || IsNewVersionAvailable 
            ? Strings.mw_button_update 
            : Strings.mw_button_install;
        CancelButtonTitle = ViewModelType is MainWindowViewModelType.InProgress or MainWindowViewModelType.Cancelling
            ? Strings.mw_button_cancel
            : Strings.mw_button_exit;
        
        if (ViewModelType == MainWindowViewModelType.Cancelling)
            ProgressBarList.Clear();

        if (ViewModelType == MainWindowViewModelType.Cancelling)
            MainProgressBarTitle = Strings.mw_progress_title_cancelling;
        else if (ViewModelType == MainWindowViewModelType.InProgress)
            MainProgressBarTitle = null;
        else if (IsNewVersionAvailable)
            MainProgressBarTitle = string.Format(Strings.mw_progress_title_update_available, storageService.GetString(StorageServiceKey.Version), _config.Metadata.LatestVersion);
        else if (IsPlayingAvailable)
            MainProgressBarTitle = Strings.mw_progress_title_play_available;
        else
            MainProgressBarTitle = string.Format(
                Strings.mw_progress_title_install_available, 
                _config.Metadata.LatestVersion,
                ByteSize.FromBytes(_config.Size.OverallBytes)
            );

        TapOnPlayButtonCommand.NotifyCanExecuteChanged();
        TapOnInstallButtonCommand.NotifyCanExecuteChanged();
        TapOnCloseButtonCommand.NotifyCanExecuteChanged();
    }
}