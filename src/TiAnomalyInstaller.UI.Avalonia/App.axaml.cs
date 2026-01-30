using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Logging;
using TiAnomalyInstaller.UI.Avalonia.UI.Windows.Main;

namespace TiAnomalyInstaller.UI.Avalonia;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        
        AppDomain.CurrentDomain.UnhandledException += (_, args) => {
            var logger = Program.GetRequiredService<ILogger<App>>();
            if (args.ExceptionObject is Exception exception && logger.IsEnabled(LogLevel.Critical))
                logger.LogCritical("{ex}", exception);
        };
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = Program.GetRequiredService<MainWindow>();;
        base.OnFrameworkInitializationCompleted();
    }
}