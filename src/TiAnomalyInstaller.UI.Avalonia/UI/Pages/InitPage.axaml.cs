// ⠀
// InitPage.axaml.cs
// TiAnomalyInstaller.UI.Avalonia
// 
// Created by the_timick on 01.02.2026.
// ⠀

using System.Threading.Tasks;
using Avalonia.Controls;
using TiAnomalyInstaller.UI.Avalonia.ViewModels.Pages;

namespace TiAnomalyInstaller.UI.Avalonia.UI.Pages;

public partial class InitPage : UserControl
{
    // ────────────────────────────────────────────────
    // Props
    // ────────────────────────────────────────────────
    
    private readonly InitPageViewModel _viewModel = Program.GetRequiredService<InitPageViewModel>();

    // ────────────────────────────────────────────────
    // Lifecycle
    // ────────────────────────────────────────────────
    
    public InitPage()
    {
        InitializeComponent();
        DataContext = _viewModel;
        Loaded += (_, _) => Task.Run(async () => await _viewModel.LoadContentAsync());
    }
}