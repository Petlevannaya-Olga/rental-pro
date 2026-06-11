using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Orders.DeleteOrderCommand;

public sealed record DeleteOrderCommand(
    Guid Id)
    : IValidation;