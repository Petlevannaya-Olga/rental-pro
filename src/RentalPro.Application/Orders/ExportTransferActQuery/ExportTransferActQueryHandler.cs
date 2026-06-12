using CSharpFunctionalExtensions;
using RentalPro.Application.Repositories;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Orders.ExportTransferActQuery;

public sealed class ExportTransferActHandler(
    IOrdersReadRepository ordersReadRepository,
    ITransferActDocumentService transferActDocumentService)
    : IQueryHandler<byte[], ExportTransferActQuery>
{
    public async Task<Result<byte[], Errors>> Handle(
        ExportTransferActQuery query,
        CancellationToken cancellationToken = default)
    {
        var actResult = await ordersReadRepository.GetTransferActDataAsync(
            query.OrderId,
            query.ActDate,
            cancellationToken);

        if (actResult.IsFailure)
            return actResult.Error;

        return await transferActDocumentService.GenerateTransferActAsync(
            actResult.Value,
            cancellationToken);
    }
}