using CSharpFunctionalExtensions;
using RentalPro.Application.Repositories;
using RentalPro.Contracts.Customers;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Customers.GetCustomersQuery;

public sealed class GetCustomersQueryHandler(
    ICustomersReadRepository readRepository)
    : IQueryHandler<PagedResult<CustomerDto>, GetCustomersQuery>
{
    public async Task<Result<PagedResult<CustomerDto>, Errors>> Handle(
        GetCustomersQuery query,
        CancellationToken cancellationToken)
    {
        return await readRepository.GetPagedAsync(
            query.Search,
            query.HasOrders,
            query.HasDebt,
            query.SortBy,
            query.Descending,
            query.Page,
            query.PageSize,
            cancellationToken);
    }
}