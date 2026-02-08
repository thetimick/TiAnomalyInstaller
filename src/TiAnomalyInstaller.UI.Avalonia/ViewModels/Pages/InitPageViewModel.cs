// ⠀
// InitPageViewModel.cs
// TiAnomalyInstaller.UI.Avalonia
// 
// Created by the_timick on 01.02.2026.
// ⠀

using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TiAnomalyInstaller.Logic.Services;
using TiAnomalyInstaller.UI.Avalonia.Components;
using TiAnomalyInstaller.UI.Avalonia.Components.Interfaces;
using TiAnomalyInstaller.UI.Avalonia.Extensions;
using TiAnomalyInstaller.UI.Avalonia.Services;

namespace TiAnomalyInstaller.UI.Avalonia.ViewModels.Pages;

public partial class InitPageViewModel(
    INavigationService navigationService,
    IDialogService dialogService,
    IStorageService storageService,
    IConfigService configService
): ObservableObject, ILoadable {
    // ────────────────────────────────────────────────
    // Props
    // ────────────────────────────────────────────────
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TapOnNextButtonCommand))]
    public partial string Url { get; set; } = string.Empty;
    
    private bool IsNextButtonEnabled => Url.IsValidUrl();

    // ────────────────────────────────────────────────
    // Methods
    // ────────────────────────────────────────────────'

    public Task LoadContentAsync()
    {
        return Task.CompletedTask;
    }
}

// ────────────────────────────────────────────────
// Commands
// ────────────────────────────────────────────────

public partial class InitPageViewModel
{
    [RelayCommand(CanExecute = nameof(IsNextButtonEnabled))]
    private async Task TapOnNextButton()
    {
        try
        {
            await configService.ObtainRemoteConfigAsync(Url, true);
            storageService.Set(StorageServiceKey.ProfileUrl, Url);
            
            await navigationService.RouteTo(Enums.PageType.Main);
        }
        catch
        {
            await dialogService.ShowErrorAsync("Произошла ошибка при получении конфига.\nСкорее всего, вы указали неверную ссылку.");
        }
    }
    
    [RelayCommand]
    private void TapOnExitButton()
    {
        Program.GetLifetime()?.TryShutdown();
    }
}