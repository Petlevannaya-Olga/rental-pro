using RentalPro.Domain.Orders;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Orders.ExportRentalContractQuery;

public sealed record ExportRentalContractQuery(OrderId OrderId) : IQuery;