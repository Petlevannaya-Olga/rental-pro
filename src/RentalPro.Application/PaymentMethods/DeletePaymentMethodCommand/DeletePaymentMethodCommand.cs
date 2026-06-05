using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.PaymentMethods.DeletePaymentMethodCommand;

public sealed record DeletePaymentMethodCommand(Guid Id) : IValidation;