using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Payments;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.PaymentTypes.CreatePaymentTypeCommand;

public sealed class CreatePaymentTypeCommandHandler(
    IPaymentTypesRepository paymentTypesRepository,
    ILogger<CreatePaymentTypeCommandHandler> logger)
    : ICommandHandler<CreatePaymentTypeCommand>
{
    public async Task<UnitResult<Errors>> Handle(
        CreatePaymentTypeCommand command,
        CancellationToken cancellationToken)
    {
        var paymentTypeResult = PaymentType.Create(command.Name);

        if (paymentTypeResult.IsFailure)
            return paymentTypeResult.Error.ToErrors();

        var addResult = await paymentTypesRepository.AddAsync(
            paymentTypeResult.Value,
            cancellationToken);

        if (addResult.IsFailure)
            return addResult.Error.ToErrors();

        logger.LogInformation(
            "Payment Type '{PaymentTypeName}' was created",
            command.Name);

        return UnitResult.Success<Errors>();
    }
}