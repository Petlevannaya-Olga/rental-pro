using CSharpFunctionalExtensions;
using RentalPro.Application.Repositories;
using RentalPro.Contracts.Customers;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Customers.GetCustomerOrderHistoryQuery;

public sealed class GetCustomerOrderHistoryQueryHandler(
    ICustomersReadRepository repository)
    : IQueryHandler<List<CustomerOrderHistoryItemDto>, GetCustomerOrderHistoryQuery>
{
    public Task<Result<List<CustomerOrderHistoryItemDto>, Errors>> Handle(
        GetCustomerOrderHistoryQuery query,
        CancellationToken cancellationToken)
    {
        return repository.GetOrderHistoryAsync(
            query.CustomerId,
            cancellationToken);
    }
}