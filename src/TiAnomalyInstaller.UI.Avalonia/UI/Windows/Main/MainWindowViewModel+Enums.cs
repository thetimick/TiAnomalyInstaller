// ⠀
// MainWindowViewModel+Enums.cs
// TiAnomalyInstaller.UI.Avalonia
// 
// Created by the_timick on 22.01.2026.
// ⠀

namespace TiAnomalyInstaller.UI.Avalonia.UI.Windows.Main;

public partial class MainWindowViewModel
{
    public enum MainWindowViewModelType
    {
        None,
        InstallAvailable,
        PlayAvailable,
        UpdateAvailable,
        InProgress,
        Cancelling
    }
    
    public enum ShortcutType
    {
        Desktop
    }
    
    public enum FolderType
    {
        Vanilla,
        Organizer,
        Storage
    }

    public enum DeleteType
    {
        Game,
        Archives
    }
}