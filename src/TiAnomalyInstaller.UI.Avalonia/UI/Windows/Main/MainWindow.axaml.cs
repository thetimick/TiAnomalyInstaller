using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace TiAnomalyInstaller.UI.Avalonia.UI.Windows.Main;

public partial class MainWindow : Window
{
    private static MainWindowViewModel ViewModel => Program
        .GetRequiredService<MainWindowViewModel>();
    
    public MainWindow()
    {
        #if DEBUG
        this.AttachDevTools();
        #endif
        
        InitializeComponent();
        
        DataContext = ViewModel;
        Loaded += (_, _) => ViewModel.Loaded();
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }
}