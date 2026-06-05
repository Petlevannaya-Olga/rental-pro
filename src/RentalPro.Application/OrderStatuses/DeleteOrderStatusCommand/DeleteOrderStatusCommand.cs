using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.OrderStatuses.DeleteOrderStatusCommand;

public sealed record DeleteOrderStatusCommand(Guid Id) : IValidation;