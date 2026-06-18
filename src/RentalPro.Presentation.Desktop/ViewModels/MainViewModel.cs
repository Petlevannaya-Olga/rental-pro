using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalPro.Presentation.Desktop.Views;

namespace RentalPro.Presentation.Desktop.ViewModels;

public partial class MainViewModel(
    DashboardView dashboardView,
    CustomersView customersView)
    : ObservableObject
{
    [ObservableProperty]
    private string _currentPageTitle = "Обзор";

    [ObservableProperty]
    private string _selectedMenuItem = "Обзор";

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

        CurrentView = value switch
        {
            "Обзор" => dashboardView,
            "Клиенты" => customersView,
            _ => CurrentView
        };
    }
}