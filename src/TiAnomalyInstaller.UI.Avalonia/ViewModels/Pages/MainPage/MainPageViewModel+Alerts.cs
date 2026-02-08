// ⠀
// MainPageViewModel+Alerts.cs
// TiAnomalyInstaller.UI.Avalonia
// 
// Created by the_timick on 08.01.2026.
// ⠀

using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using TiAnomalyInstaller.AppConstants.Localization;
using TiAnomalyInstaller.Logic.Services;

namespace TiAnomalyInstaller.UI.Avalonia.ViewModels.Pages.MainPage;

public partial class MainPageViewModel
{
    public async Task<ButtonResult> ShowQuestionOnMainAsync(string message)
    {
        return await Dispatcher.UIThread.InvokeAsync(async () => await ShowQuestionAsync(message));
    }
    
    public async Task<ButtonResult> ShowQuestionAsync(string message)
    {
        if (_lifetime?.MainWindow is not { } window)
            return ButtonResult.None;
        return await MessageBoxManager
            .GetMessageBoxStandard(
                string.Empty,
                message,
                ButtonEnum.YesNo,
                Icon.Question
            )
            .ShowAsPopupAsync(window);
    } 
    
    public async Task ShowInfoOnMainAsync(string message)
    {
        await Dispatcher.UIThread.InvokeAsync(async () => await ShowInfoAsync(message));
    }
    
    private async Task ShowInfoAsync(string message)
    {
        if (_lifetime?.MainWindow is not { } window)
            return;
        await MessageBoxManager
            .GetMessageBoxStandard(
                string.Empty,
                message,
                ButtonEnum.Ok,
                Icon.Info
            )
            .ShowAsPopupAsync(window);
    }
    
    private async Task ShowErrorWithExitAsync(Exception ex)
    {
        await ShowErrorAsync(ex);
        _lifetime?.TryShutdown();
    }

    private async Task ShowErrorAsync(Exception ex)
    {
        LogError(ex);
        await Dispatcher.UIThread.InvokeAsync(async () => {
            if (_lifetime?.MainWindow is not { } window)
                return;
            var content = ex switch
            {
                InternetUnavailableException => ex.Message,
                _ => string.Format(Strings.mw_alert_error, ex)
            };
            await MessageBoxManager
                .GetMessageBoxStandard(string.Empty, content, ButtonEnum.Ok, Icon.Error)
                .ShowAsPopupAsync(window);
        });
    }
}