// ⠀
// MainWindow.cs
// TiAnomalyInstaller.UI.Avalonia
// 
// Created by the_timick on 01.02.2026.
// 

using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using TiAnomalyInstaller.UI.Avalonia.Services;
using TiAnomalyInstaller.UI.Avalonia.ViewModels.Windows;

namespace TiAnomalyInstaller.UI.Avalonia.UI.Windows.Main;

public partial class MainWindow : Window
{
    // ─────────────── Props ───────────────
    
    private static MainWindowViewModel ViewModel => Program
        .GetRequiredService<MainWindowViewModel>();

    // ─────────────── Lifecycle ───────────────
    
    public MainWindow()
    {
        InitializeComponent();
        DataContext = ViewModel;
        
        Program.GetRequiredService<INavigationService>()
            .Setup(MainFrame);
        Program.GetRequiredService<IDialogService>()
            .Setup(this);
        
        Loaded += (_, _) => Task.Run(async () => await ViewModel.LoadContentAsync());
    }
    
    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }
}