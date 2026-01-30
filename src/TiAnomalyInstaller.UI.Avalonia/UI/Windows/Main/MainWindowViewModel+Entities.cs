// ⠀
// MainWindowViewModel+Entities.cs
// TiAnomalyInstaller.UI.Avalonia
// 
// Created by the_timick on 21.01.2026.
// ⠀

using CommunityToolkit.Mvvm.ComponentModel;

namespace TiAnomalyInstaller.UI.Avalonia.UI.Windows.Main;

public partial class MainWindowViewModel
{
    public class ProgressBarEntity(string identifier, string title, double value, bool isIndeterminate) : ObservableObject {
        public string Identifier  { get; } = identifier;
        public string Title { get; init; } = title;
        public double Value { get; init; } = value;
        public bool IsIndeterminate { get; init; } = isIndeterminate;
    }
}