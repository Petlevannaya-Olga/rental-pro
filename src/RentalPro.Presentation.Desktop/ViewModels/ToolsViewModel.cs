using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using RentalPro.Contracts.Tools;
using RentalPro.Presentation.Desktop.Api;
using RentalPro.Presentation.Desktop.Models;
using RentalPro.Presentation.Desktop.Services;
using RentalPro.Presentation.Desktop.Views;

namespace RentalPro.Presentation.Desktop.ViewModels;

public partial class ToolsViewModel(
    ToolsApiClient toolsApiClient,
    DictionariesApiClient dictionariesApiClient,
    FakeToolGeneratorService fakeToolGeneratorService,
    ToolEditViewModel toolEditViewModel,
    NavigationService navigationService,
    NotificationService notificationService)
    : ObservableObject
{
    private const int PageSize = 5;

    private readonly Random _random = new();

    private bool _isSelectingStatsFilter;

    public NotificationService Notifications { get; } = notificationService;

    public bool IsAllStatsSelected => SelectedStatsFilter == "all";

    public bool IsAvailableStatsSelected => SelectedStatsFilter == "available";

    public bool IsRentedStatsSelected => SelectedStatsFilter == "rented";

    public bool IsRepairStatsSelected => SelectedStatsFilter == "repair";

    [ObservableProperty]
    private List<ToolDto> _tools = [];

    [ObservableProperty]
    private List<DictionaryItem> _categories = [];

    [ObservableProperty]
    private List<DictionaryItem> _manufacturers = [];

    [ObservableProperty]
    private List<DictionaryItem> _statuses = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string? _search;

    [ObservableProperty]
    private Guid? _categoryId;

    [ObservableProperty]
    private Guid? _manufacturerId;

    [ObservableProperty]
    private Guid? _statusId;

    [ObservableProperty]
    private string? _sortBy = "createdat";

    [ObservableProperty]
    private bool _descending = true;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private int _totalTools;

    [ObservableProperty]
    private int _availableCount;

    [ObservableProperty]
    private int _rentedCount;

    [ObservableProperty]
    private int _repairCount;

    [ObservableProperty]
    private string? _selectedStatsFilter = "all";

    [ObservableProperty]
    private ToolDto? _selectedTool;

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
        await LoadDictionariesAsync();
        await LoadStatsAsync();
        await LoadToolsAsync();
    }

    [RelayCommand]
    private async Task SelectStatsFilterAsync(string filter)
    {
        if (Statuses.Count == 0)
            await LoadDictionariesAsync();

        var statusId = filter switch
        {
            "available" => FindStatusId("Доступен"),
            "rented" => FindStatusId("В аренде"),
            "repair" => FindStatusId("На ремонте"),
            _ => null
        };

        if (filter != "all" && statusId is null)
        {
            notificationService.Error("Не найден статус для выбранного фильтра");
            return;
        }

        _isSelectingStatsFilter = true;

        StatusId = statusId;

        _isSelectingStatsFilter = false;

        SelectedStatsFilter = filter;
        CurrentPage = 1;

        await LoadToolsAsync();
    }

    [RelayCommand]
    private async Task ApplyFiltersAsync()
    {
        CurrentPage = 1;
        await LoadToolsAsync();
    }

    [RelayCommand]
    private async Task ResetFiltersAsync()
    {
        Search = string.Empty;
        CategoryId = null;
        ManufacturerId = null;
        StatusId = null;
        SelectedStatsFilter = "all";
        CurrentPage = 1;

        await LoadToolsAsync();
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        SelectedStatsFilter = null;
        CurrentPage = 1;

        await LoadToolsAsync();
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

        await LoadToolsAsync();
    }

    [RelayCommand(CanExecute = nameof(CanGoPrevious))]
    private async Task PreviousPageAsync()
    {
        if (CurrentPage <= 1)
            return;

        CurrentPage--;

        await LoadToolsAsync();
    }

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private async Task NextPageAsync()
    {
        if (CurrentPage >= TotalPages)
            return;

        CurrentPage++;

        await LoadToolsAsync();
    }

    [RelayCommand]
    private async Task ExportAsync()
    {
        try
        {
            ErrorMessage = string.Empty;
            notificationService.Info("Началась выгрузка инструментов в Excel");

            var request = new ExportToolsRequest
            {
                Search = Search,
                CategoryId = CategoryId,
                ManufacturerId = ManufacturerId,
                StatusId = StatusId,
                SortBy = SortBy,
                Descending = Descending
            };

            var exportResult = await toolsApiClient.ExportToolsAsync(request);

            if (exportResult.IsFailure)
            {
                notificationService.Error(exportResult.Error.Message);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Title = "Сохранить список инструментов",
                FileName = "tools.xlsx",
                Filter = "Excel файл (*.xlsx)|*.xlsx"
            };

            if (dialog.ShowDialog() != true)
                return;

            await File.WriteAllBytesAsync(dialog.FileName, exportResult.Value);

            notificationService.Success("Инструменты выгружены в Excel");
        }
        catch (Exception ex)
        {
            ErrorMessage = string.Empty;
            notificationService.Error(ex.Message);
        }
    }

    [RelayCommand]
    private async Task GenerateTestToolsAsync()
    {
        if (Categories.Count == 0 ||
            Manufacturers.Count == 0 ||
            Statuses.Count == 0)
        {
            notificationService.Error("Не удалось загрузить справочники для генерации инструментов");
            return;
        }

        var availableStatus = FindStatus("Доступен");

        if (availableStatus is null)
        {
            notificationService.Error("Не найден статус «Доступен»");
            return;
        }

        const int count = 10;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            for (var i = 0; i < count; i++)
            {
                var tool = fakeToolGeneratorService.Generate();

                var categoryId = PickRandom(Categories).Id;
                var manufacturerId = PickRandom(Manufacturers).Id;

                var request = new CreateToolRequest(
                    ArticleNumber: tool.ArticleNumber,
                    Name: tool.Name,
                    Description: tool.Description,
                    CategoryId: categoryId,
                    ManufacturerId: manufacturerId,
                    RentalPricePerDay: tool.RentalPricePerDay,
                    DepositAmount: tool.DepositAmount,
                    SerialNumber: tool.SerialNumber,
                    InventoryNumber: tool.InventoryNumber,
                    CurrentCondition: tool.CurrentCondition);

                var result = await toolsApiClient.CreateToolAsync(request);

                if (result.IsFailure)
                {
                    notificationService.Error(result.Error.Message);
                    return;
                }
            }

            CurrentPage = 1;
            SelectedStatsFilter = "all";
            StatusId = null;

            await LoadStatsAsync();
            await LoadToolsAsync();

            notificationService.Success($"Успешно создано {count} тестовых инструментов");
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
    private async Task OpenCreateToolAsync()
    {
        await toolEditViewModel.OpenCreateAsync();

        navigationService.NavigateTo<ToolEditView>(
            "Добавление инструмента");
    }

    [RelayCommand]
    private async Task OpenViewToolAsync(ToolDto? tool)
    {
        if (tool is null)
            return;

        await toolEditViewModel.OpenViewAsync(tool);

        navigationService.NavigateTo<ToolEditView>("Просмотр инструмента");
    }
    
    [RelayCommand]
    private async Task OpenEditToolAsync(
        ToolDto? tool)
    {
        if (tool is null)
            return;

        await toolEditViewModel.OpenEditAsync(tool);

        navigationService.NavigateTo<ToolEditView>(
            "Редактирование инструмента");
    }

    [RelayCommand]
    private void OpenRentalHistory(ToolDto? tool)
    {
        if (tool is null)
            return;

        notificationService.Info("История аренды инструмента будет добавлена позже");
    }

    [RelayCommand]
    private async Task MoveToRepairAsync(ToolDto? tool)
    {
        if (tool is null)
            return;

        var repairStatus = FindStatus("На ремонте");

        if (repairStatus is null)
        {
            notificationService.Error("Не найден статус «На ремонте»");
            return;
        }

        var result = await toolsApiClient.ChangeToolStatusAsync(
            tool.Id,
            repairStatus.Id);

        if (result.IsFailure)
        {
            notificationService.Error(result.Error.Message);
            return;
        }

        notificationService.Success($"Инструмент «{tool.Name}» переведен в ремонт");

        await LoadStatsAsync();
        await LoadToolsAsync();
    }

    [RelayCommand]
    private async Task DeleteToolAsync(ToolDto? tool)
    {
        if (tool is null)
            return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var result = await toolsApiClient.DeleteToolAsync(tool.Id);

            if (result.IsFailure)
            {
                notificationService.Error(result.Error.Message);
                return;
            }

            notificationService.Success($"Инструмент «{tool.Name}» удален");

            await LoadStatsAsync();
            await LoadToolsAsync();
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

    private async Task LoadDictionariesAsync()
    {
        var categoriesResult = await dictionariesApiClient.GetListAsync<DictionaryItem>(
            "api/tool-categories",
            "tool.categories.load.failed",
            "Не удалось загрузить категории инструментов");

        if (categoriesResult.IsFailure)
        {
            Categories = [];
            notificationService.Error(categoriesResult.Error.Message);
        }
        else
        {
            Categories = categoriesResult.Value;
        }

        var manufacturersResult = await dictionariesApiClient.GetListAsync<DictionaryItem>(
            "api/manufacturers",
            "manufacturers.load.failed",
            "Не удалось загрузить производителей");

        if (manufacturersResult.IsFailure)
        {
            Manufacturers = [];
            notificationService.Error(manufacturersResult.Error.Message);
        }
        else
        {
            Manufacturers = manufacturersResult.Value;
        }

        var statusesResult = await dictionariesApiClient.GetListAsync<DictionaryItem>(
            "api/tool-statuses",
            "tool.statuses.load.failed",
            "Не удалось загрузить статусы инструментов");

        if (statusesResult.IsFailure)
        {
            Statuses = [];
            notificationService.Error(statusesResult.Error.Message);
        }
        else
        {
            Statuses = statusesResult.Value;
        }
    }

    private async Task LoadToolsAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var request = new GetToolsRequest(
                Search,
                CategoryId,
                ManufacturerId,
                StatusId,
                SortBy,
                Descending,
                CurrentPage,
                PageSize);

            var result = await toolsApiClient.GetToolsAsync(request);

            if (result.IsFailure)
            {
                Tools = [];
                TotalCount = 0;
                notificationService.Error(result.Error.Message);
                RefreshPaging();
                return;
            }

            Tools = result.Value.Items.ToList();
            TotalCount = result.Value.TotalCount;

            if (CurrentPage > TotalPages)
            {
                CurrentPage = TotalPages;
                await LoadToolsAsync();
                return;
            }

            RefreshPaging();
        }
        catch (Exception ex)
        {
            Tools = [];
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
            var result = await toolsApiClient.GetStatsAsync();

            if (result.IsFailure)
            {
                TotalTools = 0;
                AvailableCount = 0;
                RentedCount = 0;
                RepairCount = 0;

                notificationService.Error(result.Error.Message);
                return;
            }

            TotalTools = result.Value.TotalCount;
            AvailableCount = result.Value.AvailableCount;
            RentedCount = result.Value.RentedCount;
            RepairCount = result.Value.RepairCount;
        }
        catch (Exception ex)
        {
            ErrorMessage = string.Empty;
            notificationService.Error(ex.Message);
        }
    }

    private Guid? FindStatusId(string statusName)
    {
        return FindStatus(statusName)?.Id;
    }

    private DictionaryItem? FindStatus(string statusName)
    {
        return Statuses.FirstOrDefault(x =>
            string.Equals(
                x.Name?.Trim(),
                statusName,
                StringComparison.OrdinalIgnoreCase));
    }

    private DictionaryItem PickRandom(IReadOnlyList<DictionaryItem> items)
    {
        return items[_random.Next(items.Count)];
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
        OnPropertyChanged(nameof(IsAvailableStatsSelected));
        OnPropertyChanged(nameof(IsRentedStatsSelected));
        OnPropertyChanged(nameof(IsRepairStatsSelected));
    }

    partial void OnSearchChanged(string? value)
    {
        _ = SearchAsync();
    }

    partial void OnCategoryIdChanged(Guid? value)
    {
        if (_isSelectingStatsFilter)
            return;

        SelectedStatsFilter = null;
        _ = ApplyFiltersAsync();
    }

    partial void OnManufacturerIdChanged(Guid? value)
    {
        if (_isSelectingStatsFilter)
            return;

        SelectedStatsFilter = null;
        _ = ApplyFiltersAsync();
    }

    partial void OnStatusIdChanged(Guid? value)
    {
        if (_isSelectingStatsFilter)
            return;

        SelectedStatsFilter = null;
        _ = ApplyFiltersAsync();
    }

    partial void OnSelectedToolChanged(ToolDto? value)
    {
    }
}