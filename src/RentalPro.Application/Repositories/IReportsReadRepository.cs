using CSharpFunctionalExtensions;
using RentalPro.Contracts.Reports;
using RentalPro.Shared;

namespace RentalPro.Application.Repositories;

public interface IReportsReadRepository
{
    Task<Result<IReadOnlyList<RevenueReportDto>, Errors>> GetRevenueAsync(
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<PopularToolReportDto>, Errors>> GetPopularToolsAsync(
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<OverdueReturnReportDto>, Errors>> GetOverdueReturnsAsync(
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<ToolReportDto>, Errors>> GetToolsAsync(CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<CustomerReportDto>, Errors>> GetCustomersAsync(
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<PaymentReportDto>, Errors>> GetPaymentsAsync(
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken cancellationToken);
}