// ⠀
// App.cs
// TiAnomalyInstaller.UI.Avalonia.Reactive
// 
// Created by the_timick on 15.01.2026.
//

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using TiAnomalyInstaller.UI.Avalonia.Reactive.UI.Windows.Startup;

namespace TiAnomalyInstaller.UI.Avalonia.Reactive;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = Program.GetRequiredService<StartupWindow>();
        base.OnFrameworkInitializationCompleted();
    }
}