using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Payments.ExportPaymentsQuery;

public sealed record ExportPaymentsQuery(
    string? Search,
    Guid? PaymentTypeId,
    Guid? PaymentMethodId,
    DateTime? DateFrom,
    DateTime? DateTo,
    string? SortBy,
    bool Descending) : IQuery;