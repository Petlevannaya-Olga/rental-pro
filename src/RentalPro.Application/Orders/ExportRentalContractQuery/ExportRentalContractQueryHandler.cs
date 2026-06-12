using CSharpFunctionalExtensions;
using RentalPro.Application.Repositories;
using RentalPro.Application.Services;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Orders.ExportRentalContractQuery;

public sealed class ExportRentalContractHandler(
    IOrdersReadRepository ordersReadRepository,
    IContractDocumentService contractDocumentService)
    : IQueryHandler<byte[], ExportRentalContractQuery>
{
    public async Task<Result<byte[], Errors>> Handle(
        ExportRentalContractQuery query,
        CancellationToken cancellationToken = default)
    {
        var contractResult = await ordersReadRepository.GetContractDataAsync(
            query.OrderId,
            cancellationToken);

        if (contractResult.IsFailure)
            return contractResult.Error;

        return await contractDocumentService.GenerateRentalContractAsync(
            contractResult.Value,
            cancellationToken);
    }
}