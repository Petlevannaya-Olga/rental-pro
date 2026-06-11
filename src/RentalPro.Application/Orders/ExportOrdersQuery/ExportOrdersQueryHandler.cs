using CSharpFunctionalExtensions;
using RentalPro.Application.Repositories;
using RentalPro.Application.Services;
using RentalPro.Contracts.Orders;
using RentalPro.Domain.Orders;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Orders.ExportOrdersQuery;

public sealed class ExportOrdersQueryHandler(
    IOrdersReadRepository readRepository,
    IExcelExportService<OrderDto> exportService)
    : IQueryHandler<byte[], ExportOrdersQuery>
{
    public async Task<Result<byte[], Errors>> Handle(
        ExportOrdersQuery query,
        CancellationToken cancellationToken)
    {
        var statusIdResult = CreateOrderStatusId(query.StatusId);

        if (statusIdResult.IsFailure)
            return statusIdResult.Error.ToErrors();

        var ordersResult = await readRepository.GetForExportAsync(
            query.Search,
            statusIdResult.Value,
            query.StartFrom,
            query.StartTo,
            query.EndFrom,
            query.EndTo,
            query.SortBy,
            query.Descending,
            cancellationToken);

        if (ordersResult.IsFailure)
            return ordersResult.Error;

        var file = exportService.Export(
            ordersResult.Value);

        return file;
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