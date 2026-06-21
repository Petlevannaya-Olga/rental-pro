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

    [ObservableProperty]
    private List<DashboardReturnItemViewModel> _returns = [];

    [RelayCommand]
    public async Task LoadAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var result = await dashboardApiClient.GetDashboardAsync();

            if (result.IsFailure)
            {
                Dashboard = null;
                Returns = [];
                ErrorMessage = result.Error.Message;
                return;
            }

            Dashboard = result.Value;

            Returns =
            [
                ..Dashboard.OverdueReturns
                    .Select(x => DashboardReturnItemViewModel.FromDto(x, true)),

                ..Dashboard.UpcomingReturns
                    .Select(x => DashboardReturnItemViewModel.FromDto(x, false))
            ];
        }
        finally
        {
            IsLoading = false;
        }
    }
}