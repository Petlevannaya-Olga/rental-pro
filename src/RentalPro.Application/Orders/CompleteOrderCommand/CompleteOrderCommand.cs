using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Orders.CompleteOrderCommand;

public sealed record CompleteOrderCommand(
    Guid OrderId,
    Guid CompletedStatusId)
    : ICommand;