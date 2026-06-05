using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Payments;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.PaymentMethods.CreatePaymentMethodCommand;

public sealed class CreatePaymentMethodCommandHandler(
    IPaymentMethodsRepository paymentMethodsRepository,
    ILogger<CreatePaymentMethodCommandHandler> logger)
    : ICommandHandler<CreatePaymentMethodCommand>
{
    public async Task<UnitResult<Errors>> Handle(
        CreatePaymentMethodCommand command,
        CancellationToken cancellationToken)
    {
        var paymentMethodResult = PaymentMethod.Create(command.Name);

        if (paymentMethodResult.IsFailure)
            return paymentMethodResult.Error.ToErrors();

        var addResult = await paymentMethodsRepository.AddAsync(
            paymentMethodResult.Value,
            cancellationToken);

        if (addResult.IsFailure)
            return addResult.Error.ToErrors();

        logger.LogInformation(
            "Payment method '{PaymentMethodName}' was created",
            command.Name);

        return UnitResult.Success<Errors>();
    }
}