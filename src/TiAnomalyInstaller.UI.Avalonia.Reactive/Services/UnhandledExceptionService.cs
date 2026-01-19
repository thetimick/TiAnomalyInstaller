// ⠀
// UnhandledExceptionService.cs
// TiAnomalyInstaller.UI.Avalonia.Reactive
// 
// Created by the_timick on 17.01.2026.
// ⠀

using System;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace TiAnomalyInstaller.UI.Avalonia.Reactive.Services;

internal static partial class UnhandledExceptionService
{
    internal static void Setup(ILogger<StartupService> logger)
    {
        AppDomain.CurrentDomain.UnhandledException += (_, args) => {
            if (args.ExceptionObject is not Exception ex)
                return;
            
            logger.LogError("{ex}", ex.ToString());
            MessageBox(IntPtr.Zero, ex.Message, "UnhandledException", 0x00000000 | 0x00000010);
        };
    }
    
    [LibraryImport("user32.dll", EntryPoint = "MessageBoxW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial void MessageBox(
        IntPtr hWnd,
        string lpText,
        string lpCaption,
        uint uType
    );
}