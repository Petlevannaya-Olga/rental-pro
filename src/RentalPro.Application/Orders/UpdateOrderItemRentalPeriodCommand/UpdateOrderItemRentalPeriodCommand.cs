using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Orders.UpdateOrderItemRentalPeriodCommand;

public sealed record UpdateOrderItemRentalPeriodCommand(
    Guid OrderItemId,
    DateOnly StartDate,
    DateOnly PlannedReturnDate)
    : IValidation;