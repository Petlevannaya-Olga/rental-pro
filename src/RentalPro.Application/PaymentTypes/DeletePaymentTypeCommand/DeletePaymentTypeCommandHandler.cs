using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Database;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Payments;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.PaymentTypes.DeletePaymentTypeCommand;

public sealed class DeletePaymentTypeCommandHandler(
    IPaymentTypesRepository paymentTypesRepository,
    ITransactionManager transactionManager,
    ILogger<DeletePaymentTypeCommandHandler> logger)
    : ICommandHandler<DeletePaymentTypeCommand>
{
    public async Task<UnitResult<Errors>> Handle(
        DeletePaymentTypeCommand command,
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

        var deleteResult = paymentTypeResult.Value.Delete();

        if (deleteResult.IsFailure)
            return deleteResult.Error.ToErrors();

        var saveResult = await transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
            return saveResult.Error.ToErrors();

        logger.LogInformation(
            "Payment type with id '{PaymentTypeId}' was deleted",
            command.Id);

        return UnitResult.Success<Errors>();
    }
}