using CSharpFunctionalExtensions;
using RentalPro.Contracts.Orders;
using RentalPro.Domain.Orders;
using RentalPro.Shared;

namespace RentalPro.Application.Repositories;

public interface IOrdersReadRepository
{
    Task<Result<PagedResult<OrderDto>, Errors>> GetPagedAsync(
        string? search,
        OrderStatusId? statusId,
        DateOnly? startFrom,
        DateOnly? startTo,
        DateOnly? endFrom,
        DateOnly? endTo,
        string? sortBy,
        bool descending,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<Result<OrderStatsDto, Errors>> GetStatsAsync(
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<OrderDto>, Errors>> GetForExportAsync(
        string? search,
        OrderStatusId? statusId,
        DateOnly? startFrom,
        DateOnly? startTo,
        DateOnly? endFrom,
        DateOnly? endTo,
        string? sortBy,
        bool descending,
        CancellationToken cancellationToken);
}