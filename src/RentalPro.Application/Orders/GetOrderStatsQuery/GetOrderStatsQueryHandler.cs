using CSharpFunctionalExtensions;
using RentalPro.Application.Repositories;
using RentalPro.Contracts.Orders;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Orders.GetOrderStatsQuery;

public sealed class GetOrderStatsQueryHandler(
    IOrdersReadRepository repository)
    : IQueryHandler<OrderStatsDto, GetOrderStatsQuery>
{
    public async Task<Result<OrderStatsDto, Errors>> Handle(
        GetOrderStatsQuery query,
        CancellationToken cancellationToken)
    {
        return await repository.GetStatsAsync(
            cancellationToken);
    }
}