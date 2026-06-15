using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Orders.CloseRentalCommand;

public sealed record CloseRentalCommand(
    Guid OrderId,
    Guid PaymentMethodId,
    DateTime PaymentDate,
    string? Comment) : IValidation;