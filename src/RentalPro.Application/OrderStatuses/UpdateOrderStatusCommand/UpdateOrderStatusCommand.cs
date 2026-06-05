using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.OrderStatuses.UpdateOrderStatusCommand;

public sealed record UpdateOrderStatusCommand(
    Guid Id,
    string Name) : IValidation;