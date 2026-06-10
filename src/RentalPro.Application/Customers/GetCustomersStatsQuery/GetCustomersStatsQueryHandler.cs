using CSharpFunctionalExtensions;
using RentalPro.Application.Repositories;
using RentalPro.Contracts.Customers;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Customers.GetCustomersStatsQuery;

public sealed class GetCustomerStatsQueryHandler(
    ICustomersReadRepository readRepository)
    : IQueryHandler<CustomerStatsDto, GetCustomerStatsQuery>
{
    public async Task<Result<CustomerStatsDto, Errors>> Handle(
        GetCustomerStatsQuery query,
        CancellationToken cancellationToken)
    {
        return await readRepository.GetStatsAsync(
            cancellationToken);
    }
}