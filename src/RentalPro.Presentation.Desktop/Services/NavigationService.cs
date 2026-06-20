using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace RentalPro.Presentation.Desktop.Services;

public partial class NavigationService(IServiceProvider serviceProvider)
    : ObservableObject
{
    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private string _currentPageTitle = "Обзор";

    public void NavigateTo<TView>(string title)
        where TView : class
    {
        CurrentView = serviceProvider.GetRequiredService<TView>();
        CurrentPageTitle = title;
    }
}