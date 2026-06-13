using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Payments.CreatePaymentCommand;

public sealed record CreatePaymentCommand(
    Guid OrderId,
    Guid PaymentTypeId,
    Guid PaymentMethodId,
    decimal Amount,
    DateTime PaymentDate,
    string? Comment)
    : IValidation;