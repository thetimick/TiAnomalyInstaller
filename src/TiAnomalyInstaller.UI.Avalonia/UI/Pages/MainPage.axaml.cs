// ⠀
// MainPage.axaml.cs
// TiAnomalyInstaller.UI.Avalonia
// 
// Created by the_timick on 01.02.2026.
// ⠀

using Avalonia.Controls;
using MainPageViewModel = TiAnomalyInstaller.UI.Avalonia.ViewModels.Pages.MainPage.MainPageViewModel;

namespace TiAnomalyInstaller.UI.Avalonia.UI.Pages;

public partial class MainPage : UserControl
{
    // ─────────────── Props ───────────────
    
    private static MainPageViewModel ViewModel => Program
        .GetRequiredService<MainPageViewModel>();
    
    // ─────────────── Lifecycle ───────────────
    
    public MainPage()
    {
        InitializeComponent();
        
        DataContext = ViewModel;
        Loaded += (_, _) => ViewModel.Loaded();
    }
}