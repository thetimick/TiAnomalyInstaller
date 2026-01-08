using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using TiAnomalyInstaller.UI.Avalonia.UI;

namespace TiAnomalyInstaller.UI.Avalonia;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = Program.GetRequiredService<MainWindow>();
        
        base.OnFrameworkInitializationCompleted();
    }
}