using CSharpFunctionalExtensions;
using RentalPro.Application.Repositories;
using RentalPro.Application.Services;
using RentalPro.Contracts.Customers;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Customers.ExportCustomersQuery;

public sealed class ExportCustomersQueryHandler(
    ICustomersReadRepository readRepository,
    IExcelExportService<CustomerDto> exportService)
    : IQueryHandler<byte[], ExportCustomersQuery>
{
    public async Task<Result<byte[], Errors>> Handle(
        ExportCustomersQuery query,
        CancellationToken cancellationToken)
    {
        var customersResult = await readRepository.GetForExportAsync(
            query.Search,
            query.HasOrders,
            query.HasActiveOrders,
            query.SortBy,
            query.Descending,
            cancellationToken);

        if (customersResult.IsFailure)
            return customersResult.Error;

        var file = exportService.Export(
            customersResult.Value);

        return file;
    }
}