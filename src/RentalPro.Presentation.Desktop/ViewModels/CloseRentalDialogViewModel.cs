using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalPro.Contracts.Orders;
using RentalPro.Presentation.Desktop.Api;
using RentalPro.Presentation.Desktop.Models;
using RentalPro.Presentation.Desktop.Services;

namespace RentalPro.Presentation.Desktop.ViewModels;

public partial class CloseRentalDialogViewModel(
    OrdersApiClient ordersApiClient,
    DictionariesApiClient dictionariesApiClient,
    NotificationService notificationService)
    : ObservableObject
{
    private OrderDetailsDto? _order;

    [ObservableProperty]
    private List<DictionaryItem> paymentMethods = [];

    [ObservableProperty]
    private Guid? paymentMethodId;

    [ObservableProperty]
    private DateTime paymentDate = DateTime.Now;

    [ObservableProperty]
    private string? comment;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool isSaving;

    public bool IsSaved { get; private set; }

    public string OrderNumber => _order?.Number ?? string.Empty;

    public decimal PlannedRentalAmount => _order?.PlannedRentalAmount ?? 0;

    public decimal ActualRentalAmount => _order?.ActualRentalAmount ?? 0;

    public decimal PaidRentalAmount => _order?.PaidRentalAmount ?? 0;

    public decimal RentalRefundAmount => _order?.RentalRefundAmount ?? 0;

    public decimal RentalAdditionalPaymentAmount => _order?.RentalAdditionalPaymentAmount ?? 0;

    public decimal DepositRefundAmount => _order?.RemainingDepositRefundAmount ?? 0;

    public decimal ClientRefundTotal =>
        RentalRefundAmount + DepositRefundAmount;

    public decimal ClientPaymentTotal =>
        RentalAdditionalPaymentAmount;

    public string FinalTotalTitle =>
        ClientPaymentTotal > 0
            ? "Итого к доплате клиентом"
            : "Итого к возврату клиенту";

    public decimal FinalTotalAmount =>
        ClientPaymentTotal > 0
            ? ClientPaymentTotal
            : ClientRefundTotal;

    private bool CanSave()
    {
        return !IsSaving
               && _order is not null
               && PaymentMethodId is not null;
    }

    public async Task OpenAsync(OrderDetailsDto order)
    {
        _order = order;
        IsSaved = false;
        ErrorMessage = string.Empty;
        PaymentMethodId = null;
        PaymentDate = GetLastReturnedDate(order);
        Comment = null;

        await LoadPaymentMethodsAsync();

        OnPropertyChanged(nameof(OrderNumber));
        OnPropertyChanged(nameof(PlannedRentalAmount));
        OnPropertyChanged(nameof(ActualRentalAmount));
        OnPropertyChanged(nameof(PaidRentalAmount));
        OnPropertyChanged(nameof(RentalRefundAmount));
        OnPropertyChanged(nameof(RentalAdditionalPaymentAmount));
        OnPropertyChanged(nameof(DepositRefundAmount));
        OnPropertyChanged(nameof(ClientRefundTotal));
        OnPropertyChanged(nameof(ClientPaymentTotal));
        OnPropertyChanged(nameof(FinalTotalTitle));
        OnPropertyChanged(nameof(FinalTotalAmount));

        SaveCommand.NotifyCanExecuteChanged();
    }

    private async Task LoadPaymentMethodsAsync()
    {
        var result = await dictionariesApiClient.GetListAsync<DictionaryItem>(
            "api/payment-methods",
            "payment.methods.load.failed",
            "Не удалось загрузить способы оплаты");

        if (result.IsFailure)
        {
            notificationService.Error(result.Error.Message);
            PaymentMethods = [];
            return;
        }

        PaymentMethods = result.Value;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (_order is null)
        {
            ErrorMessage = "Заказ не выбран";
            return;
        }

        if (PaymentMethodId is null)
        {
            ErrorMessage = "Выберите способ оплаты или возврата";
            return;
        }

        try
        {
            IsSaving = true;
            ErrorMessage = string.Empty;

            var request = new CloseRentalRequest(
                PaymentMethodId.Value,
                PaymentDate,
                Comment);

            var result = await ordersApiClient.CloseRentalAsync(
                _order.Id,
                request);

            if (result.IsFailure)
            {
                ErrorMessage = result.Error.Message;
                notificationService.Error(result.Error.Message);
                return;
            }

            IsSaved = true;
            notificationService.Success("Аренда успешно закрыта");
            OnPropertyChanged(nameof(IsSaved));
        }
        finally
        {
            IsSaving = false;
        }
    }

    private static DateTime GetLastReturnedDate(OrderDetailsDto order)
    {
        var lastReturnedDate = order.Items
            .Where(x => x.ActualReturnedDate is not null)
            .Max(x => x.ActualReturnedDate);

        return lastReturnedDate?.ToDateTime(TimeOnly.MinValue)
               ?? DateTime.Now;
    }

    partial void OnPaymentMethodIdChanged(Guid? value)
    {
        SaveCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsSavingChanged(bool value)
    {
        SaveCommand.NotifyCanExecuteChanged();
    }
}