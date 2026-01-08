// ⠀
// MainWindowViewModel+Alerts.cs
// TiAnomalyInstaller.UI.Avalonia
// 
// Created by the_timick on 08.01.2026.
// ⠀

using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace TiAnomalyInstaller.UI.Avalonia.UI;

public partial class MainWindowViewModel
{
    private async Task<ButtonResult?> ShowQuestionAsync(string msg)
    {
        if (_lifetime?.MainWindow is not { } window)
            return null;
        
        return await MessageBoxManager
            .GetMessageBoxStandard(
                string.Empty,
                msg,
                ButtonEnum.YesNoCancel,
                Icon.Question
            )
            .ShowAsPopupAsync(window);
    }
    
    private async Task ShowInfoAsync(string message)
    {
        if (_lifetime?.MainWindow is { } window)
        {
            await MessageBoxManager
                .GetMessageBoxStandard(
                    string.Empty,
                    message,
                    ButtonEnum.Ok,
                    Icon.Info
                )
                .ShowAsPopupAsync(window);
        }
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
            ViewStateType = Components.ViewStateType.Error;
            if (_lifetime?.MainWindow is { } window)
            {
                await MessageBoxManager
                    .GetMessageBoxStandard(
                        string.Empty, 
                        $"""
                         Произошла ошибка!
                         Пожалуйста, обратитесь к разработчику.

                         {ex}
                         """,
                        ButtonEnum.Ok, 
                        Icon.Error
                    )
                    .ShowAsPopupAsync(window);
            }
        });
    }
}