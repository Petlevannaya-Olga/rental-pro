using CSharpFunctionalExtensions;
using RentalPro.Application.Repositories;
using RentalPro.Application.Services;
using RentalPro.Contracts.Payments;
using RentalPro.Domain.Payments;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Payments.ExportPaymentsQuery;

public sealed class ExportPaymentsQueryHandler(
    IPaymentsReadRepository paymentsReadRepository,
    IExcelExportService<PaymentDto> exportService)
    : IQueryHandler<byte[], ExportPaymentsQuery>
{
    public async Task<Result<byte[], Errors>> Handle(
        ExportPaymentsQuery query,
        CancellationToken cancellationToken = default)
    {
        PaymentTypeId? paymentTypeId = query.PaymentTypeId.HasValue
            ? PaymentTypeId.Restore(query.PaymentTypeId.Value)
            : null;

        PaymentMethodId? paymentMethodId = query.PaymentMethodId.HasValue
            ? PaymentMethodId.Restore(query.PaymentMethodId.Value)
            : null;

        var paymentsResult = await paymentsReadRepository.GetForExportAsync(
            query.Search,
            paymentTypeId,
            paymentMethodId,
            query.DateFrom,
            query.DateTo,
            query.SortBy,
            query.Descending,
            cancellationToken);

        if (paymentsResult.IsFailure)
            return paymentsResult.Error;

        var fileBytes = exportService.Export(paymentsResult.Value);

        return fileBytes;
    }
}