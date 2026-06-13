using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Payments.GetPaymentsQuery;

public sealed record GetPaymentsQuery(
    string? Search,
    Guid? PaymentTypeId,
    Guid? PaymentMethodId,
    DateTime? DateFrom,
    DateTime? DateTo,
    string? SortBy,
    bool Descending,
    int Page,
    int PageSize) : IQuery;