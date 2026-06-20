using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalPro.Presentation.Desktop.Services;
using RentalPro.Presentation.Desktop.Views;

namespace RentalPro.Presentation.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly NavigationService _navigationService;

    public MainViewModel(NavigationService navigationService)
    {
        _navigationService = navigationService;

        _navigationService.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(NavigationService.CurrentView))
                OnPropertyChanged(nameof(CurrentView));

            if (args.PropertyName == nameof(NavigationService.CurrentPageTitle))
                OnPropertyChanged(nameof(CurrentPageTitle));
        };

        _navigationService.NavigateTo<DashboardView>("Обзор");
    }

    [ObservableProperty]
    private string _selectedMenuItem = "Обзор";

    [ObservableProperty]
    private bool _isMenuCollapsed;

    public object? CurrentView => _navigationService.CurrentView;

    public string CurrentPageTitle => _navigationService.CurrentPageTitle;

    [RelayCommand]
    private void ToggleMenu()
    {
        IsMenuCollapsed = !IsMenuCollapsed;
    }

    partial void OnSelectedMenuItemChanged(string value)
    {
        switch (value)
        {
            case "Обзор":
                _navigationService.NavigateTo<DashboardView>("Обзор");
                break;

            case "Клиенты":
                _navigationService.NavigateTo<CustomersView>("Клиенты");
                break;
        }
    }
}