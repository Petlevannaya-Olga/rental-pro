using CSharpFunctionalExtensions;
using RentalPro.Contracts.Customers;
using RentalPro.Shared;

namespace RentalPro.Application.Repositories;

public interface ICustomersReadRepository
{
    Task<Result<PagedResult<CustomerDto>, Errors>> GetPagedAsync(
        string? search,
        bool? hasOrders,
        bool? isRegular,
        bool? hasActiveOrders,
        string? sortBy,
        bool descending,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<Result<CustomerStatsDto, Errors>> GetStatsAsync(
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<CustomerDto>, Errors>> GetForExportAsync(
        string? search,
        bool? hasOrders,
        bool? isRegular,
        bool? hasActiveOrders,
        string? sortBy,
        bool descending,
        CancellationToken cancellationToken);
    
    Task<Result<List<CustomerOrderHistoryItemDto>, Errors>> GetOrderHistoryAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);
}