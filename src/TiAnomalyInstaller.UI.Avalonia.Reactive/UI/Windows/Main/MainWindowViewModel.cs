// ⠀
// MainWindowViewModel.cs
// TiAnomalyInstaller.UI.Avalonia.Reactive
// 
// Created by the_timick on 15.01.2026.
// ⠀

using System;
using System.Collections.ObjectModel;
using System.Defer;
using System.Linq;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TiAnomalyInstaller.Logic.Orchestrators;
using TiAnomalyInstaller.Logic.Orchestrators.Components;
using TiAnomalyInstaller.Logic.Orchestrators.Entities;
using TiAnomalyInstaller.Logic.Services;
using TiAnomalyInstaller.Logic.Services.Entities.ConfigService;
using TiAnomalyInstaller.UI.Avalonia.Reactive.Extensions;

using Constants = TiAnomalyInstaller.AppConstants.Constants;
using Strings = TiAnomalyInstaller.AppConstants.Localization.Strings;

namespace TiAnomalyInstaller.UI.Avalonia.Reactive.UI.Windows.Main;

public partial class MainWindowViewModel : ReactiveObject, IActivatableViewModel {
    
    public class ProgressBarEntity : ReactiveObject {
        public ProgressBarEntity(string identifier, string title, double value, bool isIndeterminate)
        {
            Identifier = identifier;
            Title = title;
            Value = value;
            IsIndeterminate = isIndeterminate;
        }
        public string Identifier  { get; init; }
        public string Title { get; init; }
        public double Value { get; init; }
        public bool IsIndeterminate { get; init; }
    }
    
    // ────────────────────────────────────────────────
    // Props
    // ────────────────────────────────────────────────

    // ─────────────── Public ───────────────

    [Reactive]
    public partial string? BackgroundImagePath { get; private set; }
    [Reactive]
    public partial string Title { get; private set; } = string.Empty;
    [Reactive]
    public partial string StaticProgressBarTitle { get; private set; } = string.Empty;
    [Reactive]
    public partial ReadOnlyObservableCollection<ProgressBarEntity> DynamicProgressBarList { get; private set; } = null!;
    
    public ViewModelActivator Activator { get; }

    // ─────────────── Private ───────────────
    
    private RemoteConfigEntity _config = null!;
    private CancellationTokenSource? _tokenSource;
    private bool _isPlayingAvailable;
    
    private readonly SourceList<ProgressBarEntity> _sourceProgressBarList = new();
    private readonly IObservable<bool> _isPlayingAvailableObservable;

    private readonly IConfigServiceV2 _configService;
    private readonly IWatcherService _watcherService;
    private readonly IPlayOrchestrator _playOrchestrator;
    private readonly IInstallOrchestrator _installOrchestrator;
    private readonly ILogger<MainWindowViewModel> _logger;
    private readonly IServiceProvider _provider;
    
    public MainWindowViewModel(
        IConfigServiceV2 configService,
        IWatcherService watcherService,
        IPlayOrchestrator playOrchestrator,
        IInstallOrchestrator installOrchestrator,
        ILogger<MainWindowViewModel> logger,
        IServiceProvider provider
    ) {
        _configService = configService;
        _watcherService = watcherService;
        _playOrchestrator = playOrchestrator;
        _installOrchestrator = installOrchestrator;
        _logger = logger;
        _provider = provider;
        _isPlayingAvailableObservable = this.WhenAnyValue(vm => vm._isPlayingAvailable);
        
        Activator = new ViewModelActivator();
        SetupBindings();
    }

    // ────────────────────────────────────────────────
    // Lifecycle
    // ────────────────────────────────────────────────
    
    [ReactiveCommand]
    private void Loaded()
    {
        _logger.LogInformation("Loaded is started...");
        
        SetupConfig();
        SetupWatcher();
        CheckIsPlayingAvailable();
        
        BackgroundImagePath = _config.CustomBackgroundImageUrl != null
            ? Constants.Files.CustomBackgroundImageFileName
            : null;
        Title = _config.Title;
        StaticProgressBarTitle = _isPlayingAvailable
            ? Strings.mw_progress_title_play_available
            : Strings.mw_progress_title_install_available.WithParams(_config.Version);
        
        _logger.LogInformation("Loaded is finished...");
    }
    
    // ────────────────────────────────────────────────
    // Commands
    // ────────────────────────────────────────────────

    [ReactiveCommand(CanExecute = nameof(_isPlayingAvailableObservable))]
    private async Task Play()
    {
        _logger.LogInformation("Play is started...");
        using var defer = Deferable.Defer(() => _tokenSource = null);
        
        _tokenSource = new CancellationTokenSource();
        await _playOrchestrator.StartAsync(_tokenSource.Token);
        
        _logger.LogInformation("Play is finished...");
    }

    [ReactiveCommand]
    private async Task Install()
    {
        _logger.LogInformation("Install is started...");
        using var defer = Deferable.Defer(() => {
            _tokenSource = null;
            _sourceProgressBarList.Clear();
        });
        
        _tokenSource = new CancellationTokenSource();
        await _installOrchestrator.StartAsync(_tokenSource.Token);
        
        _logger.LogInformation("Install is finished...");
    }

    [ReactiveCommand]
    private async Task Exit()
    {
        if (_tokenSource != null)
        {
            await _tokenSource.CancelAsync();
            return;
        }
        
        _provider.GetRequiredService<MainWindow>()
            .Close();
    }
}

// ────────────────────────────────────────────────
// Private Methods
// ────────────────────────────────────────────────

public partial class MainWindowViewModel
{
    private void SetupConfig()
    {
        if (_configService.Cached is not { } config)
            return;
        _config = config;
    }

    private void SetupBindings()
    {
        this.WhenActivated(disposable => {
            _watcherService
                .Events()
                .FolderAppeared
                .Merge(_watcherService.Events().FolderDisappeared)
                .Subscribe(_ => CheckIsPlayingAvailable())
                .DisposeWith(disposable);

            (_playOrchestrator as IOrchestrator<PlayEventArgs>)
                .Events()
                .Handler
                .Subscribe(args => {
                    StaticProgressBarTitle = args.Message;
                })
                .DisposeWith(disposable);
            
            (_installOrchestrator as IOrchestrator<InstallEventArgs>)
                .Events()
                .Handler
                .Subscribe(args => {
                    _sourceProgressBarList.Edit(list => {
                        if (args.IsCompleted)
                        {
                            if (list.FindIndex(e => e.Identifier == args.Identifier) is { } rIndex)
                                list.RemoveAt(rIndex);
                            return;
                        }
                        
                        var entity = new ProgressBarEntity(args.Identifier, args.Title, args.Value, args.IsIndeterminate);
                        if (list.FindIndex(e => e.Identifier == args.Identifier) is { } index)
                        {
                            list[index] = entity;
                            return;
                        }
                        
                        list.Add(entity);
                    });
                })
                .DisposeWith(disposable);
            
            _sourceProgressBarList
                .Connect()
                .Bind(out var items)
                .Subscribe()
                .DisposeWith(disposable);
            DynamicProgressBarList = items;

            PlayCommand.ThrownExceptions
                .Merge(InstallCommand.ThrownExceptions)
                .Merge(ExitCommand.ThrownExceptions)
                .Subscribe(ex => {
                    using var defer = Deferable.Defer(() => {
                        _logger.LogError("{ex}", ex);
                        _tokenSource = null;
                        _sourceProgressBarList.Clear();
                    });
                    
                    if (ex is TaskCanceledException)
                        return;
                    
                    MessageBoxManager
                        .GetMessageBoxStandard(
                            Strings.alert_error_title,
                            Strings.alert_error_text.WithParams(ex.ToString()), 
                            ButtonEnum.Ok, 
                            Icon.Error,
                            windowStartupLocation: WindowStartupLocation.CenterOwner
                        )
                        .ShowWindowDialogAsync(_provider.GetRequiredService<MainWindow>());
                })
                .DisposeWith(disposable);
        });
    }

    private void SetupWatcher()
    {
        _watcherService.Start(
            Constants.CurrentDirectory,
            Constants.Vanilla, Constants.Organizer
        );
    }
    
    private void CheckIsPlayingAvailable()
    {
        _isPlayingAvailable = true;
        return;
        _isPlayingAvailable = _watcherService.FolderStates.Values.All(exists => exists) &&
                              _playOrchestrator.IsPlayingAvailable;
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("CheckIsPlayingAvailable is finished with result \"{result}\"", _isPlayingAvailable);
    }
}