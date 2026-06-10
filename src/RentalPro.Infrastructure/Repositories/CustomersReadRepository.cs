using CSharpFunctionalExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Repositories;
using RentalPro.Contracts.Customers;
using RentalPro.Shared;

namespace RentalPro.Infrastructure.Repositories;

public sealed class CustomersReadRepository(
    ApplicationDbContext dbContext,
    ILogger<CustomersReadRepository> logger)
    : ICustomersReadRepository
{
    public async Task<Result<PagedResult<CustomerDto>, Errors>> GetPagedAsync(
        string? search,
        bool? hasOrders,
        bool? hasDebt,
        string? sortBy,
        bool descending,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        try
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize is < 1 or > 100 ? 10 : pageSize;

            var searchText = string.IsNullOrWhiteSpace(search)
                ? null
                : search.Trim();

            var searchPattern = searchText is null
                ? null
                : $"%{searchText}%";

            var orderBy = GetOrderBy(sortBy, descending);
            var offset = (page - 1) * pageSize;

            var totalCount = await GetTotalCountAsync(
                searchText,
                searchPattern,
                hasOrders,
                hasDebt,
                cancellationToken);

            var sql = $"""
                       SELECT
                           c.id AS Id,
                           c.last_name AS LastName,
                           c.first_name AS FirstName,
                           c.middle_name AS MiddleName,
                           c.phone_number AS PhoneNumber,
                           c.email AS Email,
                           c.passport_series AS PassportSeries,
                           c.passport_number AS PassportNumber,
                           c.postal_code AS PostalCode,
                           c.region AS Region,
                           c.city AS City,
                           c.street AS Street,
                           c.house AS House,
                           c.building AS Building,
                           c.apartment AS Apartment,
                           oc.OrdersCount AS OrdersCount,
                           dc.HasDebt AS HasDebt,
                           c.created_at AS CreatedAt,
                           c.updated_at AS UpdatedAt
                       {FromClause}
                       {WhereClause}
                       {orderBy}
                       OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY
                       """;

            var customers = await dbContext.Database
                .SqlQueryRaw<CustomerDto>(
                    sql,
                    CreateParameter("@search", searchText),
                    CreateParameter("@searchPattern", searchPattern),
                    CreateParameter("@hasOrders", hasOrders),
                    CreateParameter("@hasDebt", hasDebt),
                    CreateParameter("@offset", offset),
                    CreateParameter("@pageSize", pageSize))
                .ToListAsync(cancellationToken);

            return new PagedResult<CustomerDto>(
                customers,
                page,
                pageSize,
                totalCount);
        }
        catch (OperationCanceledException)
        {
            logger.LogError("Customer page retrieval operation was cancelled");

            return CommonErrors
                .OperationCancelled("get.customers.page.was.cancelled")
                .ToErrors();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get customers page");

            return CommonErrors.Db(
                    "get.customers.page.from.db.exception",
                    "Failed to get customers page")
                .ToErrors();
        }
    }

    public async Task<Result<CustomerStatsDto, Errors>> GetStatsAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var sql = $"""
                       SELECT
                           COUNT(c.id) AS TotalCount,
                           ISNULL(SUM(CASE WHEN oc.OrdersCount > 0 THEN 1 ELSE 0 END), 0) AS WithOrdersCount,
                           ISNULL(SUM(CASE WHEN dc.HasDebt = 1 THEN 1 ELSE 0 END), 0) AS WithDebtCount
                       {FromClause}
                       WHERE c.deleted_at IS NULL
                       """;

            var stats = await dbContext.Database
                .SqlQueryRaw<CustomerStatsDto>(sql)
                .SingleAsync(cancellationToken);

            return stats;
        }
        catch (OperationCanceledException)
        {
            logger.LogError("Customer stats retrieval operation was cancelled");

            return CommonErrors
                .OperationCancelled("get.customers.stats.was.cancelled")
                .ToErrors();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get customers stats");

            return CommonErrors.Db(
                    "get.customers.stats.from.db.exception",
                    "Failed to get customers stats")
                .ToErrors();
        }
    }

    public async Task<Result<IReadOnlyList<CustomerDto>, Errors>> GetForExportAsync(
        string? search,
        bool? hasOrders,
        bool? hasDebt,
        string? sortBy,
        bool descending,
        CancellationToken cancellationToken)
    {
        try
        {
            var searchText = string.IsNullOrWhiteSpace(search)
                ? null
                : search.Trim();

            var searchPattern = searchText is null
                ? null
                : $"%{searchText}%";

            var orderBy = GetOrderBy(sortBy, descending);

            var sql = $"""
                       SELECT
                           c.id AS Id,
                           c.last_name AS LastName,
                           c.first_name AS FirstName,
                           c.middle_name AS MiddleName,
                           c.phone_number AS PhoneNumber,
                           c.email AS Email,
                           c.passport_series AS PassportSeries,
                           c.passport_number AS PassportNumber,
                           c.postal_code AS PostalCode,
                           c.region AS Region,
                           c.city AS City,
                           c.street AS Street,
                           c.house AS House,
                           c.building AS Building,
                           c.apartment AS Apartment,
                           oc.OrdersCount AS OrdersCount,
                           dc.HasDebt AS HasDebt,
                           c.created_at AS CreatedAt,
                           c.updated_at AS UpdatedAt
                       {FromClause}
                       {WhereClause}
                       {orderBy}
                       """;

            var customers = await dbContext.Database
                .SqlQueryRaw<CustomerDto>(
                    sql,
                    CreateParameter("@search", searchText),
                    CreateParameter("@searchPattern", searchPattern),
                    CreateParameter("@hasOrders", hasOrders),
                    CreateParameter("@hasDebt", hasDebt))
                .ToListAsync(cancellationToken);

            return customers;
        }
        catch (OperationCanceledException)
        {
            logger.LogError("Customers export retrieval operation was cancelled");

            return CommonErrors
                .OperationCancelled("get.customers.for.export.was.cancelled")
                .ToErrors();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve customers for export");

            return CommonErrors.Db(
                    "get.customers.for.export.from.db.exception",
                    "Failed to retrieve customers for export")
                .ToErrors();
        }
    }

    private async Task<int> GetTotalCountAsync(
        string? searchText,
        string? searchPattern,
        bool? hasOrders,
        bool? hasDebt,
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
                CreateParameter("@search", searchText),
                CreateParameter("@searchPattern", searchPattern),
                CreateParameter("@hasOrders", hasOrders),
                CreateParameter("@hasDebt", hasDebt))
            .SingleAsync(cancellationToken);
    }

    private const string FromClause = """
                                      FROM customers c
                                      OUTER APPLY
                                      (
                                          SELECT COUNT(*) AS OrdersCount
                                          FROM orders o
                                          WHERE o.customer_id = c.id
                                            AND o.deleted_at IS NULL
                                      ) oc
                                      OUTER APPLY
                                      (
                                          SELECT
                                              CASE
                                                  WHEN ISNULL(SUM(o.total_cost + o.deposit_total), 0)
                                                     > ISNULL(SUM(paid.PaidAmount), 0)
                                                  THEN CAST(1 AS bit)
                                                  ELSE CAST(0 AS bit)
                                              END AS HasDebt
                                          FROM orders o
                                          OUTER APPLY
                                          (
                                              SELECT SUM(p.amount) AS PaidAmount
                                              FROM payments p
                                              WHERE p.order_id = o.id
                                                AND p.deleted_at IS NULL
                                          ) paid
                                          WHERE o.customer_id = c.id
                                            AND o.deleted_at IS NULL
                                      ) dc
                                      """;

    private const string WhereClause = """
                                       WHERE c.deleted_at IS NULL
                                         AND (
                                             @search IS NULL
                                             OR c.last_name LIKE @searchPattern
                                             OR c.first_name LIKE @searchPattern
                                             OR c.middle_name LIKE @searchPattern
                                             OR c.phone_number LIKE @searchPattern
                                             OR c.email LIKE @searchPattern
                                             OR c.passport_series LIKE @searchPattern
                                             OR c.passport_number LIKE @searchPattern
                                             OR c.postal_code LIKE @searchPattern
                                             OR c.region LIKE @searchPattern
                                             OR c.city LIKE @searchPattern
                                             OR c.street LIKE @searchPattern
                                             OR c.house LIKE @searchPattern
                                             OR c.building LIKE @searchPattern
                                             OR c.apartment LIKE @searchPattern
                                         )
                                         AND (
                                             @hasOrders IS NULL
                                             OR (@hasOrders = 1 AND oc.OrdersCount > 0)
                                             OR (@hasOrders = 0 AND oc.OrdersCount = 0)
                                         )
                                         AND (
                                             @hasDebt IS NULL
                                             OR dc.HasDebt = @hasDebt
                                         )
                                       """;

    private static string GetOrderBy(
        string? sortBy,
        bool descending)
    {
        var direction = descending ? "DESC" : "ASC";

        return sortBy?.ToLowerInvariant() switch
        {
            "fullname" => $"ORDER BY c.last_name {direction}, c.first_name {direction}, c.middle_name {direction}",
            "lastname" => $"ORDER BY c.last_name {direction}",
            "firstname" => $"ORDER BY c.first_name {direction}",
            "middlename" => $"ORDER BY c.middle_name {direction}",
            "phone" => $"ORDER BY c.phone_number {direction}",
            "phonenumber" => $"ORDER BY c.phone_number {direction}",
            "email" => $"ORDER BY c.email {direction}",
            "passport" => $"ORDER BY c.passport_series {direction}, c.passport_number {direction}",
            "orders" => $"ORDER BY oc.OrdersCount {direction}",
            "orderscount" => $"ORDER BY oc.OrdersCount {direction}",
            "hasdebt" => $"ORDER BY dc.HasDebt {direction}",
            "debt" => $"ORDER BY dc.HasDebt {direction}",
            "createdat" => $"ORDER BY c.created_at {direction}",
            "updatedat" => $"ORDER BY c.updated_at {direction}",
            _ => $"ORDER BY c.created_at {direction}"
        };
    }

    private static SqlParameter CreateParameter(
        string name,
        object? value)
    {
        return new SqlParameter(
            name,
            value ?? DBNull.Value);
    }
}