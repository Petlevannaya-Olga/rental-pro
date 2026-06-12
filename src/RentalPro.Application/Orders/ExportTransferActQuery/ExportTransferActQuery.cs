using RentalPro.Domain.Orders;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Orders.ExportTransferActQuery;

public sealed record ExportTransferActQuery(
    OrderId OrderId,
    DateOnly ActDate) : IQuery;