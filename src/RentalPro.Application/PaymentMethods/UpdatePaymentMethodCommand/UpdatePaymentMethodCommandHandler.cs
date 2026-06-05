using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Database;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Payments;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.PaymentMethods.UpdatePaymentMethodCommand;

public sealed class UpdatePaymentMethodCommandHandler(
    IPaymentMethodsRepository paymentMethodsRepository,
    ITransactionManager transactionManager,
    ILogger<UpdatePaymentMethodCommandHandler> logger)
    : ICommandHandler<UpdatePaymentMethodCommand>
{
    public async Task<UnitResult<Errors>> Handle(
        UpdatePaymentMethodCommand command,
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

        var updateResult = paymentMethodResult.Value.Update(command.Name);

        if (updateResult.IsFailure)
            return updateResult.Error.ToErrors();

        var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
            return saveResult.Error.ToErrors();

        logger.LogInformation(
            "Payment method with id '{PaymentMethodId}' was updated",
            command.Id);

        return UnitResult.Success<Errors>();
    }
}