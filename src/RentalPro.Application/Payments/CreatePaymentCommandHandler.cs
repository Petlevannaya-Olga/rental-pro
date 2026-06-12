using CSharpFunctionalExtensions;
using RentalPro.Application.Database;
using RentalPro.Application.Repositories;
using RentalPro.Application.Services;
using RentalPro.Domain.Payments;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Payments;

public sealed class CreatePaymentHandler(
    IPaymentsRepository paymentsRepository,
    IOrdersReadRepository ordersReadRepository,
    IFiscalReceiptService fiscalReceiptService,
    ITransactionManager transactionManager)
    : ICommandHandler<Guid, CreatePaymentCommand>
{
    public async Task<Result<Guid, Errors>> Handle(
        CreatePaymentCommand command,
        CancellationToken cancellationToken = default)
    {
        var transactionResult =
            await transactionManager.BeginTransactionAsync(cancellationToken);

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

        await paymentsRepository.AddAsync(
            payment,
            cancellationToken);

        var saveResult =
            await transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
        {
            transaction.Rollback();
            return saveResult.Error.ToErrors();
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

        var fiscalSaveResult =
            await transactionManager.SaveChangesAsync(cancellationToken);

        if (fiscalSaveResult.IsFailure)
        {
            transaction.Rollback();
            return fiscalSaveResult.Error.ToErrors();
        }

        var commitResult = transaction.Commit();

        if (commitResult.IsFailure)
            return commitResult.Error.ToErrors();

        return payment.Id.Value;
    }
}