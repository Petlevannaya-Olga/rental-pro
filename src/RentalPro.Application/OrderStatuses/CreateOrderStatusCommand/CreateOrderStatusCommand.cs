using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.OrderStatuses.CreateOrderStatusCommand;

public sealed record CreateOrderStatusCommand(string Name) : IValidation;