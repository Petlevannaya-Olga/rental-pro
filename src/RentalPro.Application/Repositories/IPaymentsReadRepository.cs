using CSharpFunctionalExtensions;
using RentalPro.Contracts.Payments;
using RentalPro.Domain.Payments;
using RentalPro.Shared;

namespace RentalPro.Application.Repositories;

public interface IPaymentsReadRepository
{
    Task<Result<PagedResult<PaymentDto>, Errors>> GetPagedAsync(
        string? search,
        PaymentTypeId? paymentTypeId,
        PaymentMethodId? paymentMethodId,
        DateTime? dateFrom,
        DateTime? dateTo,
        string? sortBy,
        bool descending,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<Result<PaymentStatsDto, Errors>> GetStatsAsync(
        CancellationToken cancellationToken);
    
    Task<Result<IReadOnlyList<PaymentDto>, Errors>> GetForExportAsync(
        string? search,
        PaymentTypeId? paymentTypeId,
        PaymentMethodId? paymentMethodId,
        DateTime? dateFrom,
        DateTime? dateTo,
        string? sortBy,
        bool descending,
        CancellationToken cancellationToken);
}