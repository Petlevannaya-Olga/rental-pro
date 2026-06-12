using RentalPro.Domain.Orders;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Orders.ExportReturnActQuery;

public sealed record ExportReturnActQuery(
    OrderId OrderId,
    DateOnly ActDate) : IQuery;