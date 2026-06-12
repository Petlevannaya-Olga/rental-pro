using RentalPro.Domain.Orders;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Orders.GetOrderDocumentsQuery;

public sealed record GetOrderDocumentsQuery(OrderId OrderId) : IQuery;