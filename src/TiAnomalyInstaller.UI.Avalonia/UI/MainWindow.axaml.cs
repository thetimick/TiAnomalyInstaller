using Avalonia.Controls;
using Avalonia.Input;
using Microsoft.Extensions.DependencyInjection;

namespace TiAnomalyInstaller.UI.Avalonia.UI;

public partial class MainWindow : Window
{
    private static MainWindowViewModel ViewModel => Program
        .GetRequiredService<MainWindowViewModel>();
    
    public MainWindow()
    {
        InitializeComponent();
        
        DataContext = ViewModel;
        Loaded += (_, _) => ViewModel.Loaded();
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }
}