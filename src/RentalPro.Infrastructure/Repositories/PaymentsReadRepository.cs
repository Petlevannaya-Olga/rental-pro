using CSharpFunctionalExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Repositories;
using RentalPro.Contracts.Payments;
using RentalPro.Domain.Payments;
using RentalPro.Shared;

namespace RentalPro.Infrastructure.Repositories;

public sealed class PaymentsReadRepository(
    ApplicationDbContext dbContext,
    ILogger<PaymentsReadRepository> logger)
    : IPaymentsReadRepository
{
    public async Task<Result<PagedResult<PaymentDto>, Errors>> GetPagedAsync(
        string? search,
        PaymentTypeId? paymentTypeId,
        PaymentMethodId? paymentMethodId,
        DateTime? dateFrom,
        DateTime? dateTo,
        string? sortBy,
        bool descending,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        try
        {
            page = NormalizePage(page);
            pageSize = NormalizePageSize(pageSize);

            var searchData = CreateSearchData(search);
            var orderBy = GetOrderBy(sortBy, descending);
            var offset = (page - 1) * pageSize;

            var totalCount = await GetPaymentsCountAsync(
                searchData,
                paymentTypeId,
                paymentMethodId,
                dateFrom,
                dateTo,
                cancellationToken);

            var sql = $"""
                       SELECT
                           p.id AS Id,
                           p.order_id AS OrderId,
                           o.number AS OrderNumber,

                           o.customer_id AS CustomerId,
                           CONCAT_WS(' ', c.last_name, c.first_name, c.middle_name) AS CustomerFullName,

                           p.payment_type_id AS PaymentTypeId,
                           pt.name AS PaymentTypeName,

                           p.payment_method_id AS PaymentMethodId,
                           pm.name AS PaymentMethodName,

                           p.amount AS Amount,
                           p.payment_date AS PaymentDate,
                           o.order_date AS OrderDate,
                           p.comment AS Comment

                       {FromClause}
                       {WhereClause}
                       {orderBy}
                       OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY
                       """;

            var payments = await dbContext.Database
                .SqlQueryRaw<PaymentDto>(
                    sql,
                    CreateFilterParameters(
                        searchData,
                        paymentTypeId,
                        paymentMethodId,
                        dateFrom,
                        dateTo,
                        CreateParameter("@offset", offset),
                        CreateParameter("@pageSize", pageSize)))
                .ToListAsync(cancellationToken);

            return new PagedResult<PaymentDto>(
                payments,
                page,
                pageSize,
                totalCount);
        }
        catch (OperationCanceledException)
        {
            return CommonErrors
                .OperationCancelled("get.payments.page.was.cancelled")
                .ToErrors();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get payments page");

            return CommonErrors.Db(
                    "get.payments.page.from.db.exception",
                    "Failed to get payments page")
                .ToErrors();
        }
    }

    private async Task<int> GetPaymentsCountAsync(
        SearchData searchData,
        PaymentTypeId? paymentTypeId,
        PaymentMethodId? paymentMethodId,
        DateTime? dateFrom,
        DateTime? dateTo,
        CancellationToken cancellationToken)
    {
        var countSql = $"""
                        SELECT COUNT(*) AS Value
                        {FromClause}
                        {WhereClause}
                        """;

        return await dbContext.Database
            .SqlQueryRaw<int>(
                countSql,
                CreateFilterParameters(
                    searchData,
                    paymentTypeId,
                    paymentMethodId,
                    dateFrom,
                    dateTo))
            .SingleAsync(cancellationToken);
    }

    public async Task<Result<PaymentStatsDto, Errors>> GetStatsAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var sql = """
                      SELECT
                          COUNT(p.id) AS TotalCount,

                          ISNULL(SUM(CASE WHEN pt.name = N'Аренда' THEN 1 ELSE 0 END), 0) AS RentalCount,

                          ISNULL(SUM(CASE WHEN pt.name = N'Залог' THEN 1 ELSE 0 END), 0) AS DepositCount,

                          ISNULL(SUM(CASE WHEN pt.name = N'Возврат залога' THEN 1 ELSE 0 END), 0) AS DepositRefundCount,

                          ISNULL(
                              SUM(
                                  CASE
                                      WHEN pt.name = N'Аренда'
                                          THEN p.amount
                                      WHEN pt.name = N'Возврат аренды'
                                          THEN -p.amount
                                      ELSE 0
                                  END
                              ),
                              0
                          ) AS TotalAmount

                      FROM payments p
                      INNER JOIN payment_types pt ON pt.id = p.payment_type_id

                      WHERE p.deleted_at IS NULL
                        AND pt.deleted_at IS NULL
                      """;

            var stats = await dbContext.Database
                .SqlQueryRaw<PaymentStatsDto>(sql)
                .SingleAsync(cancellationToken);

            return stats;
        }
        catch (OperationCanceledException)
        {
            return CommonErrors
                .OperationCancelled("get.payments.stats.was.cancelled")
                .ToErrors();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get payments stats");

            return CommonErrors.Db(
                    "get.payments.stats.from.db.exception",
                    "Failed to get payments stats")
                .ToErrors();
        }
    }
    
    public async Task<Result<IReadOnlyList<PaymentDto>, Errors>> GetForExportAsync(
    string? search,
    PaymentTypeId? paymentTypeId,
    PaymentMethodId? paymentMethodId,
    DateTime? dateFrom,
    DateTime? dateTo,
    string? sortBy,
    bool descending,
    CancellationToken cancellationToken)
{
    try
    {
        var searchData = CreateSearchData(search);
        var orderBy = GetOrderBy(sortBy, descending);

        var sql = $"""
                   SELECT
                       p.id AS Id,
                       p.order_id AS OrderId,
                       o.number AS OrderNumber,

                       o.customer_id AS CustomerId,
                       CONCAT_WS(' ', c.last_name, c.first_name, c.middle_name) AS CustomerFullName,

                       p.payment_type_id AS PaymentTypeId,
                       pt.name AS PaymentTypeName,

                       p.payment_method_id AS PaymentMethodId,
                       pm.name AS PaymentMethodName,

                       p.amount AS Amount,
                       o.order_date AS OrderDate,
                       p.payment_date AS PaymentDate,
                       p.comment AS Comment

                   {FromClause}
                   {WhereClause}
                   {orderBy}
                   """;

        var payments = await dbContext.Database
            .SqlQueryRaw<PaymentDto>(
                sql,
                CreateFilterParameters(
                    searchData,
                    paymentTypeId,
                    paymentMethodId,
                    dateFrom,
                    dateTo))
            .ToListAsync(cancellationToken);

        return payments;
    }
    catch (OperationCanceledException)
    {
        return CommonErrors
            .OperationCancelled("export.payments.was.cancelled")
            .ToErrors();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to get payments for export");

        return CommonErrors.Db(
                "export.payments.from.db.exception",
                "Failed to get payments for export")
            .ToErrors();
    }
}
    
    private static SearchData CreateSearchData(string? search)
    {
        var searchText = string.IsNullOrWhiteSpace(search)
            ? null
            : search.Trim();

        return new SearchData(
            SearchText: searchText,
            SearchPattern: searchText is null
                ? null
                : $"%{searchText}%",
            FullNameSearchPattern: searchText is null
                ? null
                : $"%{searchText.Replace(" ", "%")}%");
    }

    private static SqlParameter[] CreateFilterParameters(
        SearchData searchData,
        PaymentTypeId? paymentTypeId,
        PaymentMethodId? paymentMethodId,
        DateTime? dateFrom,
        DateTime? dateTo,
        params SqlParameter[] additionalParameters)
    {
        return
        [
            CreateParameter("@search", searchData.SearchText),
            CreateParameter("@searchPattern", searchData.SearchPattern),
            CreateParameter("@fullNameSearchPattern", searchData.FullNameSearchPattern),
            CreateParameter("@paymentTypeId", paymentTypeId?.Value),
            CreateParameter("@paymentMethodId", paymentMethodId?.Value),
            CreateParameter("@dateFrom", dateFrom),
            CreateParameter("@dateTo", dateTo),
            ..additionalParameters
        ];
    }

    private static int NormalizePage(int page)
    {
        return page < 1 ? 1 : page;
    }

    private static int NormalizePageSize(int pageSize)
    {
        return pageSize is < 1 or > 100
            ? 10
            : pageSize;
    }

    private static SqlParameter CreateParameter(
        string name,
        object? value)
    {
        return new SqlParameter(
            name,
            value ?? DBNull.Value);
    }

    private static string GetOrderBy(
        string? sortBy,
        bool descending)
    {
        var direction = descending ? "DESC" : "ASC";

        return sortBy?.ToLowerInvariant() switch
        {
            "ordernumber" => $"ORDER BY o.number {direction}",
            "order" => $"ORDER BY o.number {direction}",
            "customer" => $"ORDER BY c.last_name {direction}, c.first_name {direction}, c.middle_name {direction}",
            "type" => $"ORDER BY pt.name {direction}",
            "paymenttype" => $"ORDER BY pt.name {direction}",
            "method" => $"ORDER BY pm.name {direction}",
            "paymentmethod" => $"ORDER BY pm.name {direction}",
            "amount" => $"ORDER BY p.amount {direction}",
            "date" => $"ORDER BY p.payment_date {direction}",
            "paymentdate" => $"ORDER BY p.payment_date {direction}",
            _ => "ORDER BY p.payment_date DESC"
        };
    }

    private sealed record SearchData(
        string? SearchText,
        string? SearchPattern,
        string? FullNameSearchPattern);

    private const string FromClause = """
                                      FROM payments p
                                      INNER JOIN orders o ON o.id = p.order_id
                                      INNER JOIN customers c ON c.id = o.customer_id
                                      INNER JOIN payment_types pt ON pt.id = p.payment_type_id
                                      INNER JOIN payment_methods pm ON pm.id = p.payment_method_id
                                      """;

    private const string WhereClause = """
                                       WHERE p.deleted_at IS NULL
                                         AND o.deleted_at IS NULL
                                         AND c.deleted_at IS NULL
                                         AND pt.deleted_at IS NULL
                                         AND pm.deleted_at IS NULL
                                         AND (
                                             @search IS NULL
                                             OR o.number LIKE @searchPattern
                                             OR CONCAT_WS(' ', c.last_name, c.first_name, c.middle_name) LIKE @fullNameSearchPattern
                                             OR pt.name LIKE @searchPattern
                                             OR pm.name LIKE @searchPattern
                                             OR p.comment LIKE @searchPattern
                                         )
                                         AND (@paymentTypeId IS NULL OR p.payment_type_id = @paymentTypeId)
                                         AND (@paymentMethodId IS NULL OR p.payment_method_id = @paymentMethodId)
                                         AND (@dateFrom IS NULL OR p.payment_date >= @dateFrom)
                                         AND (@dateTo IS NULL OR p.payment_date <= @dateTo)
                                       """;
}
