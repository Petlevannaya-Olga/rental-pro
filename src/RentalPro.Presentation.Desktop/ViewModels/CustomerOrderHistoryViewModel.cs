using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalPro.Contracts.Customers;
using RentalPro.Presentation.Desktop.Api;
using RentalPro.Presentation.Desktop.Services;
using RentalPro.Presentation.Desktop.Views;

namespace RentalPro.Presentation.Desktop.ViewModels;

public partial class CustomerOrderHistoryViewModel(
    CustomersApiClient customersApiClient,
    NavigationService navigationService,
    NotificationService notificationService)
    : ObservableObject
{
    [ObservableProperty]
    private string _customerFullName = string.Empty;

    [ObservableProperty]
    private List<CustomerOrderHistoryItemDto> _orders = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    private Guid _customerId;

    public async Task OpenAsync(CustomerDto customer)
    {
        _customerId = customer.Id;
        CustomerFullName = customer.FullName;

        await LoadAsync();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var result = await customersApiClient.GetOrderHistoryAsync(_customerId);

            if (result.IsFailure)
            {
                Orders = [];
                ErrorMessage = result.Error.Message;
                notificationService.Error(result.Error.Message);
                return;
            }

            Orders = result.Value;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Back()
    {
        navigationService.NavigateTo<CustomersView>("Клиенты");
    }
}