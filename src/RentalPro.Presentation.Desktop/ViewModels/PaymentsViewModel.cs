using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using RentalPro.Contracts.PaymentMethods;
using RentalPro.Contracts.Payments;
using RentalPro.Contracts.PaymentTypes;
using RentalPro.Presentation.Desktop.Api;
using RentalPro.Presentation.Desktop.Services;

namespace RentalPro.Presentation.Desktop.ViewModels;

public partial class PaymentsViewModel(
    PaymentsApiClient paymentsApiClient,
    DictionariesApiClient dictionariesApiClient,
    NotificationService notificationService)
    : ObservableObject
{
    private const int PageSize = 5;

    private CancellationTokenSource? _searchCts;
    private bool _isLoaded;
    private string _activeStatsFilter = "all";

    [ObservableProperty]
    private IReadOnlyList<PaymentDto> _payments = [];

    [ObservableProperty]
    private IReadOnlyList<PaymentTypeDto> _paymentTypes = [];

    [ObservableProperty]
    private IReadOnlyList<PaymentMethodDto> _paymentMethods = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _search;

    [ObservableProperty]
    private Guid? _paymentTypeId;

    [ObservableProperty]
    private Guid? _paymentMethodId;

    [ObservableProperty]
    private DateTime? _dateFrom;

    [ObservableProperty]
    private DateTime? _dateTo;

    [ObservableProperty]
    private string? _sortBy = "paymentdate";

    [ObservableProperty]
    private bool _descending = true;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private int _rentalCount;

    [ObservableProperty]
    private int _depositCount;

    [ObservableProperty]
    private int _depositRefundCount;

    [ObservableProperty]
    private decimal _totalAmount;

    public bool IsAllStatsSelected => _activeStatsFilter == "all";
    public bool IsRentalStatsSelected => _activeStatsFilter == "rental";
    public bool IsDepositStatsSelected => _activeStatsFilter == "deposit";
    public bool IsRefundStatsSelected => _activeStatsFilter == "refund";

    public int PageStart =>
        TotalCount == 0
            ? 0
            : ((CurrentPage - 1) * PageSize) + 1;

    public int PageEnd =>
        Math.Min(CurrentPage * PageSize, (int)TotalCount);

    [RelayCommand]
    public async Task LoadAsync()
    {
        _isLoaded = false;

        await LoadDictionariesAsync();
        await LoadStatsAsync();

        _isLoaded = true;

        await LoadPaymentsAsync();
    }

    [RelayCommand]
    private async Task ResetFiltersAsync()
    {
        Search = string.Empty;
        PaymentTypeId = null;
        PaymentMethodId = null;
        DateFrom = null;
        DateTo = null;

        SortBy = "paymentdate";
        Descending = true;
        CurrentPage = 1;

        _activeStatsFilter = "all";
        RefreshStatsSelection();

        await LoadPaymentsAsync();
    }

    [RelayCommand]
    private async Task SelectStatsFilterAsync(string filter)
    {
        _activeStatsFilter = filter;

        PaymentTypeId = filter switch
        {
            "rental" => GetPaymentTypeIdByName("Аренда"),
            "deposit" => GetPaymentTypeIdByName("Залог"),
            "refund" => GetPaymentTypeIdByName("Возврат залога"),
            _ => null
        };

        CurrentPage = 1;

        RefreshStatsSelection();

        await LoadPaymentsAsync();
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

        await LoadPaymentsAsync();
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (CurrentPage <= 1)
            return;

        CurrentPage--;

        await LoadPaymentsAsync();
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (CurrentPage >= TotalPages)
            return;

        CurrentPage++;

        await LoadPaymentsAsync();
    }

    [RelayCommand]
    private async Task ExportAsync()
    {
        var dialog = new SaveFileDialog
        {
            Title = "Экспорт оплат",
            FileName = "payments.xlsx",
            Filter = "Excel файл (*.xlsx)|*.xlsx"
        };

        if (dialog.ShowDialog() != true)
            return;

        var request = new ExportPaymentsRequest
        {
            Search = Search,
            PaymentTypeId = PaymentTypeId,
            PaymentMethodId = PaymentMethodId,
            DateFrom = DateFrom,
            DateTo = DateTo,
            SortBy = SortBy,
            Descending = Descending
        };

        var result = await paymentsApiClient.ExportPaymentsAsync(request);

        if (result.IsFailure)
        {
            notificationService.Error(result.Error.Message);
            return;
        }

        await File.WriteAllBytesAsync(dialog.FileName, result.Value);

        notificationService.Success("Оплаты экспортированы");
    }

    private async Task LoadDictionariesAsync()
    {
        var typesResult = await dictionariesApiClient.GetListAsync<PaymentTypeDto>(
            "api/payment-types",
            "payment.types.get.failed",
            "Не удалось загрузить типы оплат");

        PaymentTypes = typesResult.IsSuccess
            ? typesResult.Value
            : [];

        if (typesResult.IsFailure)
            notificationService.Error(typesResult.Error.Message);

        var methodsResult = await dictionariesApiClient.GetListAsync<PaymentMethodDto>(
            "api/payment-methods",
            "payment.methods.get.failed",
            "Не удалось загрузить способы оплат");

        PaymentMethods = methodsResult.IsSuccess
            ? methodsResult.Value
            : [];

        if (methodsResult.IsFailure)
            notificationService.Error(methodsResult.Error.Message);
    }

    private async Task LoadStatsAsync()
    {
        var result = await paymentsApiClient.GetStatsAsync();

        if (result.IsFailure)
        {
            notificationService.Error(result.Error.Message);
            return;
        }

        TotalCount = result.Value.TotalCount;
        RentalCount = result.Value.RentalCount;
        DepositCount = result.Value.DepositCount;
        DepositRefundCount = result.Value.DepositRefundCount;
        TotalAmount = result.Value.TotalAmount;
    }

    private async Task LoadPaymentsAsync()
    {
        IsLoading = true;

        var request = new GetPaymentsRequest
        {
            Search = Search,
            PaymentTypeId = PaymentTypeId,
            PaymentMethodId = PaymentMethodId,
            DateFrom = DateFrom,
            DateTo = DateTo,
            SortBy = SortBy,
            Descending = Descending,
            Page = CurrentPage,
            PageSize = PageSize
        };

        var result = await paymentsApiClient.GetPaymentsAsync(request);

        if (result.IsFailure)
        {
            Payments = [];
            TotalCount = 0;
            TotalPages = 0;

            IsLoading = false;

            RefreshPagingProperties();
            notificationService.Error(result.Error.Message);

            return;
        }

        Payments = result.Value.Items.ToList();
        TotalCount = result.Value.TotalCount;
        TotalPages = result.Value.TotalPages;

        IsLoading = false;

        RefreshPagingProperties();
    }

    private async Task SearchOnFlyAsync()
    {
        _searchCts?.Cancel();
        _searchCts?.Dispose();

        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        try
        {
            await Task.Delay(500, token);

            CurrentPage = 1;

            await LoadPaymentsAsync();
        }
        catch (OperationCanceledException)
        {
            // ignored
        }
    }

    private async Task ApplyFiltersOnFlyAsync()
    {
        CurrentPage = 1;

        await LoadPaymentsAsync();
    }

    private Guid? GetPaymentTypeIdByName(string name)
    {
        return Enumerable
            .FirstOrDefault<PaymentTypeDto>(PaymentTypes, x => x.Name == name)
            ?.Id;
    }

    private void RefreshStatsSelection()
    {
        OnPropertyChanged(nameof(IsAllStatsSelected));
        OnPropertyChanged(nameof(IsRentalStatsSelected));
        OnPropertyChanged(nameof(IsDepositStatsSelected));
        OnPropertyChanged(nameof(IsRefundStatsSelected));
    }

    private void RefreshPagingProperties()
    {
        OnPropertyChanged(nameof(PageStart));
        OnPropertyChanged(nameof(PageEnd));
    }

    partial void OnSearchChanged(string? value)
    {
        if (!_isLoaded)
            return;

        _ = SearchOnFlyAsync();
    }

    partial void OnPaymentTypeIdChanged(Guid? value)
    {
        if (!_isLoaded)
            return;

        _ = ApplyFiltersOnFlyAsync();
    }

    partial void OnPaymentMethodIdChanged(Guid? value)
    {
        if (!_isLoaded)
            return;

        _ = ApplyFiltersOnFlyAsync();
    }

    partial void OnDateFromChanged(DateTime? value)
    {
        if (!_isLoaded)
            return;

        _ = ApplyFiltersOnFlyAsync();
    }

    partial void OnDateToChanged(DateTime? value)
    {
        if (!_isLoaded)
            return;

        _ = ApplyFiltersOnFlyAsync();
    }

    partial void OnCurrentPageChanged(int value)
    {
        RefreshPagingProperties();
    }

    partial void OnTotalCountChanged(int value)
    {
        RefreshPagingProperties();
    }
}