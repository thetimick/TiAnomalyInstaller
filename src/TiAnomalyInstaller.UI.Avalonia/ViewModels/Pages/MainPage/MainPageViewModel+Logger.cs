// ⠀
// MainPageViewModel+Logger.cs
// TiAnomalyInstaller.UI.Avalonia
// 
// Created by the_timick on 08.01.2026.
// ⠀

using System;
using Microsoft.Extensions.Logging;

namespace TiAnomalyInstaller.UI.Avalonia.ViewModels.Pages.MainPage;

public partial class MainPageViewModel
{
    [LoggerMessage(Level = LogLevel.Error)]
    private partial void LogError(Exception ex);
    
    [LoggerMessage(Level = LogLevel.Information, Message = "{message}")]
    private partial void LogInfo(string message);
}