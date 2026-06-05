using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Database;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Payments;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.PaymentTypes.UpdatePaymentTypeCommand;

public sealed class UpdatePaymentTypeCommandHandler(
    IPaymentTypesRepository paymentTypesRepository,
    ITransactionManager transactionManager,
    ILogger<UpdatePaymentTypeCommandHandler> logger)
    : ICommandHandler<UpdatePaymentTypeCommand>
{
    public async Task<UnitResult<Errors>> Handle(
        UpdatePaymentTypeCommand command,
        CancellationToken cancellationToken)
    {
        var paymentTypeId = PaymentTypeId.Restore(command.Id);

        var paymentTypeResult = await paymentTypesRepository.GetByAsync(
            x => x.Id == paymentTypeId,
            cancellationToken);

        if (paymentTypeResult.IsFailure)
            return paymentTypeResult.Error.ToErrors();

        if (paymentTypeResult.Value is null)
        {
            return CommonErrors.NotFound(
                    "payment.type.not.found",
                    $"Способ оплаты с id '{command.Id}' не найден")
                .ToErrors();
        }

        var updateResult = paymentTypeResult.Value.Update(command.Name);

        if (updateResult.IsFailure)
            return updateResult.Error.ToErrors();

        var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
            return saveResult.Error.ToErrors();

        logger.LogInformation(
            "Payment type with id '{PaymentTypeId}' was updated",
            command.Id);

        return UnitResult.Success<Errors>();
    }
}