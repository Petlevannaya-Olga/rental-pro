using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Orders.ReturnOrderItemsCommand;

public sealed record ReturnOrderItemsCommand(
    Guid OrderId,
    DateOnly ActualReturnedDate,
    string? ReturnCondition,
    string? DamageComment,
    IReadOnlyList<Guid> OrderItemIds)
    : IValidation;