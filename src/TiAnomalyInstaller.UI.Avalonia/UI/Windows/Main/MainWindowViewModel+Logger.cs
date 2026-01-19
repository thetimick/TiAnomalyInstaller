// ⠀
// MainWindowViewModel+Logger.cs
// TiAnomalyInstaller.UI.Avalonia
// 
// Created by the_timick on 08.01.2026.
// ⠀

using System;
using Microsoft.Extensions.Logging;

namespace TiAnomalyInstaller.UI.Avalonia.UI.Windows.Main;

public partial class MainWindowViewModel
{
    [LoggerMessage(Level = LogLevel.Error)]
    private partial void LogError(Exception ex);
}