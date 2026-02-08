// ⠀
// MainPageViewModel+Enums.cs
// TiAnomalyInstaller.UI.Avalonia
// 
// Created by the_timick on 22.01.2026.
// ⠀

namespace TiAnomalyInstaller.UI.Avalonia.ViewModels.Pages.MainPage;

public partial class MainPageViewModel
{
    public enum MainPageViewModelType
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
    
    public enum OpenType
    {
        Vanilla,
        Organizer,
        Storage,
        Log
    }

    public enum DeleteType
    {
        Game,
        Archives,
        All
    }
}