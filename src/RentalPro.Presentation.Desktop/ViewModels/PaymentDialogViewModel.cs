using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RentalPro.Contracts.Orders;
using RentalPro.Contracts.Payments;
using RentalPro.Presentation.Desktop.Api;
using RentalPro.Presentation.Desktop.Models;
using RentalPro.Presentation.Desktop.Services;

namespace RentalPro.Presentation.Desktop.ViewModels;

public partial class PaymentDialogViewModel(
    PaymentsApiClient paymentsApiClient,
    DictionariesApiClient dictionariesApiClient,
    NotificationService notificationService)
    : ObservableObject
{
    public decimal PaidRentalAmount =>
        _order?.PaidRentalAmount ?? 0;

    public decimal RemainingRentalAmount =>
        _order?.RemainingRentalAmount ?? 0;

    public decimal PaidDepositAmount =>
        _order?.PaidDepositAmount ?? 0;

    public decimal RemainingDepositAmount =>
        _order?.RemainingDepositAmount ?? 0;
    
    public bool CanEditAmount =>
        !IsFullPayment;
    
    private static readonly Guid FullPaymentId =
        Guid.Parse("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF");

    private OrderDetailsDto? _order;

    [ObservableProperty]
    private List<DictionaryItem> paymentTypes = [];

    [ObservableProperty]
    private List<DictionaryItem> paymentMethods = [];

    [ObservableProperty]
    private Guid? paymentTypeId;

    [ObservableProperty]
    private Guid? paymentMethodId;

    [ObservableProperty]
    private decimal amount;

    [ObservableProperty]
    private DateTime paymentDate = DateTime.Now;

    [ObservableProperty]
    private string? comment;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool isSaving;

    public bool IsSaved { get; private set; }

    public string CustomerName =>
        _order?.CustomerFullName ?? string.Empty;

    public decimal RentalAmount =>
        _order?.TotalCost ?? 0;

    public decimal DepositAmount =>
        _order?.DepositTotal ?? 0;

    public decimal TotalAmount =>
        RentalAmount + DepositAmount;

    public decimal PaidAmount =>
        _order?.TotalPaidAmount ?? 0;

    public decimal RemainingAmount =>
        _order?.TotalRemainingAmount ?? 0;

    public bool IsFullPayment =>
        PaymentTypeId == FullPaymentId;

    private bool CanSave()
    {
        return !IsSaving
               && _order is not null
               && PaymentTypeId is not null
               && PaymentMethodId is not null
               && Amount > 0;
    }

    public async Task OpenAsync(OrderDetailsDto order)
    {
        _order = order;
        IsSaved = false;
        ErrorMessage = string.Empty;

        PaymentTypeId = FullPaymentId;
        PaymentMethodId = null;
        Amount = order.TotalRemainingAmount;
        PaymentDate = DateTime.Now;
        Comment = null;

        await LoadDictionariesAsync();

        OnPropertyChanged(nameof(CustomerName));
        OnPropertyChanged(nameof(RentalAmount));
        OnPropertyChanged(nameof(DepositAmount));
        OnPropertyChanged(nameof(TotalAmount));
        OnPropertyChanged(nameof(PaidAmount));
        OnPropertyChanged(nameof(RemainingAmount));
        OnPropertyChanged(nameof(IsFullPayment));
        
        OnPropertyChanged(nameof(PaidRentalAmount));
        OnPropertyChanged(nameof(RemainingRentalAmount));
        OnPropertyChanged(nameof(PaidDepositAmount));
        OnPropertyChanged(nameof(RemainingDepositAmount));

        SaveCommand.NotifyCanExecuteChanged();
    }

    private async Task LoadDictionariesAsync()
    {
        var typesResult = await dictionariesApiClient.GetListAsync<DictionaryItem>(
            "api/payment-types",
            "payment.types.load.failed",
            "Не удалось загрузить типы оплаты");

        if (typesResult.IsFailure)
        {
            notificationService.Error(typesResult.Error.Message);
            return;
        }

        PaymentTypes =
        [
            new DictionaryItem()
            {
                Id = FullPaymentId,
                Name = "Полная оплата"
            },

            ..typesResult.Value
                .Where(x => x.Name is "Аренда" or "Залог")
        ];

        var methodsResult = await dictionariesApiClient.GetListAsync<DictionaryItem>(
            "api/payment-methods",
            "payment.methods.load.failed",
            "Не удалось загрузить способы оплаты");

        if (methodsResult.IsFailure)
        {
            notificationService.Error(methodsResult.Error.Message);
            return;
        }

        PaymentMethods = methodsResult.Value;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (_order is null)
        {
            ErrorMessage = "Заказ не выбран";
            return;
        }

        if (!ValidatePaymentAmount())
            return;

        try
        {
            IsSaving = true;
            ErrorMessage = string.Empty;

            if (PaymentTypeId == FullPaymentId)
            {
                await CreateFullPaymentAsync();
                return;
            }

            await CreateSinglePaymentAsync();
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task CreateFullPaymentAsync()
    {
        if (_order is null)
            return;

        if (PaymentMethodId is null)
        {
            ErrorMessage = "Выберите способ оплаты";
            return;
        }

        var rentalType = PaymentTypes.FirstOrDefault(x => x.Name == "Аренда");
        var depositType = PaymentTypes.FirstOrDefault(x => x.Name == "Залог");

        if (rentalType is null || depositType is null)
        {
            ErrorMessage = "Не найдены типы оплаты";
            notificationService.Error(ErrorMessage);
            return;
        }

        if (_order.RemainingRentalAmount > 0)
        {
            var rentalResult = await paymentsApiClient.CreatePaymentAsync(
                new CreatePaymentRequest(
                    OrderId: _order.Id,
                    PaymentTypeId: rentalType.Id,
                    PaymentMethodId: PaymentMethodId.Value,
                    Amount: _order.RemainingRentalAmount,
                    PaymentDate: PaymentDate,
                    Comment: Comment));

            if (rentalResult.IsFailure)
            {
                ErrorMessage = rentalResult.Error.Message;
                notificationService.Error(rentalResult.Error.Message);
                return;
            }
        }

        if (_order.RemainingDepositAmount > 0)
        {
            var depositResult = await paymentsApiClient.CreatePaymentAsync(
                new CreatePaymentRequest(
                    OrderId: _order.Id,
                    PaymentTypeId: depositType.Id,
                    PaymentMethodId: PaymentMethodId.Value,
                    Amount: _order.RemainingDepositAmount,
                    PaymentDate: PaymentDate,
                    Comment: Comment));

            if (depositResult.IsFailure)
            {
                ErrorMessage = depositResult.Error.Message;
                notificationService.Error(depositResult.Error.Message);
                return;
            }
        }

        IsSaved = true;
        notificationService.Success("Полная оплата успешно принята");
        OnPropertyChanged(nameof(IsSaved));
    }

    private async Task CreateSinglePaymentAsync()
    {
        if (_order is null)
            return;

        if (PaymentTypeId is null)
        {
            ErrorMessage = "Выберите тип оплаты";
            return;
        }

        if (PaymentMethodId is null)
        {
            ErrorMessage = "Выберите способ оплаты";
            return;
        }

        var request = new CreatePaymentRequest(
            OrderId: _order.Id,
            PaymentTypeId: PaymentTypeId.Value,
            PaymentMethodId: PaymentMethodId.Value,
            Amount: Amount,
            PaymentDate: PaymentDate,
            Comment: Comment);

        var result = await paymentsApiClient.CreatePaymentAsync(request);

        if (result.IsFailure)
        {
            ErrorMessage = result.Error.Message;
            notificationService.Error(result.Error.Message);
            return;
        }

        IsSaved = true;
        notificationService.Success("Оплата успешно принята");
        OnPropertyChanged(nameof(IsSaved));
    }

    private bool ValidatePaymentAmount()
    {
        if (_order is null)
            return false;

        if (PaymentTypeId is null)
        {
            ErrorMessage = "Выберите тип оплаты";
            return false;
        }

        if (Amount <= 0)
        {
            ErrorMessage = "Сумма оплаты должна быть больше 0";
            return false;
        }

        if (PaymentTypeId == FullPaymentId)
        {
            if (Amount != _order.TotalRemainingAmount)
            {
                ErrorMessage = "Полная оплата должна быть равна остатку к оплате";
                return false;
            }

            return true;
        }

        var selectedType = PaymentTypes
            .FirstOrDefault(x => x.Id == PaymentTypeId);

        if (selectedType is null)
        {
            ErrorMessage = "Тип оплаты не найден";
            return false;
        }

        var maxAmount = selectedType.Name switch
        {
            "Аренда" => _order.RemainingRentalAmount,
            "Залог" => _order.RemainingDepositAmount,
            _ => 0
        };

        if (Amount > maxAmount)
        {
            ErrorMessage =
                $"Сумма оплаты по типу «{selectedType.Name}» не может превышать {maxAmount:N2} ₽";

            return false;
        }

        return true;
    }

    partial void OnPaymentTypeIdChanged(Guid? value)
    {
        OnPropertyChanged(nameof(IsFullPayment));

        if (_order is null || value is null)
            return;

        if (value == FullPaymentId)
        {
            Amount = _order.TotalRemainingAmount;
            SaveCommand.NotifyCanExecuteChanged();
            return;
        }

        var selectedType = PaymentTypes.FirstOrDefault(x => x.Id == value);

        Amount = selectedType?.Name switch
        {
            "Аренда" => _order.RemainingRentalAmount,
            "Залог" => _order.RemainingDepositAmount,
            _ => 0
        };

        OnPropertyChanged(nameof(CanEditAmount));
        SaveCommand.NotifyCanExecuteChanged();
    }

    partial void OnPaymentMethodIdChanged(Guid? value)
    {
        SaveCommand.NotifyCanExecuteChanged();
    }

    partial void OnAmountChanged(decimal value)
    {
        SaveCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsSavingChanged(bool value)
    {
        SaveCommand.NotifyCanExecuteChanged();
    }
}