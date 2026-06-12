using CSharpFunctionalExtensions;
using RentalPro.Application.Repositories;
using RentalPro.Contracts.Orders;
using RentalPro.Domain.Orders;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Orders.GetOrderByIdQuery;

public sealed class GetOrderByIdQueryHandler(
    IOrdersReadRepository repository)
    : IQueryHandler<OrderDetailsDto, GetOrderByIdQuery>
{
    public async Task<Result<OrderDetailsDto, Errors>> Handle(
        GetOrderByIdQuery query,
        CancellationToken cancellationToken)
    {
        var orderIdResult = OrderId.Create(query.OrderId);

        if (orderIdResult.IsFailure)
            return orderIdResult.Error.ToErrors();

        return await repository.GetByIdAsync(
            orderIdResult.Value,
            cancellationToken);
    }
}