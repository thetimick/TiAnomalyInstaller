// ⠀
// INavigationService.cs
// TiAnomalyInstaller.UI.Avalonia
// 
// Created by the_timick on 08.02.2026.
// ⠀

using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TiAnomalyInstaller.UI.Avalonia.Components;
using TiAnomalyInstaller.UI.Avalonia.UI.Pages;

namespace TiAnomalyInstaller.UI.Avalonia.Services;

public interface INavigationService
{
    void Setup(Frame frame);
    Task RouteTo(Enums.PageType page);
}

public class NavigationService(
    INavigationPageFactory factory,
    ILogger<NavigationService> logger
): INavigationService {
    private Frame? _frame;
    private Enums.PageType? _currentPage;
    
    public void Setup(Frame frame)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Setup Frame on NavigationService");
        
        _frame = frame;
        _frame.NavigationPageFactory = factory;
    }

    public async Task RouteTo(Enums.PageType page)
    {
        if (_currentPage == page || _frame == null)
            return;
        
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Route from {old} to {new}", _currentPage, page);
        
        await Dispatcher.UIThread.InvokeAsync(() => {
            _frame.NavigateFromObject(page);
            _currentPage = page; 
        });
    }
}

public class NavigationPageFactory(IServiceProvider provider): INavigationPageFactory
{
    public Control GetPage(Type srcType)
    {
        throw new NotImplementedException();
    }
    
    public Control GetPageFromObject(object target)
    {
        if (target is not Enums.PageType type)
            throw new ArgumentException(null, nameof(target));
        return type switch {
            Enums.PageType.Init => provider.GetRequiredService<InitPage>(),
            Enums.PageType.Loading => provider.GetRequiredService<LoadingPage>(),
            Enums.PageType.Main => provider.GetRequiredService<MainPage>(),
            _ => throw new ArgumentOutOfRangeException(nameof(target))
        };
    }
}