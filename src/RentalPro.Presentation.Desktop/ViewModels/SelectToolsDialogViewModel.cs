using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalPro.Contracts.Tools;
using RentalPro.Presentation.Desktop.Api;
using RentalPro.Presentation.Desktop.Models;
using RentalPro.Presentation.Desktop.Services;

namespace RentalPro.Presentation.Desktop.ViewModels;

public partial class SelectToolsDialogViewModel(
    ToolsApiClient toolsApiClient,
    DictionariesApiClient dictionariesApiClient,
    NotificationService notificationService)
    : ObservableObject
{
    private const int PageSize = 5;

    private Guid? _availableStatusId;
    private CancellationTokenSource? _searchCts;

    [ObservableProperty]
    private List<SelectableToolModel> tools = [];

    [ObservableProperty]
    private HashSet<Guid> selectedToolIds = [];

    private readonly Dictionary<Guid, SelectedToolModel> _selectedTools = [];

    public List<SelectedToolModel> Result { get; private set; } = [];

    public int SelectedToolsCount => _selectedTools.Count;

    [ObservableProperty]
    private string? search;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private int currentPage = 1;

    [ObservableProperty]
    private int totalCount;

    [ObservableProperty]
    private string sortBy = "name";

    [ObservableProperty]
    private bool descending;

    public int TotalPages =>
        TotalCount <= 0
            ? 1
            : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public async Task LoadAsync(IEnumerable<OrderToolEditModel> selectedTools)
    {
        _selectedTools.Clear();

        foreach (var tool in selectedTools)
        {
            _selectedTools[tool.ToolId] = new SelectedToolModel
            {
                ToolId = tool.ToolId,
                ToolName = tool.ToolName,
                RentalPricePerDay = tool.RentalPricePerDay,
                DepositAmount = tool.DepositAmount
            };
        }

        SelectedToolIds = _selectedTools.Keys.ToHashSet();

        await LoadAvailableStatusAsync();
        await LoadToolsAsync();

        OnPropertyChanged(nameof(SelectedToolsCount));
    }

    [RelayCommand]
    private void ToggleTool(SelectableToolModel? item)
    {
        if (item is null)
            return;

        item.IsSelected = !item.IsSelected;

        if (item.IsSelected)
        {
            _selectedTools[item.Id] = new SelectedToolModel
            {
                ToolId = item.Id,
                ToolName = item.Name,
                RentalPricePerDay = item.RentalPricePerDay,
                DepositAmount = item.DepositAmount,
                InventoryNumber = item.InventoryNumber,
                SerialNumber = item.SerialNumber
            };

            SelectedToolIds.Add(item.Id);
        }
        else
        {
            _selectedTools.Remove(item.Id);
            SelectedToolIds.Remove(item.Id);
        }

        SelectedToolIds = SelectedToolIds.ToHashSet();

        OnPropertyChanged(nameof(SelectedToolsCount));
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (CurrentPage >= TotalPages)
            return;

        CurrentPage++;
        await LoadToolsAsync();
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (CurrentPage <= 1)
            return;

        CurrentPage--;
        await LoadToolsAsync();
    }

    [RelayCommand]
    private void Confirm()
    {
        Result = _selectedTools.Values.ToList();
    }

    public bool IsSelected(Guid toolId)
    {
        return SelectedToolIds.Contains(toolId);
    }

    private async Task LoadAvailableStatusAsync()
    {
        if (_availableStatusId is not null)
            return;

        var result = await dictionariesApiClient.GetListAsync<DictionaryItem>(
            "api/tool-statuses",
            "tool.statuses.load.failed",
            "Не удалось загрузить статусы инструментов");

        if (result.IsFailure)
        {
            notificationService.Error(result.Error.Message);
            return;
        }

        _availableStatusId = result.Value
            .FirstOrDefault(x => x.Name == "Доступен")
            ?.Id;

        if (_availableStatusId is null)
            notificationService.Error("Статус инструмента «Доступен» не найден");
    }

    private async Task LoadToolsAsync()
    {
        if (_availableStatusId is null)
            return;

        IsLoading = true;

        var request = new GetToolsRequest(
            Search: Search,
            CategoryId: null,
            ManufacturerId: null,
            StatusId: _availableStatusId,
            SortBy: SortBy,
            Descending: Descending,
            Page: CurrentPage,
            PageSize: PageSize);

        var result = await toolsApiClient.GetToolsAsync(request);

        IsLoading = false;

        if (result.IsFailure)
        {
            Tools = [];
            TotalCount = 0;
            notificationService.Error(result.Error.Message);
            return;
        }

        Tools = result.Value.Items
            .Select(x => new SelectableToolModel(
                x,
                SelectedToolIds.Contains(x.Id)))
            .ToList();
        
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
            await LoadToolsAsync();
        }
        catch (OperationCanceledException)
        {
        }
    }
}