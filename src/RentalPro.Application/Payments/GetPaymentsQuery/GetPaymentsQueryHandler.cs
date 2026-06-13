using CSharpFunctionalExtensions;
using RentalPro.Application.Repositories;
using RentalPro.Contracts.Payments;
using RentalPro.Domain.Payments;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Payments.GetPaymentsQuery;

public sealed class GetPaymentsQueryHandler(
    IPaymentsReadRepository paymentsReadRepository)
    : IQueryHandler<PagedResult<PaymentDto>, GetPaymentsQuery>
{
    public async Task<Result<PagedResult<PaymentDto>, Errors>> Handle(
        GetPaymentsQuery query,
        CancellationToken cancellationToken = default)
    {
        PaymentTypeId? paymentTypeId = query.PaymentTypeId.HasValue
            ? PaymentTypeId.Restore(query.PaymentTypeId.Value)
            : null;

        PaymentMethodId? paymentMethodId = query.PaymentMethodId.HasValue
            ? PaymentMethodId.Restore(query.PaymentMethodId.Value)
            : null;

        return await paymentsReadRepository.GetPagedAsync(
            query.Search,
            paymentTypeId,
            paymentMethodId,
            query.DateFrom,
            query.DateTo,
            query.SortBy,
            query.Descending,
            query.Page,
            query.PageSize,
            cancellationToken);
    }
}