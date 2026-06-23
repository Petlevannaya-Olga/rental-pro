using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.DependencyInjection;
using RentalPro.Contracts.Customers;
using RentalPro.Contracts.Orders;
using RentalPro.Contracts.Tools;
using RentalPro.Presentation.Desktop.Api;
using RentalPro.Presentation.Desktop.Auth;
using RentalPro.Presentation.Desktop.Common;
using RentalPro.Presentation.Desktop.Models;
using RentalPro.Presentation.Desktop.Services;
using RentalPro.Presentation.Desktop.Views;
using RentalPro.Shared;

namespace RentalPro.Presentation.Desktop.ViewModels;

public partial class OrderEditViewModel(
    OrdersApiClient ordersApiClient,
    NavigationService navigationService,
    DictionariesApiClient dictionariesApiClient,
    TokenStorage tokenStorage,
    IServiceProvider serviceProvider,
    FakeOrderGeneratorService fakeOrderGeneratorService,
    ToolsApiClient toolsApiClient,
    CustomersApiClient customersApiClient,
    NotificationService notificationService)
    : ObservableObject
{
    private Guid? _currentUserId;
    private Guid? _confirmedStatusId;
    private Guid? _availableToolStatusId;
    private IReadOnlyList<OrderDocumentDto> _orderDocuments = [];
    
    public bool CanReturnTools =>
        Details is not null
        && Details.StatusName == "Выполняется";
    
    public bool CanIssueTools =>
        Details is not null
        && Details.StatusName == "Готов к выдаче";
    
    public IReadOnlyList<OrderDocumentDto> OrderDocuments =>
        Details?.StatusName == "Отменен"
            ? []
            : _orderDocuments;
    
    [ObservableProperty]
    private OrderEditModel order = new();

    [ObservableProperty]
    private FormMode mode = FormMode.Create;

    [ObservableProperty]
    private Guid? orderId;

    [ObservableProperty]
    private OrderDetailsDto? details;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool isSaving;

    [ObservableProperty]
    private bool isLoading;

    public bool IsCreateMode => Mode == FormMode.Create;

    public bool IsViewMode => Mode == FormMode.View;

    public bool IsEditable => Mode == FormMode.Create;

    public string Title =>
        Mode == FormMode.Create
            ? "Создание заказа"
            : $"Заказ {Details?.Number}";
    
    public string ManagerFullName =>
        tokenStorage.ManagerFullName;

    public decimal RentalAmount =>
        Order.Tools.Sum(x => x.TotalAmount);

    public decimal DepositAmount =>
        Order.Tools.Sum(x => x.DepositAmount);

    public decimal TotalToPay =>
        RentalAmount + DepositAmount;

    public int ToolsCount =>
        Order.Tools.Count;
    
    public string RentalDaysText =>
        Order.Tools.Count == 0
            ? "0"
            : $"{Order.Tools.Max(x => x.RentalDays)}";
    
    public decimal ViewRentalAmount =>
        Details?.TotalCost ?? 0;

    public decimal ViewDepositAmount =>
        Details?.DepositTotal ?? 0;

    public decimal ViewTotalToPay =>
        ViewRentalAmount + ViewDepositAmount;

    public int ViewToolsCount =>
        Details?.Items.Count ?? 0;
    
    public bool CanAcceptPayment =>
        Details is not null
        && Details.TotalRemainingAmount > 0
        && Details.StatusName == "Подтвержден";
    
    public List<OrderItemViewModel> ViewItems =>
        Details?.Items
            .Select(x => new OrderItemViewModel
            {
                ToolName = x.ToolName,
                StartDate = x.StartDate,
                PlannedReturnDate = x.PlannedReturnDate,
                ActualReturnedDate = x.ActualReturnedDate,
                RentalPricePerDay = x.RentalPricePerDay,
                TotalAmount = x.TotalAmount,
                DepositAmount = x.DepositAmount
            })
            .ToList()
        ?? [];

    public string ViewPeriodText
    {
        get
        {
            if (Details is null || Details.Items.Count == 0)
                return "—";

            var start = Details.Items.Min(x => x.StartDate);
            var end = Details.Items.Max(x => x.PlannedReturnDate);

            return $"{start:dd.MM} — {end:dd.MM}";
        }
    }

    private bool CanSave()
    {
        return IsCreateMode
               && !IsSaving
               && Order.CustomerId is not null
               && Order.Tools.Count > 0;
    }

    public async Task OpenCreateAsync()
    {
        Mode = FormMode.Create;
        OrderId = null;
        Details = null;
        ErrorMessage = string.Empty;
        IsSaving = false;

        await LoadCreateDataAsync();

        SetOrder(new OrderEditModel
        {
            OrderDate = DateTime.Today,
            Tools = new ObservableCollection<OrderToolEditModel>()
        });

        RefreshState();
    }
    
    private async Task LoadCreateDataAsync()
    {
        _currentUserId = tokenStorage.UserId;

        if (_currentUserId is null)
        {
            notificationService.Error("Не удалось определить текущего пользователя");
            return;
        }

        var statusesResult = await dictionariesApiClient.GetListAsync<DictionaryItem>(
            "api/order-statuses",
            "order.statuses.load.failed",
            "Не удалось загрузить статусы заказов");

        if (statusesResult.IsFailure)
        {
            notificationService.Error(statusesResult.Error.Message);
            return;
        }

        _confirmedStatusId = statusesResult.Value
            .FirstOrDefault(x => x.Name == "Подтвержден")
            ?.Id;

        if (_confirmedStatusId is null)
            notificationService.Error("Статус заказа «Подтвержден» не найден");
    }
    
   public async Task OpenViewAsync(Guid id)
    {
        Mode = FormMode.View;
        OrderId = id;
        ErrorMessage = string.Empty;
        IsLoading = true;

        try
        {
            var result = await ordersApiClient.GetByIdAsync(id);

            if (result.IsFailure)
            {
                ErrorMessage = result.Error.Message;
                notificationService.Error(result.Error.Message);
                return;
            }

            Details = result.Value;
            await LoadOrderDocumentsAsync(id);
            RefreshState();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Back()
    {
        navigationService.NavigateTo<OrdersView>("Заказы");
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (Order.CustomerId is null)
        {
            ErrorMessage = "Выберите клиента";
            return;
        }

        if (Order.Tools.Count == 0)
        {
            ErrorMessage = "Выберите инструменты";
            return;
        }

        if (_currentUserId is null)
        {
            ErrorMessage = "Не удалось определить текущего пользователя";
            return;
        }

        if (_confirmedStatusId is null)
        {
            ErrorMessage = "Статус заказа «Подтвержден» не найден";
            return;
        }
        
        try
        {
            IsSaving = true;
            ErrorMessage = string.Empty;

            var request = new CreateOrderRequest(
                UserId: _currentUserId.Value,
                CustomerId: Order.CustomerId.Value,
                StatusId: _confirmedStatusId.Value,
                OrderDate: Order.OrderDate,
                DepositTotal: DepositAmount,
                Comment: Order.Comment,
                Items: Order.Tools
                    .Select(x => new CreateOrderItemDto(
                        ToolId: x.ToolId,
                        RentalPricePerDay: x.RentalPricePerDay,
                        DepositAmount: x.DepositAmount,
                        StartDate: DateOnly.FromDateTime(Order.OrderDate),
                        PlannedReturnDate: DateOnly.FromDateTime(x.EndDate)))
                    .ToList());

            var result = await ordersApiClient.CreateOrderAsync(request);

            if (result.IsFailure)
            {
                ErrorMessage = result.Error.Message;
                notificationService.Error(result.Error.Message);
                return;
            }

            notificationService.Success("Заказ создан");

            await OpenViewAsync(result.Value.Id);

            navigationService.NavigateTo<OrderEditView>(
                $"Заказ {result.Value.Id}");
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private void SelectCustomer()
    {
        var dialog = serviceProvider.GetRequiredService<SelectCustomerDialog>();

        var result = dialog.ShowDialog();

        if (result != true)
            return;

        if (dialog.ViewModel.SelectedCustomerId is not null)
        {
            Order.CustomerId = dialog.ViewModel.SelectedCustomerId;
            Order.CustomerName = dialog.ViewModel.SelectedCustomerName ?? string.Empty;
        }
        else if (dialog.ViewModel.Result is not null)
        {
            Order.CustomerId = dialog.ViewModel.Result.Id;
            Order.CustomerName = dialog.ViewModel.Result.FullName;
        }

        RefreshState();
        SaveCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private async Task SelectToolsAsync()
    {
        var dialog = serviceProvider.GetRequiredService<SelectToolsDialog>();

        await dialog.LoadAsync(Order.Tools);

        var result = dialog.ShowDialog();

        if (result != true)
            return;

        var selectedTools = dialog.ViewModel.Result;

        var previousTools = Order.Tools
            .ToDictionary(x => x.ToolId);

        UnsubscribeTools();
        
        Order.Tools = new ObservableCollection<OrderToolEditModel>(
            selectedTools.Select(x =>
            {
                if (previousTools.TryGetValue(x.ToolId, out var existing))
                    return existing;

                return new OrderToolEditModel
                {
                    ToolId = x.ToolId,
                    ToolName = x.ToolName,
                    RentalPricePerDay = x.RentalPricePerDay,
                    DepositAmount = x.DepositAmount,
                    StartDate = Order.OrderDate,
                    EndDate = Order.OrderDate.AddDays(3)
                };
            }));

        SubscribeTools();

        RefreshState();
        SaveCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void RemoveTool(OrderToolEditModel? tool)
    {
        if (tool is null)
            return;

        tool.PropertyChanged -= ToolOnPropertyChanged;

        Order.Tools.Remove(tool);

        RefreshTotals();
        SaveCommand.NotifyCanExecuteChanged();
    }
    
    [RelayCommand]
    private async Task FillTestDataAsync()
    {
        var customersResult = await customersApiClient.GetCustomersAsync(
            new GetCustomersRequest(
                Search: null,
                HasOrders: null,
                HasActiveOrders: null,
                IsRegular: null,
                SortBy: "fullname",
                Descending: false,
                Page: 1,
                PageSize: 100));

        if (customersResult.IsFailure)
        {
            notificationService.Error(customersResult.Error.Message);
            return;
        }

        if (_availableToolStatusId is null)
            await LoadAvailableToolStatusAsync();

        if (_availableToolStatusId is null)
        {
            notificationService.Error("Статус инструмента «Доступен» не найден");
            return;
        }

        var toolsResult = await toolsApiClient.GetToolsAsync(
            new GetToolsRequest(
                Search: null,
                CategoryId: null,
                ManufacturerId: null,
                StatusId: _availableToolStatusId,
                SortBy: "name",
                Descending: false,
                Page: 1,
                PageSize: 100));

        if (toolsResult.IsFailure)
        {
            notificationService.Error(toolsResult.Error.Message);
            return;
        }

        UnsubscribeTools();

        fakeOrderGeneratorService.Fill(
            Order,
            customersResult.Value.Items.ToList(),
            toolsResult.Value.Items.ToList());

        SubscribeTools();

        RefreshState();
        SaveCommand.NotifyCanExecuteChanged();

        notificationService.Success("Тестовые данные заполнены");
    }

    [RelayCommand]
    private async Task DownloadDocumentAsync(OrderDocumentDto? document)
    {
        if (document is null)
            return;

        if (Details is null)
        {
            notificationService.Error("Заказ не выбран");
            return;
        }

        Result<byte[], Errors> result;

        if (document.Type == OrderDocumentType.Contract)
        {
            result = await ordersApiClient.ExportContractAsync(Details.Id);
        }
        else if (document.Type == OrderDocumentType.TransferAct && document.Date is not null)
        {
            result = await ordersApiClient.ExportTransferActAsync(
                Details.Id,
                document.Date.Value);
        }
        else if (document.Type == OrderDocumentType.ReturnAct && document.Date is not null)
        {
            result = await ordersApiClient.ExportReturnActAsync(
                Details.Id,
                document.Date.Value);
        }
        else
        {
            notificationService.Error("Не удалось определить тип документа");
            return;
        }

        if (result.IsFailure)
        {
            notificationService.Error(result.Error.Message);
            return;
        }

        var fileName = GetDocumentFileName(document);

        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            FileName = fileName,
            Filter = "Документ Word (*.docx)|*.docx|Все файлы (*.*)|*.*"
        };

        if (dialog.ShowDialog() != true)
            return;

        await File.WriteAllBytesAsync(dialog.FileName, result.Value);

        notificationService.Success("Документ успешно сохранен");
    }
    
    [RelayCommand]
    private async Task OpenPaymentAsync()
    {
        if (Details is null)
        {
            notificationService.Error("Заказ не выбран");
            return;
        }

        var dialog = serviceProvider.GetRequiredService<PaymentDialog>();

        await dialog.OpenAsync(Details);

        var result = dialog.ShowDialog();

        if (result != true)
            return;

        await OpenViewAsync(Details.Id);

        notificationService.Success("Данные заказа обновлены");
    }

    [RelayCommand]
    private async Task IssueToolsAsync()
    {
        if (Details is null)
        {
            notificationService.Error("Заказ не выбран");
            return;
        }

        var result = await ordersApiClient.IssueAsync(Details.Id);

        if (result.IsFailure)
        {
            notificationService.Error(result.Error.Message);
            return;
        }

        notificationService.Success("Инструменты успешно выданы");

        await OpenViewAsync(Details.Id);
    }
    
    [RelayCommand]
    private async Task OpenReturnAsync()
    {
        if (Details is null)
        {
            notificationService.Error("Заказ не выбран");
            return;
        }

        var dialog = serviceProvider.GetRequiredService<ReturnDialog>();

        dialog.Open(Details);

        var result = dialog.ShowDialog();

        if (result != true)
            return;

        await OpenViewAsync(Details.Id);
    }
    
    private static string GetDocumentFileName(OrderDocumentDto document)
    {
        var title = document.Title
            .Replace(" ", "_")
            .Replace("№", "N");

        return $"{title}.docx";
    }
    
    private void SetOrder(OrderEditModel newOrder)
    {
        Order.PropertyChanged -= OrderOnPropertyChanged;

        foreach (var tool in Order.Tools)
            tool.PropertyChanged -= ToolOnPropertyChanged;

        Order = newOrder;

        Order.PropertyChanged += OrderOnPropertyChanged;

        foreach (var tool in Order.Tools)
            tool.PropertyChanged += ToolOnPropertyChanged;

        RefreshTotals();
        SaveCommand.NotifyCanExecuteChanged();
    }
    
    private void ToolOnPropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(OrderToolEditModel.RentalDays)
            or nameof(OrderToolEditModel.TotalAmount)
            or nameof(OrderToolEditModel.EndDate)
            or nameof(OrderToolEditModel.StartDate))
        {
            RefreshTotals();
        }
    }

    private void OrderOnPropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(OrderEditModel.OrderDate))
        {
            foreach (var tool in Order.Tools)
            {
                tool.StartDate = Order.OrderDate;

                if (tool.EndDate <= tool.StartDate)
                    tool.EndDate = tool.StartDate.AddDays(1);
            }
        }

        RefreshTotals();
        SaveCommand.NotifyCanExecuteChanged();
    }

    private void RefreshState()
    {
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(ManagerFullName));
        OnPropertyChanged(nameof(IsCreateMode));
        OnPropertyChanged(nameof(IsViewMode));
        OnPropertyChanged(nameof(IsEditable));
        
        OnPropertyChanged(nameof(ViewRentalAmount));
        OnPropertyChanged(nameof(ViewDepositAmount));
        OnPropertyChanged(nameof(ViewTotalToPay));
        OnPropertyChanged(nameof(ViewToolsCount));
        OnPropertyChanged(nameof(ViewPeriodText));
        OnPropertyChanged(nameof(ViewItems));
        OnPropertyChanged(nameof(OrderDocuments));
        
        OnPropertyChanged(nameof(CanAcceptPayment));
        OnPropertyChanged(nameof(CanIssueTools));
        
        OnPropertyChanged(nameof(CanReturnTools));

        RefreshTotals();
        SaveCommand.NotifyCanExecuteChanged();
    }

    private void RefreshTotals()
    {
        OnPropertyChanged(nameof(RentalAmount));
        OnPropertyChanged(nameof(DepositAmount));
        OnPropertyChanged(nameof(TotalToPay));
        OnPropertyChanged(nameof(ToolsCount));
        OnPropertyChanged(nameof(RentalDaysText));
    }

    partial void OnModeChanged(FormMode value)
    {
        RefreshState();
    }

    partial void OnIsSavingChanged(bool value)
    {
        SaveCommand.NotifyCanExecuteChanged();
    }
    
    private async Task LoadAvailableToolStatusAsync()
    {
        var result = await dictionariesApiClient.GetListAsync<DictionaryItem>(
            "api/tool-statuses",
            "tool.statuses.load.failed",
            "Не удалось загрузить статусы инструментов");

        if (result.IsFailure)
        {
            notificationService.Error(result.Error.Message);
            return;
        }

        _availableToolStatusId = result.Value
            .FirstOrDefault(x => x.Name == "Доступен")
            ?.Id;
    }
    
    private void SubscribeTools()
    {
        foreach (var tool in Order.Tools)
            tool.PropertyChanged += ToolOnPropertyChanged;
    }

    private void UnsubscribeTools()
    {
        foreach (var tool in Order.Tools)
            tool.PropertyChanged -= ToolOnPropertyChanged;
    }
    
    private async Task LoadOrderDocumentsAsync(Guid orderId)
    {
        var result = await ordersApiClient.GetDocumentsAsync(orderId);

        if (result.IsFailure)
        {
            _orderDocuments = [];
            notificationService.Error(result.Error.Message);
            OnPropertyChanged(nameof(OrderDocuments));
            return;
        }

        _orderDocuments = result.Value;
        OnPropertyChanged(nameof(OrderDocuments));
    }
}