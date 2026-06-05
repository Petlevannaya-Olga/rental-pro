using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.PaymentTypes.UpdatePaymentTypeCommand;

public sealed record UpdatePaymentTypeCommand(
    Guid Id,
    string Name) : IValidation;