// ⠀
// IDialogService.cs
// TiAnomalyInstaller.UI.Avalonia
// 
// Created by the_timick on 08.02.2026.
// ⠀

using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using TiAnomalyInstaller.AppConstants.Localization;
using TiAnomalyInstaller.Logic.Services;

namespace TiAnomalyInstaller.UI.Avalonia.Services;

public interface IDialogService
{
    void Setup(Window window);
    
    Task ShowInfoAsync(string message);
    Task ShowErrorAsync(Exception ex);
    Task ShowErrorAsync(string message);

    Task<ButtonResult> ShowQuestionAsync(string message);
}

public class DialogService : IDialogService
{
    // ────────────────────────────────────────────────
    // Props
    // ────────────────────────────────────────────────
    private Window? _window;

    // ────────────────────────────────────────────────
    // IDialogService
    // ────────────────────────────────────────────────
    
    public void Setup(Window window)
    {
        _window = window;
    }
    
    public async Task ShowInfoAsync(string message)
    {
        if (_window is not { } window)
            return;
        await Dispatcher.UIThread.InvokeAsync(async () => {
            await MessageBoxManager
                    .GetMessageBoxStandard(string.Empty, message, ButtonEnum.Ok, Icon.Info)
                    .ShowAsPopupAsync(window);
        });
    }
    
    public async Task ShowErrorAsync(Exception ex)
    {
        if (_window is not { } window)
            return;
        var content = ex switch {
            InternetUnavailableException => ex.Message,
            _ => string.Format(Strings.mw_alert_error, ex)
        };
        await Dispatcher.UIThread.InvokeAsync(async () => {
            await MessageBoxManager
                .GetMessageBoxStandard(string.Empty, content, ButtonEnum.Ok, Icon.Error)
                .ShowAsPopupAsync(window);
        });
    }

    public async Task ShowErrorAsync(string message)
    {
        if (_window is not { } window)
            return;
        await Dispatcher.UIThread.InvokeAsync(async () => {
            await MessageBoxManager
                .GetMessageBoxStandard(string.Empty, message, ButtonEnum.Ok, Icon.Error)
                .ShowAsPopupAsync(window);
        });
    }

    public async Task<ButtonResult> ShowQuestionAsync(string message)
    {
        if (_window is not { } window)
            return ButtonResult.None;
        return await MessageBoxManager
            .GetMessageBoxStandard(string.Empty, message, ButtonEnum.YesNo, Icon.Question)
            .ShowAsPopupAsync(window);
    }
}