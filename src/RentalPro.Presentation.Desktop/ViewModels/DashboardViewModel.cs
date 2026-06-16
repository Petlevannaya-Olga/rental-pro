using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalPro.Contracts.Dashboard;
using RentalPro.Presentation.Desktop.Api;

namespace RentalPro.Presentation.Desktop.ViewModels;

public partial class DashboardViewModel(
    DashboardApiClient dashboardApiClient)
    : ObservableObject
{
    [ObservableProperty]
    private DashboardDto? _dashboard;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [RelayCommand]
    public async Task LoadAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            Dashboard = await dashboardApiClient.GetDashboardAsync();

            if (Dashboard is null)
            {
                ErrorMessage = "Не удалось загрузить данные дашборда";
            }
        }
        catch
        {
            ErrorMessage = "Не удалось подключиться к серверу";
        }
        finally
        {
            IsLoading = false;
        }
    }
}