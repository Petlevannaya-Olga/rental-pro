using CSharpFunctionalExtensions;
using RentalPro.Application.Repositories;
using RentalPro.Contracts.Orders;
using RentalPro.Domain.Orders;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Orders.GetOrdersQuery;

public sealed class GetOrdersQueryHandler(
    IOrdersReadRepository readRepository)
    : IQueryHandler<PagedResult<OrderDto>, GetOrdersQuery>
{
    public async Task<Result<PagedResult<OrderDto>, Errors>> Handle(
        GetOrdersQuery query,
        CancellationToken cancellationToken)
    {
        var statusIdResult = CreateOrderStatusId(query.StatusId);

        if (statusIdResult.IsFailure)
            return statusIdResult.Error.ToErrors();

        return await readRepository.GetPagedAsync(
            query.Search,
            statusIdResult.Value,
            query.StartFrom,
            query.StartTo,
            query.EndFrom,
            query.EndTo,
            query.SortBy,
            query.Descending,
            query.Page,
            query.PageSize,
            cancellationToken);
    }

    private static Result<OrderStatusId?, Error> CreateOrderStatusId(
        Guid? statusId)
    {
        if (statusId is null)
            return (OrderStatusId?)null;

        var result = OrderStatusId.Create(statusId.Value);

        if (result.IsFailure)
            return result.Error;

        return result.Value;
    }
}