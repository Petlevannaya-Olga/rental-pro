using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalPro.Contracts.Customers;
using RentalPro.Presentation.Desktop.Api;
using RentalPro.Presentation.Desktop.Services;

namespace RentalPro.Presentation.Desktop.ViewModels;

public partial class SelectCustomerDialogViewModel(
    CustomersApiClient customersApiClient,
    NotificationService notificationService)
    : ObservableObject
{
    private const int PageSize = 5;
    
    private CancellationTokenSource? _searchCts;

    [ObservableProperty]
    private List<CustomerDto> customers = [];

    [ObservableProperty]
    private CustomerDto? selectedCustomer;

    [ObservableProperty]
    private string? search;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private int currentPage = 1;

    [ObservableProperty]
    private int totalCount;

    [ObservableProperty]
    private string sortBy = "fullname";

    [ObservableProperty]
    private bool descending;

    public CustomerDto? Result { get; private set; }

    public int TotalPages =>
        TotalCount <= 0 ? 1 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public async Task LoadAsync()
    {
        await LoadCustomersAsync();
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        CurrentPage = 1;
        await LoadCustomersAsync();
    }

    [RelayCommand]
    private void Select()
    {
        if (SelectedCustomer is null)
        {
            notificationService.Error("Выберите клиента");
            return;
        }

        Result = SelectedCustomer;
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (CurrentPage >= TotalPages)
            return;

        CurrentPage++;
        await LoadCustomersAsync();
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (CurrentPage <= 1)
            return;

        CurrentPage--;
        await LoadCustomersAsync();
    }

    private async Task LoadCustomersAsync()
    {
        IsLoading = true;

        var request = new GetCustomersRequest(
            Search: Search,
            HasOrders: null,
            HasActiveOrders: null,
            IsRegular: null,
            SortBy: SortBy,
            Descending: Descending,
            Page: CurrentPage,
            PageSize: PageSize);

        var result = await customersApiClient.GetCustomersAsync(request);

        IsLoading = false;

        if (result.IsFailure)
        {
            Customers = [];
            TotalCount = 0;
            notificationService.Error(result.Error.Message);
            return;
        }

        Customers = result.Value.Items.ToList();
        TotalCount = result.Value.TotalCount;
        
        OnPropertyChanged(nameof(TotalPages));
    }
    
    partial void OnSearchChanged(string? value)
    {
        _ = SearchWithDelayAsync();
    }

    private async Task SearchWithDelayAsync()
    {
        _searchCts?.Cancel();
        _searchCts?.Dispose();

        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        try
        {
            await Task.Delay(500, token);

            CurrentPage = 1;

            await LoadCustomersAsync();
        }
        catch (OperationCanceledException)
        {
            // ignored
        }
    }
}