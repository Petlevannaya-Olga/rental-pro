using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using RentalPro.Contracts.Customers;
using RentalPro.Presentation.Desktop.Api;
using RentalPro.Presentation.Desktop.Services;
using RentalPro.Presentation.Desktop.Views;

namespace RentalPro.Presentation.Desktop.ViewModels;

public partial class CustomersViewModel(
    CustomersApiClient customersApiClient,
    NavigationService navigationService,
    CustomerEditViewModel customerEditViewModel,
    FakeCustomerGeneratorService fakeCustomerGeneratorService,
    NotificationService notificationService)
    : ObservableObject
{
    private const int PageSize = 5;

    [ObservableProperty]
    private List<CustomerDto> _customers = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string? _search;

    [ObservableProperty]
    private string? _ordersFilter;

    [ObservableProperty]
    private string? _regularFilter;

    [ObservableProperty]
    private string? _activeOrdersFilter;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private int _totalCustomers;

    [ObservableProperty]
    private int _withOrdersCount;

    [ObservableProperty]
    private int _regularCustomersCount;

    [ObservableProperty]
    private int _withActiveOrdersCount;

    [ObservableProperty]
    private string? _sortBy = "createdat";

    [ObservableProperty]
    private bool _descending = true;

    [ObservableProperty]
    private CustomerDto? _selectedCustomer;

    [ObservableProperty]
    private string? _selectedStatsFilter = "all";

    public NotificationService Notifications { get; } = notificationService;

    public int TotalPages =>
        TotalCount <= 0
            ? 1
            : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public int PageStart =>
        TotalCount == 0
            ? 0
            : ((CurrentPage - 1) * PageSize) + 1;

    public int PageEnd =>
        Math.Min(CurrentPage * PageSize, TotalCount);

    public bool CanGoPrevious => CurrentPage > 1;

    public bool CanGoNext => CurrentPage < TotalPages;

    private bool? HasOrders =>
        OrdersFilter switch
        {
            "withOrders" => true,
            "withoutOrders" => false,
            _ => null
        };

    private bool? IsRegular =>
        RegularFilter switch
        {
            "regular" => true,
            "notRegular" => false,
            _ => null
        };

    private bool? HasActiveOrders =>
        ActiveOrdersFilter switch
        {
            "withActiveOrders" => true,
            "withoutActiveOrders" => false,
            _ => null
        };

    [RelayCommand]
    public async Task LoadAsync()
    {
        await LoadStatsAsync();
        await LoadCustomersAsync();
    }

    [RelayCommand]
    private async Task SelectStatsFilterAsync(string filter)
    {
        SelectedStatsFilter = filter;

        switch (filter)
        {
            case "all":
                OrdersFilter = null;
                RegularFilter = null;
                ActiveOrdersFilter = null;
                break;

            case "withOrders":
                OrdersFilter = "withOrders";
                RegularFilter = null;
                ActiveOrdersFilter = null;
                break;

            case "regular":
                OrdersFilter = null;
                RegularFilter = "regular";
                ActiveOrdersFilter = null;
                break;

            case "active":
                OrdersFilter = null;
                RegularFilter = null;
                ActiveOrdersFilter = "withActiveOrders";
                break;
        }

        CurrentPage = 1;

        await LoadCustomersAsync();
    }

    [RelayCommand]
    private void OpenOrdersHistory(CustomerDto? customer)
    {
        if (customer is null)
            return;

        notificationService.Info("История заказов клиента будет добавлена позже");
    }

    [RelayCommand]
    private void OpenCreateCustomer()
    {
        customerEditViewModel.OpenCreate();

        navigationService.NavigateTo<CustomerEditView>("Добавление клиента");
    }

    [RelayCommand]
    private void OpenEditCustomer(CustomerDto? customer)
    {
        if (customer is null)
            return;

        customerEditViewModel.OpenEdit(customer);

        navigationService.NavigateTo<CustomerEditView>("Редактирование клиента");
    }

    [RelayCommand(CanExecute = nameof(CanOpenCustomerDetails))]
    private void OpenCustomerDetails(CustomerDto? customer)
    {
        if (customer is null)
            return;

        customerEditViewModel.OpenView(customer);

        navigationService.NavigateTo<CustomerEditView>("Просмотр клиента");
    }

    private bool CanOpenCustomerDetails(CustomerDto? customer)
    {
        return customer is not null;
    }

    [RelayCommand]
    private async Task GenerateTestCustomersAsync()
    {
        const int count = 10;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            for (var i = 0; i < count; i++)
            {
                var customer = fakeCustomerGeneratorService.Generate();

                var request = new CreateCustomerRequest(
                    customer.LastName,
                    customer.FirstName,
                    customer.MiddleName,
                    customer.PhoneNumber,
                    customer.Email,
                    customer.PassportSeries,
                    customer.PassportNumber,
                    customer.PostalCode,
                    customer.Region,
                    customer.City,
                    customer.Street,
                    customer.House,
                    ToNullable(customer.Building),
                    ToNullable(customer.Apartment));

                var createResult = await customersApiClient.CreateCustomerAsync(request);

                if (createResult.IsFailure)
                {
                    notificationService.Error(createResult.Error.Message);
                    return;
                }
            }

            CurrentPage = 1;
            SelectedStatsFilter = "all";

            await LoadStatsAsync();
            await LoadCustomersAsync();

            notificationService.Success(
                $"Успешно создано {count} тестовых клиентов");
        }
        catch (Exception ex)
        {
            ErrorMessage = string.Empty;
            notificationService.Error(ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ApplyFiltersAsync()
    {
        CurrentPage = 1;
        await LoadCustomersAsync();
    }

    [RelayCommand]
    private async Task ResetFiltersAsync()
    {
        Search = string.Empty;
        OrdersFilter = null;
        RegularFilter = null;
        ActiveOrdersFilter = null;
        SelectedStatsFilter = "all";
        CurrentPage = 1;

        await LoadCustomersAsync();
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        SelectedStatsFilter = null;
        CurrentPage = 1;

        await LoadCustomersAsync();
    }

    [RelayCommand]
    private async Task SortAsync(string sortBy)
    {
        if (SortBy == sortBy)
            Descending = !Descending;
        else
        {
            SortBy = sortBy;
            Descending = false;
        }

        CurrentPage = 1;

        await LoadCustomersAsync();
    }

    [RelayCommand(CanExecute = nameof(CanGoPrevious))]
    private async Task PreviousPageAsync()
    {
        if (CurrentPage <= 1)
            return;

        CurrentPage--;
        await LoadCustomersAsync();
    }

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private async Task NextPageAsync()
    {
        if (CurrentPage >= TotalPages)
            return;

        CurrentPage++;
        await LoadCustomersAsync();
    }

    [RelayCommand]
    private async Task ExportAsync()
    {
        try
        {
            ErrorMessage = string.Empty;
            notificationService.Info("Началась выгрузка клиентов в Excel");

            var request = new ExportCustomersRequest(
                Search,
                HasOrders,
                IsRegular,
                HasActiveOrders,
                SortBy,
                Descending);

            var exportResult = await customersApiClient.ExportCustomersAsync(request);

            if (exportResult.IsFailure)
            {
                notificationService.Error(exportResult.Error.Message);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Title = "Сохранить список клиентов",
                FileName = "customers.xlsx",
                Filter = "Excel файл (*.xlsx)|*.xlsx"
            };

            if (dialog.ShowDialog() != true)
                return;

            await File.WriteAllBytesAsync(dialog.FileName, exportResult.Value);

            notificationService.Success("Клиенты выгружены в Excel");
        }
        catch (Exception ex)
        {
            ErrorMessage = string.Empty;
            notificationService.Error(ex.Message);
        }
    }

    [RelayCommand]
    private async Task DeleteCustomerAsync(CustomerDto? customer)
    {
        if (customer is null)
            return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var deleteResult = await customersApiClient.DeleteCustomerAsync(customer.Id);

            if (deleteResult.IsFailure)
            {
                notificationService.Error(deleteResult.Error.Message);
                return;
            }

            notificationService.Success("Клиент удален");

            await LoadStatsAsync();
            await LoadCustomersAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = string.Empty;
            notificationService.Error(ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadCustomersAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var request = new GetCustomersRequest(
                Search,
                HasOrders,
                IsRegular,
                HasActiveOrders,
                SortBy,
                Descending,
                CurrentPage,
                PageSize);

            var result = await customersApiClient.GetCustomersAsync(request);

            if (result.IsFailure)
            {
                Customers = [];
                TotalCount = 0;
                notificationService.Error(result.Error.Message);
                RefreshPaging();
                return;
            }

            Customers = result.Value.Items.ToList();
            TotalCount = result.Value.TotalCount;

            RefreshPaging();
        }
        catch (Exception ex)
        {
            Customers = [];
            TotalCount = 0;
            ErrorMessage = string.Empty;
            notificationService.Error(ex.Message);

            RefreshPaging();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadStatsAsync()
    {
        try
        {
            var result = await customersApiClient.GetStatsAsync();

            if (result.IsFailure)
            {
                notificationService.Error(result.Error.Message);
                return;
            }

            TotalCustomers = result.Value.TotalCount;
            WithOrdersCount = result.Value.WithOrdersCount;
            RegularCustomersCount = result.Value.RegularCustomersCount;
            WithActiveOrdersCount = result.Value.WithActiveOrdersCount;
        }
        catch (Exception ex)
        {
            ErrorMessage = string.Empty;
            notificationService.Error(ex.Message);
        }
    }

    private void RefreshPaging()
    {
        OnPropertyChanged(nameof(TotalPages));
        OnPropertyChanged(nameof(PageStart));
        OnPropertyChanged(nameof(PageEnd));
        OnPropertyChanged(nameof(CanGoPrevious));
        OnPropertyChanged(nameof(CanGoNext));

        PreviousPageCommand.NotifyCanExecuteChanged();
        NextPageCommand.NotifyCanExecuteChanged();
    }

    private static string? ToNullable(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    partial void OnOrdersFilterChanged(string? value)
    {
        SelectedStatsFilter = null;
        _ = ApplyFiltersAsync();
    }

    partial void OnRegularFilterChanged(string? value)
    {
        SelectedStatsFilter = null;
        _ = ApplyFiltersAsync();
    }

    partial void OnActiveOrdersFilterChanged(string? value)
    {
        SelectedStatsFilter = null;
        _ = ApplyFiltersAsync();
    }

    partial void OnSearchChanged(string? value)
    {
        _ = SearchAsync();
    }

    partial void OnSelectedCustomerChanged(CustomerDto? value)
    {
        OpenCustomerDetailsCommand.NotifyCanExecuteChanged();
    }
}