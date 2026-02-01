using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TiAnomalyInstaller.UI.Avalonia.UI.Windows.Main;

public partial class MainWindow : Window
{
    // ─────────────── Props ───────────────
    
    private static MainWindowViewModel ViewModel => Program
        .GetRequiredService<MainWindowViewModel>();
    
    // ─────────────── Lifecycle ───────────────
    
    public MainWindow()
    {
        InitializeComponent();
        
        DataContext = ViewModel;
        Loaded += (_, _) => ViewModel.Loaded();

        ViewModel.PropertyChanged += ViewModelOnPropertyChanged;
    }
    
    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }

    // ─────────────── Private Methods ───────────────
    
    private static void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ViewModel.ViewModelType):
                ViewModel.AdditionallyPanelIsEnabled =
                    ViewModel.ViewModelType != MainWindowViewModel.MainWindowViewModelType.InProgress;
                ViewModel.MenuItemDeleteGameIsVisible = 
                    ViewModel.ViewModelType == MainWindowViewModel.MainWindowViewModelType.PlayAvailable;
            break;
        }
    }
}

// ────────────────────────────────────────────────
// UI Only Properties on ViewModel
// ────────────────────────────────────────────────

public partial class MainWindowViewModel
{
    [ObservableProperty]
    public partial bool AdditionallyPanelIsEnabled { get; set; } = false;
    
    [ObservableProperty]
    public partial bool MenuItemDeleteGameIsVisible { get; set; } = false;
}