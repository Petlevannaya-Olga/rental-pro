using CSharpFunctionalExtensions;
using RentalPro.Application.Database;
using RentalPro.Application.Repositories;
using RentalPro.Application.Services;
using RentalPro.Domain.Orders;
using RentalPro.Domain.Payments;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Payments;

public sealed class CreatePaymentHandler(
    IPaymentsRepository paymentsRepository,
    IOrdersRepository ordersRepository,
    IOrdersReadRepository ordersReadRepository,
    IOrderStatusesRepository orderStatusesRepository,
    IFiscalReceiptService fiscalReceiptService,
    IPaymentTypesRepository paymentTypesRepository,
    ITransactionManager transactionManager)
    : ICommandHandler<Guid, CreatePaymentCommand>
{
    public async Task<Result<Guid, Errors>> Handle(
        CreatePaymentCommand command,
        CancellationToken cancellationToken = default)
    {
        var transactionResult = await transactionManager.BeginTransactionAsync(
            cancellationToken);

        if (transactionResult.IsFailure)
            return transactionResult.Error.ToErrors();

        using var transaction = transactionResult.Value;

        var paymentResult = Payment.Create(
            command.OrderId,
            command.PaymentMethodId,
            command.PaymentTypeId,
            command.PaymentDate,
            command.Amount,
            command.Comment);

        if (paymentResult.IsFailure)
        {
            transaction.Rollback();
            return paymentResult.Error.ToErrors();
        }

        var payment = paymentResult.Value;

        Console.WriteLine($"PAYMENT TYPE ENTITY: {payment.PaymentTypeId.Value}");
        Console.WriteLine($"PAYMENT METHOD ENTITY: {payment.PaymentMethodId.Value}");

        var addPaymentResult = await paymentsRepository.AddAsync(
            payment,
            cancellationToken);

        if (addPaymentResult.IsFailure)
        {
            transaction.Rollback();
            return addPaymentResult.Error;
        }

        var savePaymentResult = await transactionManager.SaveChangesAsync(
            cancellationToken);

        if (savePaymentResult.IsFailure)
        {
            transaction.Rollback();
            return savePaymentResult.Error.ToErrors();
        }

        var fiscalizationDataResult =
            await ordersReadRepository.GetPaymentFiscalizationDataAsync(
                payment.Id,
                cancellationToken);

        if (fiscalizationDataResult.IsSuccess)
        {
            var fiscalResult =
                await fiscalReceiptService.CreateReceiptAsync(
                    fiscalizationDataResult.Value,
                    cancellationToken);

            if (fiscalResult.IsSuccess)
            {
                payment.MarkFiscalized(
                    fiscalResult.Value.ReceiptId,
                    fiscalResult.Value.Status,
                    fiscalResult.Value.FiscalizedAt);
            }
            else
            {
                payment.MarkFiscalizationFailed(
                    fiscalResult.Error.Message);
            }
        }
        else
        {
            payment.MarkFiscalizationFailed(
                fiscalizationDataResult.Error.Message);
        }

        var saveFiscalizationResult = await transactionManager.SaveChangesAsync(
            cancellationToken);

        if (saveFiscalizationResult.IsFailure)
        {
            transaction.Rollback();
            return saveFiscalizationResult.Error.ToErrors();
        }

        var completePaymentResult = await MoveOrderToReadyForIssueIfPaidAsync(
            command.OrderId,
            cancellationToken);

        if (completePaymentResult.IsFailure)
        {
            transaction.Rollback();
            return completePaymentResult.Error;
        }

        var completeOrderResult = await MoveOrderToCompletedIfDepositRefundedAsync(
            command.OrderId,
            command.PaymentTypeId,
            command.Amount,
            cancellationToken);

        if (completeOrderResult.IsFailure)
        {
            transaction.Rollback();
            return completeOrderResult.Error;
        }

        var saveOrderStatusResult = await transactionManager.SaveChangesAsync(
            cancellationToken);

        if (saveOrderStatusResult.IsFailure)
        {
            transaction.Rollback();
            return saveOrderStatusResult.Error.ToErrors();
        }

        var commitResult = transaction.Commit();

        if (commitResult.IsFailure)
            return commitResult.Error.ToErrors();

        return payment.Id.Value;
    }

    private async Task<UnitResult<Errors>> MoveOrderToCompletedIfDepositRefundedAsync(
        Guid orderId,
        Guid paymentTypeId,
        decimal refundAmount,
        CancellationToken cancellationToken)
    {
        var orderIdResult = OrderId.Create(orderId);

        if (orderIdResult.IsFailure)
            return orderIdResult.Error.ToErrors();

        var paymentTypeNameResult = PaymentTypeName.Create("Возврат залога");

        if (paymentTypeNameResult.IsFailure)
            return paymentTypeNameResult.Error.ToErrors();

        var paymentTypeResult = await paymentTypesRepository.GetByAsync(
            x => x.Name == paymentTypeNameResult.Value,
            cancellationToken);

        if (paymentTypeResult.IsFailure)
            return paymentTypeResult.Error.ToErrors();

        if (paymentTypeResult.Value.Id.Value != paymentTypeId)
            return UnitResult.Success<Errors>();

        var orderDetailsResult = await ordersReadRepository.GetByIdAsync(
            orderIdResult.Value,
            cancellationToken);

        if (orderDetailsResult.IsFailure)
            return orderDetailsResult.Error;

        var orderDetails = orderDetailsResult.Value;

        if (!orderDetails.AllItemsReturned)
            return UnitResult.Success<Errors>();

        if (orderDetails.RemainingDepositRefundAmount > 0.01m)
            return UnitResult.Success<Errors>();

        var completedStatusNameResult = OrderStatusName.Create("Завершен");

        if (completedStatusNameResult.IsFailure)
            return completedStatusNameResult.Error.ToErrors();

        var completedStatusResult = await orderStatusesRepository.GetByAsync(
            x => x.Name == completedStatusNameResult.Value,
            cancellationToken);

        if (completedStatusResult.IsFailure)
            return completedStatusResult.Error.ToErrors();

        var orderResult = await ordersRepository.GetByAsync(
            x => x.Id == orderIdResult.Value,
            cancellationToken);

        if (orderResult.IsFailure)
            return orderResult.Error.ToErrors();

        var order = orderResult.Value;

        var changeStatusResult = order.ChangeStatus(
            completedStatusResult.Value.Id.Value);

        if (changeStatusResult.IsFailure)
            return changeStatusResult.Error.ToErrors();

        return UnitResult.Success<Errors>();
    }

    private async Task<UnitResult<Errors>> MoveOrderToReadyForIssueIfPaidAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        var orderIdResult = OrderId.Create(orderId);

        if (orderIdResult.IsFailure)
            return orderIdResult.Error.ToErrors();

        var orderDetailsResult = await ordersReadRepository.GetByIdAsync(
            orderIdResult.Value,
            cancellationToken);

        if (orderDetailsResult.IsFailure)
            return orderDetailsResult.Error;

        var orderDetails = orderDetailsResult.Value;

        if (orderDetails.RemainingRentalAmount > 0.01m ||
            orderDetails.RemainingDepositAmount > 0.01m)
        {
            return UnitResult.Success<Errors>();
        }

        var isConfirmed =
            orderDetails.StatusName is "Подтверждён" or "Подтвержден";

        if (!isConfirmed)
        {
            return UnitResult.Success<Errors>();
        }

        var readyStatusNameResult = OrderStatusName.Create("Готов к выдаче");

        if (readyStatusNameResult.IsFailure)
            return readyStatusNameResult.Error.ToErrors();

        var readyStatusResult = await orderStatusesRepository.GetByAsync(
            x => x.Name == readyStatusNameResult.Value,
            cancellationToken);

        if (readyStatusResult.IsFailure)
            return readyStatusResult.Error.ToErrors();

        var orderResult = await ordersRepository.GetByAsync(
            x => x.Id == orderIdResult.Value,
            cancellationToken);

        if (orderResult.IsFailure)
            return orderResult.Error.ToErrors();

        var order = orderResult.Value;

        var changeStatusResult = order.ChangeStatus(
            readyStatusResult.Value.Id.Value);

        if (changeStatusResult.IsFailure)
            return changeStatusResult.Error.ToErrors();

        return UnitResult.Success<Errors>();
    }
}