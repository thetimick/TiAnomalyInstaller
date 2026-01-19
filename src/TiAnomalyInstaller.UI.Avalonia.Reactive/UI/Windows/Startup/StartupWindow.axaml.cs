// ⠀
// StartupWindow.axaml.cs
// TiAnomalyInstaller.UI.Avalonia.Reactive
// 
// Created by the_timick on 18.01.2026.
// ⠀


using System;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Interactivity;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace TiAnomalyInstaller.UI.Avalonia.Reactive.UI.Windows.Startup;

public partial class StartupWindow: ReactiveWindow<StartupWindowViewModel>
{
    // ────────────────────────────────────────────────
    // Lifecycle
    // ────────────────────────────────────────────────
    
    public StartupWindow()
    {
        InitializeComponent();
        ViewModel = Program.GetRequiredService<StartupWindowViewModel>();
        
        this.WhenActivated(disposables => {
            ViewModel.OnLoadedCommand
                .Execute()
                .Subscribe()
                .DisposeWith(disposables);

            ViewModel.OnLoadedCommand.IsExecuting
                .Merge(ViewModel.SaveCommand.IsExecuting)
                .Merge(ViewModel.ExitCommand.IsExecuting)
                .Subscribe(isExecuting => {
                    ProfileSeparator.IsVisible = isExecuting;
                    ProfileStackPanel.IsVisible = isExecuting;
                })
                .DisposeWith(disposables);
            
            this.Bind(ViewModel, vm => vm.ProfileTitle, window => window.ProfileText.Text)
                .DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.ProgressTitle, window => window.ProgressText.Text)
                .DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.ProfileUrl, window => window.ProfileUrlTextBox.Text)
                .DisposeWith(disposables);
            
            this.OneWayBind(ViewModel, vm => vm.SaveCommand, window => window.SaveButton.Command)
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