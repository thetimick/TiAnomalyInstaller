// ⠀
// MainWindow.cs
// TiAnomalyInstaller.UI.Avalonia.Reactive
// 
// Created by the_timick on 15.01.2026.
//

using System;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Avalonia;
using Strings = TiAnomalyInstaller.AppConstants.Localization.Strings;

namespace TiAnomalyInstaller.UI.Avalonia.Reactive.UI.Windows.Main;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    // ────────────────────────────────────────────────
    // Lifecycle
    // ────────────────────────────────────────────────
    
    public MainWindow()
    {
        InitializeComponent();
        ViewModel = Program.GetRequiredService<MainWindowViewModel>();
        DataContext = ViewModel;

        this.WhenActivated(disposables => {
            // ─────────────── Activation ───────────────
            
            ViewModel.LoadedCommand
                .Execute()
                .Subscribe()
                .DisposeWith(disposables);

            // ─────────────── Props ───────────────
            
            this.Bind(ViewModel, vm => vm.Title, window => window.TitleTextBlock.Text)
                .DisposeWith(disposables);
            
            this.Bind(ViewModel, vm => vm.StaticProgressBarTitle, window => window.StaticProgressBarText.Text)
                .DisposeWith(disposables);
            
            this.OneWayBind(ViewModel, vm => vm.DynamicProgressBarList, window => window.DynamicProgressBarList.ItemsSource)
                .DisposeWith(disposables);
            
            ViewModel.WhenValueChanged(model => model.BackgroundImagePath)
                .Subscribe(value => {
                    BackgroundImage.Source = new Bitmap(value ?? "../../../Resources/Assets/background_720.jpg");
                })
                .DisposeWith(disposables);
            
            // ─────────────── Commands ───────────────
            
            ViewModel.PlayCommand.IsExecuting
                .Merge(ViewModel.InstallCommand.IsExecuting)
                .Subscribe(isExecuting => {
                    PlayButton.IsEnabled = !isExecuting;
                    InstallButton.IsEnabled = !isExecuting;
                    ExitButton.Content = isExecuting 
                        ? Strings.mw_button_cancel 
                        : Strings.mw_button_exit;
                })
                .DisposeWith(disposables);
            
            ViewModel.PlayCommand.IsExecuting
                .Subscribe(isExecuting => {
                    PlayButtonTitle.IsVisible = !isExecuting;
                    PlayButtonProgressBar.IsVisible = isExecuting;
                })
                .DisposeWith(disposables);
            
            ViewModel.InstallCommand.IsExecuting
                .Subscribe(isExecuting => {
                    InstallButtonText.IsVisible = !isExecuting;
                    InstallButtonProgressBar.IsVisible = isExecuting;
                    StaticProgressBarStack.IsVisible = !isExecuting;
                    DynamicProgressBarList.IsVisible = isExecuting;
                })
                .DisposeWith(disposables);
            
            this.OneWayBind(ViewModel, vm => vm.PlayCommand, window => window.PlayButton.Command)
                .DisposeWith(disposables);
            
            this.OneWayBind(ViewModel, vm => vm.InstallCommand, window => window.InstallButton.Command)
                .DisposeWith(disposables);
            
            this.OneWayBind(ViewModel, vm => vm.ExitCommand, window => window.ExitButton.Command)
                .DisposeWith(disposables);
        });
    }
    
    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }
}