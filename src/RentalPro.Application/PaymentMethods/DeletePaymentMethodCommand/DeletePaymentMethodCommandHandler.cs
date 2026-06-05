using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Database;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Payments;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.PaymentMethods.DeletePaymentMethodCommand;

public sealed class DeletePaymentMethodCommandHandler(
    IPaymentMethodsRepository paymentMethodsRepository,
    ITransactionManager transactionManager,
    ILogger<DeletePaymentMethodCommandHandler> logger)
    : ICommandHandler<DeletePaymentMethodCommand>
{
    public async Task<UnitResult<Errors>> Handle(
        DeletePaymentMethodCommand command,
        CancellationToken cancellationToken)
    {
        var paymentMethodId = PaymentMethodId.Restore(command.Id);

        var paymentMethodResult = await paymentMethodsRepository.GetByAsync(
            x => x.Id == paymentMethodId,
            cancellationToken);

        if (paymentMethodResult.IsFailure)
            return paymentMethodResult.Error.ToErrors();

        if (paymentMethodResult.Value is null)
        {
            return CommonErrors.NotFound(
                    "payment.method.not.found",
                    $"Способ оплаты с id '{command.Id}' не найден")
                .ToErrors();
        }

        var deleteResult = paymentMethodResult.Value.Delete();

        if (deleteResult.IsFailure)
            return deleteResult.Error.ToErrors();

        var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
            return saveResult.Error.ToErrors();

        logger.LogInformation(
            "Payment method with id '{PaymentMethodId}' was deleted",
            command.Id);

        return UnitResult.Success<Errors>();
    }
}