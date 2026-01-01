using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using MsBox.Avalonia;
using TiAnomalyInstaller.Logic.Configuration;

namespace TiAnomalyInstaller.UI.Avalonia;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        Task.Factory.StartNew(async () => {
            await Task.Delay(1000);
            await Dispatcher.UIThread.InvokeAsync(() => {
                try
                {
                    var confService = new ConfigurationService();
                    confService.GetLocalConfig(Constants.LocalConfigFileName);
                }
                catch (Exception ex)
                {
                    MessageBoxManager
                        .GetMessageBoxStandard(
                            "Ошибка!",
                            $"Произошла ошибка при получении локального конфига...\n{ex.Message}",
                            icon: MsBox.Avalonia.Enums.Icon.Error
                        )
                        .ShowAsPopupAsync(this);
                }
            });
        });
    }
}