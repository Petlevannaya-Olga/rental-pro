using System.ComponentModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSharpFunctionalExtensions;
using Microsoft.Win32;
using RentalPro.Contracts.Tools;
using RentalPro.Presentation.Desktop.Api;
using RentalPro.Presentation.Desktop.Common;
using RentalPro.Presentation.Desktop.Models;
using RentalPro.Presentation.Desktop.Services;
using RentalPro.Presentation.Desktop.Views;
using RentalPro.Shared;

namespace RentalPro.Presentation.Desktop.ViewModels;

public partial class ToolEditViewModel(
    ToolsApiClient toolsApiClient,
    DictionariesApiClient dictionariesApiClient,
    NavigationService navigationService,
    FakeToolGeneratorService fakeToolGeneratorService,
    NotificationService notificationService)
    : ObservableObject
{
    [ObservableProperty]
    private ToolEditModel tool = new();

    [ObservableProperty]
    private FormMode mode = FormMode.Create;

    [ObservableProperty]
    private Guid? toolId;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool isSaving;

    [ObservableProperty]
    private List<DictionaryItem> categories = [];

    [ObservableProperty]
    private List<DictionaryItem> manufacturers = [];

    [ObservableProperty]
    private List<DictionaryItem> statuses = [];

    private byte[]? _selectedImageBytes;
    private string? _selectedImageFileName;
    private string? _selectedImageContentType;

    public bool IsCreateMode => Mode == FormMode.Create;

    public bool IsEditMode => Mode == FormMode.Edit;

    public bool IsViewMode => Mode == FormMode.View;

    public bool IsEditable => Mode is FormMode.Create or FormMode.Edit;

    public bool CanShowTestDataButton => Mode == FormMode.Create;

    public string CategoryName =>
        Categories.FirstOrDefault(x => x.Id == Tool.CategoryId)?.Name ?? "Не указано";

    public string ManufacturerName =>
        Manufacturers.FirstOrDefault(x => x.Id == Tool.ManufacturerId)?.Name ?? "Не указано";

    public string StatusName =>
        Statuses.FirstOrDefault(x => x.Id == Tool.StatusId)?.Name ?? "Не указано";
    
    public string Title =>
        Mode switch
        {
            FormMode.Create => "Добавление инструмента",
            FormMode.Edit => "Редактирование инструмента",
            FormMode.View => "Просмотр инструмента",
            _ => "Инструмент"
        };

    public string SaveButtonText =>
        Mode == FormMode.Edit
            ? "Сохранить"
            : "Сохранить инструмент";

    private bool IsFormFilled =>
        !string.IsNullOrWhiteSpace(Tool.Name)
        && !string.IsNullOrWhiteSpace(Tool.ArticleNumber)
        && !string.IsNullOrWhiteSpace(Tool.SerialNumber)
        && !string.IsNullOrWhiteSpace(Tool.InventoryNumber)
        && Tool.CategoryId is not null
        && Tool.ManufacturerId is not null
        && Tool.RentalPricePerDay > 0
        && Tool.DepositAmount >= 0;

    private bool CanSave()
    {
        return IsEditable
               && !IsSaving
               && IsFormFilled;
    }

    public async Task OpenCreateAsync()
    {
        ToolId = null;
        ErrorMessage = string.Empty;
        IsSaving = false;

        await LoadDictionariesAsync();

        SetTool(new ToolEditModel
        {
            StatusId = FindStatusId("Доступен")
        });

        ClearSelectedImage();

        Mode = FormMode.Create;

        RefreshState();
    }

    public async Task OpenEditAsync(ToolDto dto)
    {
        ToolId = dto.Id;
        ErrorMessage = string.Empty;
        IsSaving = false;

        await LoadDictionariesAsync();

        SetToolFromDto(dto);
        ClearSelectedImage();

        Mode = FormMode.Edit;

        RefreshState();
    }

    public async Task OpenViewAsync(ToolDto dto)
    {
        ToolId = dto.Id;
        ErrorMessage = string.Empty;
        IsSaving = false;

        await LoadDictionariesAsync();

        SetToolFromDto(dto);
        ClearSelectedImage();

        Mode = FormMode.View;

        RefreshState();
    }

    [RelayCommand]
    private void Back()
    {
        navigationService.NavigateTo<ToolsView>("Инструменты");
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        try
        {
            IsSaving = true;
            ErrorMessage = string.Empty;

            UnitResult<Errors> result;

            if (Mode == FormMode.Create)
                result = await CreateAsync();
            else if (Mode == FormMode.Edit)
                result = await UpdateAsync();
            else
                return;

            if (result.IsFailure)
            {
                ErrorMessage = result.Error.Message;
                notificationService.Error(result.Error.Message);
                return;
            }

            notificationService.Success(
                Mode == FormMode.Edit
                    ? "Данные инструмента обновлены"
                    : "Инструмент добавлен");

            navigationService.NavigateTo<ToolsView>("Инструменты");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            notificationService.Error("Не удалось сохранить инструмент");
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanFillTestData))]
    private void FillTestData()
    {
        var generated = fakeToolGeneratorService.Generate();

        generated.CategoryId = Categories.FirstOrDefault()?.Id;
        generated.ManufacturerId = Manufacturers.FirstOrDefault()?.Id;
        generated.StatusId = FindStatusId("Доступен");

        SetTool(generated);
        ErrorMessage = string.Empty;

        notificationService.Success("Сгенерированы тестовые данные");
    }

    private bool CanFillTestData()
    {
        return Mode == FormMode.Create && !IsSaving;
    }

    [RelayCommand]
    private void SelectImage()
    {
        if (!IsEditable)
            return;

        var dialog = new OpenFileDialog
        {
            Title = "Выберите фото инструмента",
            Filter = "Изображения (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
        };

        if (dialog.ShowDialog() != true)
            return;

        _selectedImageBytes = File.ReadAllBytes(dialog.FileName);
        _selectedImageFileName = Path.GetFileName(dialog.FileName);
        _selectedImageContentType = GetContentType(dialog.FileName);

        Tool.PhotoPath = dialog.FileName;

        RefreshSaveState();
    }

    private async Task<UnitResult<Errors>> CreateAsync()
    {
        var request = new CreateToolRequest(
            ArticleNumber: Tool.ArticleNumber.Trim(),
            Name: Tool.Name.Trim(),
            Description: NormalizeOptional(Tool.Description),
            CategoryId: Tool.CategoryId!.Value,
            ManufacturerId: Tool.ManufacturerId!.Value,
            RentalPricePerDay: Tool.RentalPricePerDay,
            DepositAmount: Tool.DepositAmount,
            SerialNumber: Tool.SerialNumber.Trim(),
            InventoryNumber: Tool.InventoryNumber.Trim(),
            CurrentCondition: NormalizeOptional(Tool.CurrentCondition));

        var createResult = await toolsApiClient.CreateToolAsync(request);

        if (createResult.IsFailure)
            return createResult.Error;

        if (_selectedImageBytes is null ||
            string.IsNullOrWhiteSpace(_selectedImageFileName) ||
            string.IsNullOrWhiteSpace(_selectedImageContentType))
        {
            return UnitResult.Success<Errors>();
        }

        return await toolsApiClient.UploadToolImageAsync(
            createResult.Value,
            _selectedImageBytes,
            _selectedImageFileName,
            _selectedImageContentType);
    }

    private async Task<UnitResult<Errors>> UpdateAsync()
    {
        if (ToolId is null)
        {
            return CommonErrors
                .Failure(
                    "tool.update.no-selected-tool",
                    "Не выбран инструмент для редактирования")
                .ToErrors();
        }

        var request = new UpdateToolRequest(
            ArticleNumber: Tool.ArticleNumber.Trim(),
            Name: Tool.Name.Trim(),
            Description: NormalizeOptional(Tool.Description),
            CategoryId: Tool.CategoryId!.Value,
            ManufacturerId: Tool.ManufacturerId!.Value,
            RentalPricePerDay: Tool.RentalPricePerDay,
            DepositAmount: Tool.DepositAmount,
            SerialNumber: Tool.SerialNumber.Trim(),
            InventoryNumber: Tool.InventoryNumber.Trim(),
            CurrentCondition: NormalizeOptional(Tool.CurrentCondition));

        var updateResult = await toolsApiClient.UpdateToolAsync(
            ToolId.Value,
            request);

        if (updateResult.IsFailure)
            return updateResult;

        if (_selectedImageBytes is null ||
            string.IsNullOrWhiteSpace(_selectedImageFileName) ||
            string.IsNullOrWhiteSpace(_selectedImageContentType))
        {
            return UnitResult.Success<Errors>();
        }

        return await toolsApiClient.UploadToolImageAsync(
            ToolId.Value,
            _selectedImageBytes,
            _selectedImageFileName,
            _selectedImageContentType);
    }

    private async Task LoadDictionariesAsync()
    {
        var categoriesResult = await dictionariesApiClient.GetListAsync<DictionaryItem>(
            "api/tool-categories",
            "tool.categories.load.failed",
            "Не удалось загрузить категории инструментов");

        Categories = categoriesResult.IsSuccess
            ? categoriesResult.Value
            : [];

        var manufacturersResult = await dictionariesApiClient.GetListAsync<DictionaryItem>(
            "api/manufacturers",
            "manufacturers.load.failed",
            "Не удалось загрузить производителей");

        Manufacturers = manufacturersResult.IsSuccess
            ? manufacturersResult.Value
            : [];

        var statusesResult = await dictionariesApiClient.GetListAsync<DictionaryItem>(
            "api/tool-statuses",
            "tool.statuses.load.failed",
            "Не удалось загрузить статусы инструментов");

        Statuses = statusesResult.IsSuccess
            ? statusesResult.Value
            : [];
    }

    private void SetToolFromDto(ToolDto dto)
    {
        SetTool(new ToolEditModel
        {
            ArticleNumber = dto.ArticleNumber,
            Name = dto.Name,
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            ManufacturerId = dto.ManufacturerId,
            StatusId = dto.StatusId,
            RentalPricePerDay = dto.RentalPricePerDay,
            DepositAmount = dto.DepositAmount,
            SerialNumber = dto.SerialNumber,
            InventoryNumber = dto.InventoryNumber,
            CurrentCondition = dto.CurrentCondition,
            PhotoPath = dto.PhotoPath
        });
    }

    private void SetTool(ToolEditModel newTool)
    {
        Tool.PropertyChanged -= ToolOnPropertyChanged;

        Tool = newTool;

        Tool.PropertyChanged += ToolOnPropertyChanged;

        RefreshToolDerivedProperties();
        RefreshSaveState();
    }
    
    private void RefreshToolDerivedProperties()
    {
        OnPropertyChanged(nameof(CategoryName));
        OnPropertyChanged(nameof(ManufacturerName));
        OnPropertyChanged(nameof(StatusName));
    }
    
    private void ToolOnPropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        RefreshToolDerivedProperties();
        RefreshSaveState();
    }

    private void RefreshState()
    {
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(SaveButtonText));
        OnPropertyChanged(nameof(IsCreateMode));
        OnPropertyChanged(nameof(IsEditMode));
        OnPropertyChanged(nameof(IsViewMode));
        OnPropertyChanged(nameof(IsEditable));
        OnPropertyChanged(nameof(CanShowTestDataButton));
        
        OnPropertyChanged(nameof(CategoryName));
        OnPropertyChanged(nameof(ManufacturerName));
        OnPropertyChanged(nameof(StatusName));

        RefreshSaveState();
        FillTestDataCommand.NotifyCanExecuteChanged();
    }

    private void RefreshSaveState()
    {
        SaveCommand.NotifyCanExecuteChanged();
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

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        return extension switch
        {
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            _ => "application/octet-stream"
        };
    }

    private void ClearSelectedImage()
    {
        _selectedImageBytes = null;
        _selectedImageFileName = null;
        _selectedImageContentType = null;
    }

    partial void OnModeChanged(FormMode value)
    {
        RefreshState();
    }

    partial void OnIsSavingChanged(bool value)
    {
        RefreshSaveState();
        FillTestDataCommand.NotifyCanExecuteChanged();
    }
}