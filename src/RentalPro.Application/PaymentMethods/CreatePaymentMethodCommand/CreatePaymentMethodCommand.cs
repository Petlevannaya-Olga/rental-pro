using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.PaymentMethods.CreatePaymentMethodCommand;

public sealed record CreatePaymentMethodCommand(
    string Name) : IValidation;