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

        OpenEditCustomer(customer);
    }

    private bool CanOpenCustomerDetails(CustomerDto? customer)
    {
        return customer is not null;
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
        CurrentPage = 1;

        await LoadCustomersAsync();
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
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
            notificationService.Info("Началась выгрузка в Excel");
            ErrorMessage = string.Empty;

            var request = new ExportCustomersRequest(
                Search,
                HasOrders,
                IsRegular,
                HasActiveOrders,
                SortBy,
                Descending);

            var bytes = await customersApiClient.ExportCustomersAsync(request);

            var dialog = new SaveFileDialog
            {
                Title = "Сохранить список клиентов",
                FileName = "customers.xlsx",
                Filter = "Excel файл (*.xlsx)|*.xlsx"
            };

            if (dialog.ShowDialog() != true)
                return;

            await File.WriteAllBytesAsync(dialog.FileName, bytes);
            notificationService.Success("Клиенты выгружены в Excel");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            notificationService.Error("Не удалось выгрузить клиентов в Excel");
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

            Customers = result.Items.ToList();
            TotalCount = result.TotalCount;

            OnPropertyChanged(nameof(TotalPages));
            OnPropertyChanged(nameof(PageStart));
            OnPropertyChanged(nameof(PageEnd));
            OnPropertyChanged(nameof(CanGoPrevious));
            OnPropertyChanged(nameof(CanGoNext));

            PreviousPageCommand.NotifyCanExecuteChanged();
            NextPageCommand.NotifyCanExecuteChanged();
        }
        catch (Exception ex)
        {
            Customers = [];
            TotalCount = 0;
            ErrorMessage = ex.Message;
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
            var stats = await customersApiClient.GetStatsAsync();

            TotalCustomers = stats.TotalCount;
            WithOrdersCount = stats.WithOrdersCount;
            RegularCustomersCount = stats.RegularCustomersCount;
            WithActiveOrdersCount = stats.WithActiveOrdersCount;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    partial void OnOrdersFilterChanged(string? value)
    {
        _ = ApplyFiltersAsync();
    }

    partial void OnRegularFilterChanged(string? value)
    {
        _ = ApplyFiltersAsync();
    }

    partial void OnActiveOrdersFilterChanged(string? value)
    {
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