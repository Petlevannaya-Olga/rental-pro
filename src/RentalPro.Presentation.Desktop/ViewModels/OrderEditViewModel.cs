using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using RentalPro.Contracts.Orders;
using RentalPro.Presentation.Desktop.Api;
using RentalPro.Presentation.Desktop.Auth;
using RentalPro.Presentation.Desktop.Common;
using RentalPro.Presentation.Desktop.Models;
using RentalPro.Presentation.Desktop.Services;
using RentalPro.Presentation.Desktop.Views;

namespace RentalPro.Presentation.Desktop.ViewModels;

public partial class OrderEditViewModel(
    OrdersApiClient ordersApiClient,
    NavigationService navigationService,
    DictionariesApiClient dictionariesApiClient,
    TokenStorage tokenStorage,
    IServiceProvider serviceProvider,
    NotificationService notificationService)
    : ObservableObject
{
    private Guid? _currentUserId;
    private Guid? _confirmedStatusId;
    
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

        foreach (var tool in Order.Tools)
            tool.PropertyChanged += ToolOnPropertyChanged;
        
        RefreshTotals();
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
}