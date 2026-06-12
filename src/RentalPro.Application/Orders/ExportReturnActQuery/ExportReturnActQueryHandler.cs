using CSharpFunctionalExtensions;
using RentalPro.Application.Repositories;
using RentalPro.Application.Services;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Orders.ExportReturnActQuery;

public sealed class ExportReturnActHandler(
    IOrdersReadRepository ordersReadRepository,
    IReturnActDocumentService returnActDocumentService)
    : IQueryHandler<byte[], ExportReturnActQuery>
{
    public async Task<Result<byte[], Errors>> Handle(
        ExportReturnActQuery query,
        CancellationToken cancellationToken = default)
    {
        var actResult = await ordersReadRepository.GetReturnActDataAsync(
            query.OrderId,
            query.ActDate,
            cancellationToken);

        if (actResult.IsFailure)
            return actResult.Error;

        return await returnActDocumentService.GenerateReturnActAsync(
            actResult.Value,
            cancellationToken);
    }
}