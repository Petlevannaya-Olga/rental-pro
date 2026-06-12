using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Orders.GetOrderByIdQuery;

public sealed record GetOrderByIdQuery(Guid OrderId) : IQuery;