using CSharpFunctionalExtensions;
using RentalPro.Application.Repositories;
using RentalPro.Contracts.Payments;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Payments.GetPaymentStatsQuery;

public sealed class GetPaymentStatsQueryHandler(
    IPaymentsReadRepository paymentsReadRepository)
    : IQueryHandler<PaymentStatsDto, GetPaymentStatsQuery>
{
    public async Task<Result<PaymentStatsDto, Errors>> Handle(
        GetPaymentStatsQuery query,
        CancellationToken cancellationToken = default)
    {
        return await paymentsReadRepository.GetStatsAsync(cancellationToken);
    }
}