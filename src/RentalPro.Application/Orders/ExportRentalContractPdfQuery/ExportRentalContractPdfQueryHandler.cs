using CSharpFunctionalExtensions;
using RentalPro.Application.Repositories;
using RentalPro.Application.Services;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Orders.ExportRentalContractPdfQuery;

public sealed class ExportRentalContractPdfHandler(
    IOrdersReadRepository ordersReadRepository,
    IContractPdfService contractPdfService)
    : IQueryHandler<byte[], ExportRentalContractPdfQuery>
{
    public async Task<Result<byte[], Errors>> Handle(
        ExportRentalContractPdfQuery query,
        CancellationToken cancellationToken = default)
    {
        var contractResult = await ordersReadRepository.GetContractDataAsync(
            query.OrderId,
            cancellationToken);

        if (contractResult.IsFailure)
            return contractResult.Error;

        return await contractPdfService.GenerateRentalContractPdfAsync(
            contractResult.Value,
            cancellationToken);
    }
}