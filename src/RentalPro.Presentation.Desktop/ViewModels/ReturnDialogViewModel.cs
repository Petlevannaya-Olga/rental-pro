using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalPro.Contracts.Orders;
using RentalPro.Presentation.Desktop.Api;
using RentalPro.Presentation.Desktop.Models;
using RentalPro.Presentation.Desktop.Services;

namespace RentalPro.Presentation.Desktop.ViewModels;

public partial class ReturnDialogViewModel(
    OrdersApiClient ordersApiClient,
    NotificationService notificationService)
    : ObservableObject
{
    private OrderDetailsDto? _order;

    [ObservableProperty]
    private List<ReturnOrderItemModel> items = [];

    [ObservableProperty]
    private DateTime actualReturnedDate = DateTime.Today;

    [ObservableProperty]
    private string returnCondition = "Исправно";

    [ObservableProperty]
    private string? damageComment;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool isSaving;

    public bool IsSaved { get; private set; }

    public string OrderNumber =>
        _order?.Number ?? string.Empty;

    public string CustomerName =>
        _order?.CustomerFullName ?? string.Empty;

    public IReadOnlyList<string> Conditions { get; } =
    [
        "Исправно",
        "Требует обслуживания"
    ];

    private bool CanSave()
    {
        return !IsSaving
               && _order is not null
               && Items.Any(x => x.IsSelected);
    }

    public void Open(OrderDetailsDto order)
    {
        _order = order;
        IsSaved = false;
        ErrorMessage = string.Empty;
        ActualReturnedDate = DateTime.Today;
        ReturnCondition = "Исправно";
        DamageComment = null;

        Items = order.Items
            .Where(x => x.ActualReturnedDate is null)
            .Select(x => new ReturnOrderItemModel
            {
                Id = x.Id,
                ToolName = x.ToolName,
                PlannedReturnDate = x.PlannedReturnDate
            })
            .ToList();

        foreach (var item in Items)
            item.PropertyChanged += (_, _) => SaveCommand.NotifyCanExecuteChanged();

        OnPropertyChanged(nameof(OrderNumber));
        OnPropertyChanged(nameof(CustomerName));
        SaveCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (_order is null)
        {
            ErrorMessage = "Заказ не выбран";
            return;
        }

        var selectedIds = Items
            .Where(x => x.IsSelected)
            .Select(x => x.Id)
            .ToList();

        if (selectedIds.Count == 0)
        {
            ErrorMessage = "Выберите инструменты для возврата";
            return;
        }

        try
        {
            IsSaving = true;
            ErrorMessage = string.Empty;

            var request = new ReturnOrderItemsRequest(
                ActualReturnedDate: DateOnly.FromDateTime(ActualReturnedDate),
                ReturnCondition: ReturnCondition,
                DamageComment: DamageComment,
                OrderItemIds: selectedIds);

            var result = await ordersApiClient.ReturnItemsAsync(
                _order.Id,
                request);

            if (result.IsFailure)
            {
                ErrorMessage = result.Error.Message;
                notificationService.Error(result.Error.Message);
                return;
            }

            IsSaved = true;
            notificationService.Success("Возврат успешно оформлен");
            OnPropertyChanged(nameof(IsSaved));
        }
        finally
        {
            IsSaving = false;
        }
    }

    partial void OnIsSavingChanged(bool value)
    {
        SaveCommand.NotifyCanExecuteChanged();
    }
}