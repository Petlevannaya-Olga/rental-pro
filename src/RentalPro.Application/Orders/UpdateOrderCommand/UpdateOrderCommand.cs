using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Orders.UpdateOrderCommand;

public sealed record UpdateOrderCommand(
    Guid Id,
    Guid StatusId,
    string? Comment)
    : IValidation;

