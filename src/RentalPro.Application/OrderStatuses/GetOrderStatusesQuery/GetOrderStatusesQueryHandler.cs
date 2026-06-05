using CSharpFunctionalExtensions;
using RentalPro.Application.Repositories;
using RentalPro.Contracts.OrderStatuses;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.OrderStatuses.GetOrderStatusesQuery;

public sealed class GetOrderStatusesQueryHandler(IOrderStatusesRepository repository)
    : IQueryHandler<IReadOnlyList<OrderStatusDto>, GetOrderStatusesQuery>
{
    public async Task<Result<IReadOnlyList<OrderStatusDto>, Errors>> Handle(
        GetOrderStatusesQuery query,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetAllAsync(cancellationToken);

        if (result.IsFailure)
            return result.Error.ToErrors();

        return result.Value
            .Select(x => new OrderStatusDto(
                x.Id.Value,
                x.Name.Value,
                x.CreatedAt,
                x.UpdatedAt))
            .ToList();
    }
}