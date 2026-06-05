using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.PaymentMethods.UpdatePaymentMethodCommand;

public sealed record UpdatePaymentMethodCommand(
    Guid Id,
    string Name) : IValidation;