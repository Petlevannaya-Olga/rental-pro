using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using RentalPro.Presentation.Desktop.Auth;
using RentalPro.Presentation.Desktop.Services;
using RentalPro.Presentation.Desktop.Views;

namespace RentalPro.Presentation.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly NavigationService _navigationService;
    private readonly NotificationService _notificationService;
    private readonly TokenStorage _tokenStorage;
    private readonly IServiceProvider _serviceProvider;
    
    public string CurrentUserName => _tokenStorage.ManagerFullName;

    public string CurrentUserRole => _tokenStorage.Role ?? "Пользователь";

    public MainViewModel(
        NavigationService navigationService,
        NotificationService notificationService,
        IServiceProvider serviceProvider,
        TokenStorage tokenStorage)
    {
        Notifications = notificationService;

        _navigationService = navigationService;
        _notificationService = notificationService;
        _tokenStorage = tokenStorage;
        _serviceProvider = serviceProvider;

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

    public NotificationService Notifications { get; }

    public bool CanOpenAdminPages => _tokenStorage.IsAdmin;

    [RelayCommand]
    private void Logout()
    {
        _tokenStorage.Clear();

        var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();

        Application.Current.MainWindow = loginWindow;

        loginWindow.Show();

        foreach (Window window in Application.Current.Windows)
        {
            if (window is MainWindow)
            {
                window.Close();
                break;
            }
        }
    }
    
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

            case "Инструменты":
                _navigationService.NavigateTo<ToolsView>("Инструменты");
                break;

            case "Заказы":
                _navigationService.NavigateTo<OrdersView>("Заказы");
                break;

            case "Оплаты":
                if (!EnsureAdmin())
                    return;

                _navigationService.NavigateTo<PaymentsView>("Оплаты");
                break;

            case "Отчеты":
                if (!EnsureAdmin())
                    return;

                _navigationService.NavigateTo<ReportsView>("Отчеты");
                break;

            case "Справочники":
                if (!EnsureAdmin())
                    return;

                _navigationService.NavigateTo<DictionariesView>("Справочники");
                break;
        }
    }

    private bool EnsureAdmin()
    {
        if (_tokenStorage.IsAdmin)
            return true;

        _notificationService.Error("У вас нет прав для открытия этого раздела");

        SelectedMenuItem = CurrentPageTitle;

        return false;
    }
}