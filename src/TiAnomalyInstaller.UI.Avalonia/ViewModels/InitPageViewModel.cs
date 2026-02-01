// ⠀
// InitPageViewModel.cs
// TiAnomalyInstaller.UI.Avalonia
// 
// Created by the_timick on 01.02.2026.
// ⠀

using CommunityToolkit.Mvvm.ComponentModel;

namespace TiAnomalyInstaller.UI.Avalonia.ViewModels;

public partial class InitPageViewModel: ObservableObject
{
    [ObservableProperty]
    public partial string ConfigUrlTextBox { get; set; } = string.Empty;
}