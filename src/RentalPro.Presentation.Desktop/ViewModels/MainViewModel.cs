using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalPro.Presentation.Desktop.Views;

namespace RentalPro.Presentation.Desktop.ViewModels;

public partial class MainViewModel(
    DashboardView dashboardView)
    : ObservableObject
{
    [ObservableProperty]
    private string _currentPageTitle = "Дашборд";

    [ObservableProperty]
    private string _selectedMenuItem = "Дашборд";

    [ObservableProperty]
    private object _currentView = dashboardView;

    [ObservableProperty]
    private bool _isMenuCollapsed;

    [RelayCommand]
    private void ToggleMenu()
    {
        IsMenuCollapsed = !IsMenuCollapsed;
    }

    partial void OnSelectedMenuItemChanged(string value)
    {
        CurrentPageTitle = value;

        if (value == "Дашборд")
        {
            CurrentView = dashboardView;
        }
    }
}