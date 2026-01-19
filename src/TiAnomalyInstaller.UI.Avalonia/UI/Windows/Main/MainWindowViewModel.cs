// ⠀
// MainWindowViewModel.cs
// TiAnomalyInstaller.UI.Avalonia
// 
// Created by the_timick on 02.01.2026.
// ⠀

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using TiAnomalyInstaller.AppConstants;
using TiAnomalyInstaller.AppConstants.Localization;
using TiAnomalyInstaller.Logic.Orchestrators;
using TiAnomalyInstaller.Logic.Orchestrators.Entities;
using TiAnomalyInstaller.Logic.Services;
using TiAnomalyInstaller.UI.Avalonia.Components;
using TiAnomalyInstaller.UI.Avalonia.Extensions;

namespace TiAnomalyInstaller.UI.Avalonia.UI.Windows.Main;

public partial class MainWindowViewModel(
    IConfigService configService,
    IInMemoryStorageService inMemoryStorageService,
    IPlayingService playingService,
    IWatcherService watcherService,
    IPlayOrchestrator playOrchestrator,
    IInstallOrchestrator installOrchestrator,
    ILogger<MainWindowViewModel> logger
): ObservableObject {
    // Nested
    
    public class ProgressBarEntity(string identifier, string title, double value, bool isIndeterminate) : ObservableObject {
        public string Identifier  { get; init; } = identifier;
        public string Title { get; init; } = title;
        public double Value { get; init; } = value;
        public bool IsIndeterminate { get; init; } = isIndeterminate;
    }
    
    // Public Props

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TapOnPlayButtonCommand))]
    [NotifyCanExecuteChangedFor(nameof(TapOnInstallButtonCommand))]
    public partial ViewStateType ViewType { get; set; } = ViewStateType.None;
    
    [ObservableProperty]
    public partial string? CustomBackgroundImageUrl { get; set; }
    
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
    
    // Private Props

    /// <summary>
    /// Игра установлена и готова к запуску
    /// </summary>
    private bool IsPlayingAvailable => watcherService.FolderStates.Values.All(exists => exists) &&
                                       playingService.IsPlayingAvailable;
    
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
            SetLoadingState();
            WatcherServiceStart();
            
            _lifetime = Program.GetLifetime();
            if (inMemoryStorageService.GetValue<Exception>(InMemoryStorageKey.ConfigError) is { } ex)
                throw ex;
            
            if (configService.RemoteCached?.CustomBackgroundImageUrl != null)
                CustomBackgroundImageUrl = Constants.Files.CustomBackgroundImageFileName;
            Title = configService.RemoteCached?.Title ?? "n/n";
        }
        catch (Exception ex)
        {
            Task.Factory.StartNew(() => ShowErrorWithExitAsync(ex));
        }
        finally
        {
            SetNormalState();
        }
    }
    
    // Commands
    
    [RelayCommand(CanExecute = nameof(IsPlayingAvailable))]
    private async Task TapOnPlayButton()
    {
        try
        {
            _tokenSource = new CancellationTokenSource();
            SetLoadingState();
            
            playOrchestrator.Handler += Handler;
            await playOrchestrator.StartAsync(_tokenSource.Token);
        }
        catch (OperationCanceledException ex)
        {
            LogError(ex);
            await ShowInfoAsync(Strings.mw_alert_operation_cancelled);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
        finally
        {
            SetNormalState(); 
            playOrchestrator.Handler -= Handler;
        }

        return;

        void Handler(object? sender, PlayEventArgs args)
        {
            _lifetime?.MainWindow?.WindowState = args.IsCompleted ? WindowState.Normal : WindowState.Minimized;
            MainProgressBarTitle = "MO2 запущен...";
        }
    }

    [RelayCommand(CanExecute = nameof(IsInstallButtonEnabled))]
    private async Task TapOnInstallButton()
    {

        try
        {
            _tokenSource = new CancellationTokenSource();
            SetLoadingState(isHideMainProgressBar: true);
            
            installOrchestrator.Handler += Handler;
            await installOrchestrator.StartAsync(_tokenSource.Token);
        }
        catch (OperationCanceledException ex)
        {
            LogError(ex);
            await ShowInfoAsync(Strings.mw_alert_operation_cancelled);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(ex);
        }
        finally
        {
            installOrchestrator.Handler -= Handler;
            SetNormalState();  
        }
        
        return;

        void Handler(object? sender, InstallEventArgs args)
        {
            Dispatcher.UIThread.Invoke(() => {
                if (args.IsCompleted)
                {
                    if (ProgressBarList.FindIndex(e => e.Identifier == args.Identifier) is { } rIndex)
                        ProgressBarList.RemoveAt(rIndex);
                    return;
                }
                
                var entity = new ProgressBarEntity(args.Identifier, args.Title, args.Value, args.IsIndeterminate);
                if (ProgressBarList.FindIndex(e => e.Identifier == args.Identifier) is { } index)
                {
                    ProgressBarList[index] = entity;
                    return;
                }
                
                ProgressBarList.Add(entity);
            });
        }
    }

    [RelayCommand]
    private void TapOnSettings() { }
    
    [RelayCommand]
    private async Task TapOnCloseButton()
    {
        if (_tokenSource is not null)
            await _tokenSource.CancelAsync();
        
        if (ViewType != ViewStateType.InProgress)
            _lifetime?.TryShutdown();
    }
}

// Private Methods

public partial class MainWindowViewModel
{
    private void WatcherServiceStart()
    {
        watcherService.FolderAppeared += (_, _) => Dispatcher.UIThread.Invoke(() => UpdateTitles());
        watcherService.FolderDisappeared += (_, _) => Dispatcher.UIThread.Invoke(() => UpdateTitles());
        watcherService.Start(
            Constants.CurrentDirectory,
            Constants.Vanilla, Constants.Organizer
        );
    }
    
    private void SetLoadingState(string? msg = "Пожалуйста, подождите...", bool isHideMainProgressBar = false)
    {
        ViewType = ViewStateType.InProgress;
        ProgressBarList.Clear();
        UpdateTitles(msg, isHideMainProgressBar);
    }
    
    private void SetNormalState()
    {
        ViewType = IsPlayingAvailable ? ViewStateType.PlayAvailable :  ViewStateType.InstallAvailable;
        
        ProgressBarList.Clear();
        UpdateTitles();
    }

    private void UpdateTitles(string? msg = null, bool isHideMainProgressBar = false)
    {
        InstallButtonTitle = IsPlayingAvailable || IsNewVersionAvailable 
            ? Strings.mw_button_update 
            : Strings.mw_button_install;
        CancelButtonTitle = ViewType == ViewStateType.InProgress || ViewType  == ViewStateType.Cancelling
            ? Strings.mw_button_cancel
            : Strings.mw_button_exit;

        if (isHideMainProgressBar)
            MainProgressBarTitle = null;
        else if (msg != null)
            MainProgressBarTitle = msg;
        else if (ViewType == ViewStateType.InProgress)
            MainProgressBarTitle = null;
        else if (IsNewVersionAvailable)
            MainProgressBarTitle = string.Format(Strings.mw_progress_title_update_available, configService.LocalCached?.Version, configService.RemoteCached?.Version);
        else if (IsPlayingAvailable)
            MainProgressBarTitle = Strings.mw_progress_title_play_available;
        else
            MainProgressBarTitle = string.Format(Strings.mw_progress_title_install_available, configService.RemoteCached?.Version);

        TapOnPlayButtonCommand.NotifyCanExecuteChanged();
        TapOnInstallButtonCommand.NotifyCanExecuteChanged();
    }
}