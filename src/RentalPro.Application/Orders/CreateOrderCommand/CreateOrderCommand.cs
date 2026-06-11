using RentalPro.Contracts.Orders;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Orders.CreateOrderCommand;

public sealed record CreateOrderCommand(
    Guid UserId,
    Guid CustomerId,
    Guid StatusId,
    DateTime OrderDate,
    decimal DepositTotal,
    string? Comment,
    IReadOnlyList<CreateOrderItemDto> Items)
    : IValidation;