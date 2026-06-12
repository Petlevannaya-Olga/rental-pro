using RentalPro.Domain.Orders;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Orders.ExportRentalContractPdfQuery;

public sealed record ExportRentalContractPdfQuery(OrderId OrderId) : IQuery;