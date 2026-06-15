using CSharpFunctionalExtensions;
using RentalPro.Application.Database;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Orders;
using RentalPro.Domain.Payments;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Orders.CloseRentalCommand;

public sealed class CloseRentalCommandHandler(
    IOrdersRepository ordersRepository,
    IOrdersReadRepository ordersReadRepository,
    IOrderStatusesRepository orderStatusesRepository,
    IPaymentsRepository paymentsRepository,
    IPaymentTypesRepository paymentTypesRepository,
    ITransactionManager transactionManager)
    : ICommandHandler<CloseRentalCommand>
{
    public async Task<UnitResult<Errors>> Handle(
        CloseRentalCommand command,
        CancellationToken cancellationToken)
    {
        var orderIdResult = OrderId.Create(command.OrderId);

        if (orderIdResult.IsFailure)
            return orderIdResult.Error.ToErrors();

        var paymentMethodIdResult = PaymentMethodId.Create(command.PaymentMethodId);

        if (paymentMethodIdResult.IsFailure)
            return paymentMethodIdResult.Error.ToErrors();

        var orderId = orderIdResult.Value;

        var orderResult = await ordersRepository.GetByAsync(
            order => order.Id == orderId,
            cancellationToken);

        if (orderResult.IsFailure)
            return orderResult.Error.ToErrors();

        var order = orderResult.Value;

        if (order is null)
        {
            return CommonErrors.NotFound(
                    "order.not.found",
                    "Заказ не найден",
                    command.OrderId)
                .ToErrors();
        }

        var waitingClosingStatusResult = await GetOrderStatusAsync(
            "Ожидает закрытия аренды",
            "order.status.waiting.rental.closing.not.found",
            "Статус заказа «Ожидает закрытия аренды» не найден",
            cancellationToken);

        if (waitingClosingStatusResult.IsFailure)
            return waitingClosingStatusResult.Error;

        var completedStatusResult = await GetOrderStatusAsync(
            "Завершен",
            "order.status.completed.not.found",
            "Статус заказа «Завершен» не найден",
            cancellationToken);

        if (completedStatusResult.IsFailure)
            return completedStatusResult.Error;

        var waitingClosingStatus = waitingClosingStatusResult.Value;
        var completedStatus = completedStatusResult.Value;

        if (order.StatusId != waitingClosingStatus.Id)
        {
            return CommonErrors.Validation(
                    "order.cannot.be.closed",
                    "Закрыть аренду можно только для заказа в статусе «Ожидает закрытия аренды»")
                .ToErrors();
        }

        var itemsResult = await ordersRepository.GetItemsAsync(
            orderId,
            cancellationToken);

        if (itemsResult.IsFailure)
            return itemsResult.Error.ToErrors();

        var items = itemsResult.Value;

        if (items.Count == 0)
        {
            return CommonErrors.Validation(
                    "order.items.not.found",
                    "В заказе нет инструментов")
                .ToErrors();
        }

        if (items.Any(item => item.ActualReturnedDate is null))
        {
            return CommonErrors.Validation(
                    "order.items.not.returned",
                    "Закрыть аренду можно только после возврата всех инструментов")
                .ToErrors();
        }

        var calculationResult =
            await ordersReadRepository.GetCloseRentalCalculationAsync(
                orderId,
                cancellationToken);

        if (calculationResult.IsFailure)
            return calculationResult.Error;

        var calculation = calculationResult.Value;

        var paymentTypesResult = await paymentTypesRepository.GetAllAsync(
            cancellationToken);

        if (paymentTypesResult.IsFailure)
            return paymentTypesResult.Error.ToErrors();

        var rentalPaymentType = paymentTypesResult.Value
            .FirstOrDefault(type => type.Name.Value == "Аренда");

        var rentalRefundPaymentType = paymentTypesResult.Value
            .FirstOrDefault(type => type.Name.Value == "Возврат аренды");

        var depositRefundPaymentType = paymentTypesResult.Value
            .FirstOrDefault(type => type.Name.Value == "Возврат залога");

        if (rentalPaymentType is null)
        {
            return CommonErrors.NotFound(
                    "payment.type.rental.not.found",
                    "Тип оплаты «Аренда» не найден")
                .ToErrors();
        }

        if (rentalRefundPaymentType is null)
        {
            return CommonErrors.NotFound(
                    "payment.type.rental.refund.not.found",
                    "Тип оплаты «Возврат аренды» не найден")
                .ToErrors();
        }

        if (depositRefundPaymentType is null)
        {
            return CommonErrors.NotFound(
                    "payment.type.deposit.refund.not.found",
                    "Тип оплаты «Возврат залога» не найден")
                .ToErrors();
        }

        var transactionResult = await transactionManager.BeginTransactionAsync(
            cancellationToken);

        if (transactionResult.IsFailure)
            return transactionResult.Error.ToErrors();

        using var transaction = transactionResult.Value;

        if (calculation.RentalAdditionalPaymentAmount > 0)
        {
            var addPaymentResult = await AddPaymentAsync(
                orderId: command.OrderId,
                paymentMethodId: command.PaymentMethodId,
                paymentTypeId: rentalPaymentType.Id.Value,
                paymentDate: command.PaymentDate,
                amount: calculation.RentalAdditionalPaymentAmount,
                comment: command.Comment,
                cancellationToken: cancellationToken);

            if (addPaymentResult.IsFailure)
            {
                transaction.Rollback();
                return addPaymentResult.Error;
            }
        }

        if (calculation.RentalRefundAmount > 0)
        {
            var addPaymentResult = await AddPaymentAsync(
                orderId: command.OrderId,
                paymentMethodId: command.PaymentMethodId,
                paymentTypeId: rentalRefundPaymentType.Id.Value,
                paymentDate: command.PaymentDate,
                amount: calculation.RentalRefundAmount,
                comment: command.Comment,
                cancellationToken: cancellationToken);

            if (addPaymentResult.IsFailure)
            {
                transaction.Rollback();
                return addPaymentResult.Error;
            }
        }

        if (calculation.DepositRefundAmount > 0)
        {
            var addPaymentResult = await AddPaymentAsync(
                orderId: command.OrderId,
                paymentMethodId: command.PaymentMethodId,
                paymentTypeId: depositRefundPaymentType.Id.Value,
                paymentDate: command.PaymentDate,
                amount: calculation.DepositRefundAmount,
                comment: command.Comment,
                cancellationToken: cancellationToken);

            if (addPaymentResult.IsFailure)
            {
                transaction.Rollback();
                return addPaymentResult.Error;
            }
        }

        order.ChangeStatus(completedStatus.Id.Value);

        var saveResult = await transactionManager.SaveChangesAsync(
            cancellationToken);

        if (saveResult.IsFailure)
        {
            transaction.Rollback();
            return saveResult.Error.ToErrors();
        }

        var commitResult = transaction.Commit();

        if (commitResult.IsFailure)
            return commitResult.Error.ToErrors();

        return UnitResult.Success<Errors>();
    }

    private async Task<UnitResult<Errors>> AddPaymentAsync(
        Guid orderId,
        Guid paymentMethodId,
        Guid paymentTypeId,
        DateTime paymentDate,
        decimal amount,
        string? comment,
        CancellationToken cancellationToken)
    {
        var paymentResult = Payment.Create(
            orderId,
            paymentMethodId,
            paymentTypeId,
            paymentDate,
            amount,
            comment);

        if (paymentResult.IsFailure)
            return paymentResult.Error.ToErrors();

        var addResult = await paymentsRepository.AddAsync(
            paymentResult.Value,
            cancellationToken);

        if (addResult.IsFailure)
            return addResult.Error;

        return UnitResult.Success<Errors>();
    }

    private async Task<Result<OrderStatus, Errors>> GetOrderStatusAsync(
        string name,
        string notFoundCode,
        string notFoundMessage,
        CancellationToken cancellationToken)
    {
        var nameResult = OrderStatusName.Create(name);

        if (nameResult.IsFailure)
            return nameResult.Error.ToErrors();

        var statusResult = await orderStatusesRepository.GetByAsync(
            status => status.Name == nameResult.Value,
            cancellationToken);

        if (statusResult.IsFailure)
            return statusResult.Error.ToErrors();

        var status = statusResult.Value;

        if (status is null)
        {
            return CommonErrors.NotFound(
                    notFoundCode,
                    notFoundMessage)
                .ToErrors();
        }

        return status;
    }
}