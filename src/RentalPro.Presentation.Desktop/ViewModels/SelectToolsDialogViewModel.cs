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
    private List<ToolDto> tools = [];

    [ObservableProperty]
    private HashSet<Guid> selectedToolIds = [];

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

    public List<ToolDto> Result { get; private set; } = [];

    public int SelectedToolsCount => SelectedToolIds.Count;

    public int TotalPages =>
        TotalCount <= 0
            ? 1
            : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public async Task LoadAsync(IEnumerable<Guid> selectedIds)
    {
        SelectedToolIds = selectedIds.ToHashSet();

        await LoadAvailableStatusAsync();
        await LoadToolsAsync();

        OnPropertyChanged(nameof(SelectedToolsCount));
    }

    [RelayCommand]
    private void ToggleTool(ToolDto? tool)
    {
        if (tool is null)
            return;

        if (!SelectedToolIds.Add(tool.Id))
            SelectedToolIds.Remove(tool.Id);

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
        Result = Tools
            .Where(x => SelectedToolIds.Contains(x.Id))
            .ToList();
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

        Tools = result.Value.Items.ToList();
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