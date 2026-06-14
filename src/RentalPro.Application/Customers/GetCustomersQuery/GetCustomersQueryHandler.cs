using CSharpFunctionalExtensions;
using RentalPro.Application.Repositories;
using RentalPro.Contracts.Customers;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Customers.GetCustomersQuery;

public sealed class GetCustomersQueryHandler(
    ICustomersReadRepository customersReadRepository)
    : IQueryHandler<PagedResult<CustomerDto>, GetCustomersQuery>
{
    public async Task<Result<PagedResult<CustomerDto>, Errors>> Handle(
        GetCustomersQuery query,
        CancellationToken cancellationToken)
    {
        return await customersReadRepository.GetPagedAsync(
            query.Search,
            query.HasOrders,
            query.IsRegular,
            query.HasActiveOrders,
            query.SortBy,
            query.Descending,
            query.Page,
            query.PageSize,
            cancellationToken);
    }
}