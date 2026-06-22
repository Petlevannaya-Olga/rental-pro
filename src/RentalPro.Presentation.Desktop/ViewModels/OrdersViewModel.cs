using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using RentalPro.Contracts.Orders;
using RentalPro.Presentation.Desktop.Api;
using RentalPro.Presentation.Desktop.Models;
using RentalPro.Presentation.Desktop.Services;

namespace RentalPro.Presentation.Desktop.ViewModels;

public partial class OrdersViewModel(
    OrdersApiClient ordersApiClient,
    DictionariesApiClient dictionariesApiClient,
    NavigationService navigationService,
    NotificationService notificationService)
    : ObservableObject
{
    private const int PageSize = 5;

    private bool _isSelectingStatsFilter;

    public NotificationService Notifications { get; } = notificationService;

    public bool IsAllStatsSelected => SelectedStatsFilter == "all";

    public bool IsReadyStatsSelected => SelectedStatsFilter == "ready";

    public bool IsActiveStatsSelected => SelectedStatsFilter == "active";

    public bool IsClosingStatsSelected => SelectedStatsFilter == "closing";

    [ObservableProperty] private List<OrderDto> _orders = [];

    [ObservableProperty] private List<DictionaryItem> _statuses = [];

    [ObservableProperty] private bool _isLoading;

    [ObservableProperty] private string _errorMessage = string.Empty;

    [ObservableProperty] private string? _search;

    [ObservableProperty] private Guid? _selectedStatusId;

    [ObservableProperty] private DateOnly? _startFrom;

    [ObservableProperty] private DateOnly? _startTo;

    [ObservableProperty] private DateOnly? _endFrom;

    [ObservableProperty] private DateOnly? _endTo;

    [ObservableProperty] private string? _sortBy = "createdat";

    [ObservableProperty] private bool _descending = true;

    [ObservableProperty] private int _currentPage = 1;

    [ObservableProperty] private int _totalCount;

    [ObservableProperty] private int _allOrdersCount;

    [ObservableProperty] private int _readyOrdersCount;

    [ObservableProperty] private int _activeOrdersCount;

    [ObservableProperty] private int _closingOrdersCount;

    [ObservableProperty] private string? _selectedStatsFilter = "all";

    [ObservableProperty] private OrderDto? _selectedOrder;

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

    [RelayCommand]
    public async Task LoadAsync()
    {
        await LoadStatusesAsync();
        await LoadStatsAsync();
        await LoadOrdersAsync();
    }

    [RelayCommand]
    private async Task SelectStatsFilterAsync(string filter)
    {
        if (Statuses.Count == 0)
            await LoadStatusesAsync();

        _isSelectingStatsFilter = true;

        SelectedStatusId = filter switch
        {
            "ready" => FindStatusId("Готов к выдаче"),
            "active" => FindStatusId("Выполняется"),
            "closing" => FindStatusId("Ожидает закрытия аренды"),
            _ => null
        };

        _isSelectingStatsFilter = false;

        SelectedStatsFilter = filter;
        CurrentPage = 1;

        await LoadOrdersAsync();
    }

    [RelayCommand]
    private async Task ApplyFiltersAsync()
    {
        CurrentPage = 1;
        await LoadOrdersAsync();
    }

    [RelayCommand]
    private async Task ResetFiltersAsync()
    {
        Search = string.Empty;
        SelectedStatusId = null;

        StartFrom = null;
        StartTo = null;
        EndFrom = null;
        EndTo = null;

        SelectedStatsFilter = "all";
        CurrentPage = 1;

        await LoadOrdersAsync();
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        SelectedStatsFilter = null;
        CurrentPage = 1;

        await LoadOrdersAsync();
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

        await LoadOrdersAsync();
    }

    [RelayCommand(CanExecute = nameof(CanGoPrevious))]
    private async Task PreviousPageAsync()
    {
        if (CurrentPage <= 1)
            return;

        CurrentPage--;

        await LoadOrdersAsync();
    }

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private async Task NextPageAsync()
    {
        if (CurrentPage >= TotalPages)
            return;

        CurrentPage++;

        await LoadOrdersAsync();
    }

    [RelayCommand]
    private async Task ExportAsync()
    {
        try
        {
            ErrorMessage = string.Empty;
            notificationService.Info("Началась выгрузка заказов в Excel");

            var request = new ExportOrdersRequest(
                Search: Search,
                StatusId: SelectedStatusId,
                StartFrom: StartFrom,
                StartTo: StartTo,
                EndFrom: EndFrom,
                EndTo: EndTo,
                SortBy: SortBy,
                Descending: Descending);

            var result = await ordersApiClient.ExportOrdersAsync(request);

            if (result.IsFailure)
            {
                notificationService.Error(result.Error.Message);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Title = "Сохранить список заказов",
                FileName = "orders.xlsx",
                Filter = "Excel файл (*.xlsx)|*.xlsx"
            };

            if (dialog.ShowDialog() != true)
                return;

            await File.WriteAllBytesAsync(dialog.FileName, result.Value);

            notificationService.Success("Заказы выгружены в Excel");
        }
        catch (Exception ex)
        {
            ErrorMessage = string.Empty;
            notificationService.Error(ex.Message);
        }
    }

    [RelayCommand]
    private void OpenCreateOrder()
    {
        notificationService.Info("Форма создания заказа будет добавлена следующим шагом");
    }

    [RelayCommand]
    private void OpenOrderDetails(OrderDto? order)
    {
        if (order is null)
            return;

        notificationService.Info($"Просмотр заказа {order.Number} будет добавлен следующим шагом");

        // Потом заменим на:
        // orderDetailsViewModel.Open(order.Id);
        // navigationService.NavigateTo<OrderDetailsView>($"Заказ {order.Number}");
    }

    [RelayCommand]
    private void OpenActions(OrderDto? order)
    {
        if (order is null)
            return;

        SelectedOrder = order;
        notificationService.Info($"Действия с заказом {order.Number} будут добавлены следующим шагом");
    }

    private async Task LoadStatusesAsync()
    {
        var result = await dictionariesApiClient.GetListAsync<DictionaryItem>(
            "api/order-statuses",
            "order.statuses.load.failed",
            "Не удалось загрузить статусы заказов");

        if (result.IsFailure)
        {
            Statuses = [];
            notificationService.Error(result.Error.Message);
            return;
        }

        Statuses = result.Value;
    }

    private async Task LoadOrdersAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var request = new GetOrdersRequest(
                Search: Search,
                StatusId: SelectedStatusId,
                StartFrom: StartFrom,
                StartTo: StartTo,
                EndFrom: EndFrom,
                EndTo: EndTo,
                SortBy: SortBy,
                Descending: Descending,
                Page: CurrentPage,
                PageSize: PageSize);

            var result = await ordersApiClient.GetOrdersAsync(request);

            if (result.IsFailure)
            {
                Orders = [];
                TotalCount = 0;
                notificationService.Error(result.Error.Message);
                RefreshPaging();
                return;
            }

            Orders = result.Value.Items.ToList();
            TotalCount = result.Value.TotalCount;

            if (CurrentPage > TotalPages)
            {
                CurrentPage = TotalPages;
                await LoadOrdersAsync();
                return;
            }

            RefreshPaging();
        }
        catch (Exception ex)
        {
            Orders = [];
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
        AllOrdersCount = await GetOrdersCountByStatusAsync(null);
        ReadyOrdersCount = await GetOrdersCountByStatusAsync("Готов к выдаче");
        ActiveOrdersCount = await GetOrdersCountByStatusAsync("Выполняется");
        ClosingOrdersCount = await GetOrdersCountByStatusAsync("Ожидает закрытия аренды");
    }

    private async Task<int> GetOrdersCountByStatusAsync(string? statusName)
    {
        Guid? statusId = null;

        if (!string.IsNullOrWhiteSpace(statusName))
            statusId = FindStatusId(statusName);

        var request = new GetOrdersRequest(
            Search: null,
            StatusId: statusId,
            StartFrom: null,
            StartTo: null,
            EndFrom: null,
            EndTo: null,
            SortBy: "createdat",
            Descending: true,
            Page: 1,
            PageSize: 1);

        var result = await ordersApiClient.GetOrdersAsync(request);

        if (result.IsFailure)
        {
            notificationService.Error(result.Error.Message);
            return 0;
        }

        return result.Value.TotalCount;
    }

    private Guid? FindStatusId(string statusName)
    {
        return Statuses
            .FirstOrDefault(x => string.Equals(
                x.Name?.Trim(),
                statusName,
                StringComparison.OrdinalIgnoreCase))
            ?.Id;
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

    partial void OnSelectedStatsFilterChanged(string? value)
    {
        OnPropertyChanged(nameof(IsAllStatsSelected));
        OnPropertyChanged(nameof(IsReadyStatsSelected));
        OnPropertyChanged(nameof(IsActiveStatsSelected));
        OnPropertyChanged(nameof(IsClosingStatsSelected));
    }

    partial void OnSearchChanged(string? value)
    {
        _ = SearchAsync();
    }

    partial void OnSelectedStatusIdChanged(Guid? value)
    {
        if (_isSelectingStatsFilter)
            return;

        SelectedStatsFilter = null;
        _ = ApplyFiltersAsync();
    }

    partial void OnStartFromChanged(DateOnly? value)
    {
        SelectedStatsFilter = null;
        _ = ApplyFiltersAsync();
    }

    partial void OnStartToChanged(DateOnly? value)
    {
        SelectedStatsFilter = null;
        _ = ApplyFiltersAsync();
    }

    partial void OnEndFromChanged(DateOnly? value)
    {
        SelectedStatsFilter = null;
        _ = ApplyFiltersAsync();
    }

    partial void OnEndToChanged(DateOnly? value)
    {
        SelectedStatsFilter = null;
        _ = ApplyFiltersAsync();
    }
}