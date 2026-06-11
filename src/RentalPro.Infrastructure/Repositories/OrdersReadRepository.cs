using CSharpFunctionalExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Repositories;
using RentalPro.Contracts.Orders;
using RentalPro.Domain.Orders;
using RentalPro.Shared;

namespace RentalPro.Infrastructure.Repositories;

public sealed class OrdersReadRepository(
    ApplicationDbContext dbContext,
    ILogger<OrdersReadRepository> logger)
    : IOrdersReadRepository
{
    public async Task<Result<PagedResult<OrderDto>, Errors>> GetPagedAsync(
        string? search,
        OrderStatusId? statusId,
        DateOnly? startFrom,
        DateOnly? startTo,
        DateOnly? endFrom,
        DateOnly? endTo,
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

            var fullNameSearchPattern = searchText is null
                ? null
                : $"%{searchText.Replace(" ", "%")}%";

            var orderBy = GetOrderBy(sortBy, descending);
            var offset = (page - 1) * pageSize;

            var countSql = $"""
                            SELECT COUNT(*) AS Value
                            {FromClause}
                            {WhereClause}
                            """;

            var totalCount = await dbContext.Database
                .SqlQueryRaw<int>(
                    countSql,
                    CreateParameter("@search", searchText),
                    CreateParameter("@searchPattern", searchPattern),
                    CreateParameter("@fullNameSearchPattern", fullNameSearchPattern),
                    CreateParameter("@statusId", statusId?.Value),
                    CreateParameter("@startFrom", startFrom),
                    CreateParameter("@startTo", startTo),
                    CreateParameter("@endFrom", endFrom),
                    CreateParameter("@endTo", endTo))
                .SingleAsync(cancellationToken);

            var sql = $"""
                       SELECT
                           o.id AS Id,
                           CONCAT(
                               N'ORD-',
                               FORMAT(o.order_date, 'yyyy'),
                               N'-',
                               RIGHT(CONCAT(N'000', ROW_NUMBER() OVER (ORDER BY o.created_at)), 3)
                           ) AS Number,

                           o.customer_id AS CustomerId,
                           CONCAT_WS(' ', c.last_name, c.first_name, c.middle_name) AS CustomerFullName,

                           o.user_id AS UserId,
                           CONCAT_WS(' ', u.last_name, u.first_name, u.middle_name) AS UserFullName,

                           o.status_id AS StatusId,
                           os.name AS StatusName,

                           ISNULL(oa.ItemsCount, 0) AS ItemsCount,
                           ISNULL(oa.ToolsNames, N'') AS ToolsNames,

                           o.order_date AS OrderDate,
                           oa.StartDate AS StartDate,
                           oa.NearestReturnDate AS PlannedReturnDate,

                           ISNULL(oa.TotalCost, 0) AS TotalCost,
                           ISNULL(oa.DepositTotal, 0) AS DepositTotal,

                           o.comment AS Comment,
                           o.created_at AS CreatedAt,
                           o.updated_at AS UpdatedAt
                       {FromClause}
                       {WhereClause}
                       {orderBy}
                       OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY
                       """;

            var orders = await dbContext.Database
                .SqlQueryRaw<OrderDto>(
                    sql,
                    CreateParameter("@search", searchText),
                    CreateParameter("@searchPattern", searchPattern),
                    CreateParameter("@fullNameSearchPattern", fullNameSearchPattern),
                    CreateParameter("@statusId", statusId?.Value),
                    CreateParameter("@startFrom", startFrom),
                    CreateParameter("@startTo", startTo),
                    CreateParameter("@endFrom", endFrom),
                    CreateParameter("@endTo", endTo),
                    CreateParameter("@offset", offset),
                    CreateParameter("@pageSize", pageSize))
                .ToListAsync(cancellationToken);

            return new PagedResult<OrderDto>(
                orders,
                page,
                pageSize,
                totalCount);
        }
        catch (OperationCanceledException)
        {
            return CommonErrors
                .OperationCancelled("get.orders.page.was.cancelled")
                .ToErrors();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get orders page");

            return CommonErrors.Db(
                    "get.orders.page.from.db.exception",
                    "Failed to get orders page")
                .ToErrors();
        }
    }

    public async Task<Result<OrderStatsDto, Errors>> GetStatsAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var sql = """
                      SELECT
                          COUNT(o.id) AS TotalCount,
                          ISNULL(SUM(CASE WHEN os.name = N'Активен' THEN 1 ELSE 0 END), 0) AS ActiveCount,
                          ISNULL(SUM(CASE WHEN os.name = N'Завершён' THEN 1 ELSE 0 END), 0) AS CompletedCount,
                          ISNULL(SUM(CASE WHEN os.name = N'Просрочен' THEN 1 ELSE 0 END), 0) AS OverdueCount
                      FROM orders o
                      INNER JOIN order_statuses os ON os.id = o.status_id
                      WHERE o.deleted_at IS NULL
                        AND os.deleted_at IS NULL
                      """;

            var stats = await dbContext.Database
                .SqlQueryRaw<OrderStatsDto>(sql)
                .SingleAsync(cancellationToken);

            return stats;
        }
        catch (OperationCanceledException)
        {
            return CommonErrors
                .OperationCancelled("get.orders.stats.was.cancelled")
                .ToErrors();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get orders stats");

            return CommonErrors.Db(
                    "get.orders.stats.from.db.exception",
                    "Failed to get orders stats")
                .ToErrors();
        }
    }

    public async Task<Result<IReadOnlyList<OrderDto>, Errors>> GetForExportAsync(
        string? search,
        OrderStatusId? statusId,
        DateOnly? startFrom,
        DateOnly? startTo,
        DateOnly? endFrom,
        DateOnly? endTo,
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

            var fullNameSearchPattern = searchText is null
                ? null
                : $"%{searchText.Replace(" ", "%")}%";

            var orderBy = GetOrderBy(sortBy, descending);

            var sql = $"""
                       SELECT
                           o.id AS Id,
                           CONCAT(N'ORD-', FORMAT(o.order_date, 'yyyy'), N'-', RIGHT(CONCAT(N'000', ROW_NUMBER() OVER (ORDER BY o.created_at)), 3)) AS Number,
                           o.customer_id AS CustomerId,
                           CONCAT_WS(' ', c.last_name, c.first_name, c.middle_name) AS CustomerName,
                           o.user_id AS UserId,
                           CONCAT_WS(' ', u.last_name, u.first_name, u.middle_name) AS UserName,
                           o.status_id AS StatusId,
                           os.name AS StatusName,
                           o.order_date AS OrderDate,
                           oa.StartDate AS StartDate,
                           oa.PlannedReturnDate AS PlannedReturnDate,
                           oa.ToolNames AS ToolNames,
                           oa.ItemsCount AS ItemsCount,
                           o.total_cost AS TotalCost,
                           o.deposit_total AS DepositTotal,
                           o.comment AS Comment,
                           o.created_at AS CreatedAt,
                           o.updated_at AS UpdatedAt
                       {FromClause}
                       {WhereClause}
                       {orderBy}
                       """;

            var orders = await dbContext.Database
                .SqlQueryRaw<OrderDto>(
                    sql,
                    CreateParameter("@search", searchText),
                    CreateParameter("@searchPattern", searchPattern),
                    CreateParameter("@fullNameSearchPattern", fullNameSearchPattern),
                    CreateParameter("@statusId", statusId?.Value),
                    CreateParameter("@startFrom", startFrom),
                    CreateParameter("@startTo", startTo),
                    CreateParameter("@endFrom", endFrom),
                    CreateParameter("@endTo", endTo))
                .ToListAsync(cancellationToken);

            return orders;
        }
        catch (OperationCanceledException)
        {
            return CommonErrors
                .OperationCancelled("export.orders.was.cancelled")
                .ToErrors();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get orders for export");

            return CommonErrors.Db(
                    "export.orders.from.db.exception",
                    "Failed to get orders for export")
                .ToErrors();
        }
    }

    private const string FromClause = """
                                  FROM orders o
                                  INNER JOIN customers c ON c.id = o.customer_id
                                  INNER JOIN users u ON u.id = o.user_id
                                  INNER JOIN order_statuses os ON os.id = o.status_id
                                  OUTER APPLY
                                  (
                                      SELECT
                                          COUNT(oi.id) AS ItemsCount,
                                          STRING_AGG(t.name, N', ') AS ToolsNames,
                                          MIN(oi.start_date) AS StartDate,
                                          MIN(oi.planned_return_date) AS NearestReturnDate,
                                          SUM(oi.deposit_amount) AS DepositTotal,
                                          SUM(
                                              oi.rental_price_per_day *
                                              DATEDIFF(DAY, oi.start_date, oi.planned_return_date)
                                          ) AS TotalCost
                                      FROM order_items oi
                                      INNER JOIN tools t ON t.id = oi.tool_id
                                      WHERE oi.order_id = o.id
                                        AND oi.deleted_at IS NULL
                                        AND t.deleted_at IS NULL
                                  ) oa
                                  """;

private const string WhereClause = """
                                   WHERE o.deleted_at IS NULL
                                     AND c.deleted_at IS NULL
                                     AND u.deleted_at IS NULL
                                     AND os.deleted_at IS NULL
                                     AND (
                                         @search IS NULL
                                         OR CONVERT(nvarchar(36), o.id) LIKE @searchPattern
                                         OR CONCAT_WS(' ', c.last_name, c.first_name, c.middle_name) LIKE @fullNameSearchPattern
                                         OR CONCAT_WS(' ', u.last_name, u.first_name, u.middle_name) LIKE @fullNameSearchPattern
                                         OR oa.ToolsNames LIKE @searchPattern
                                     )
                                     AND (@statusId IS NULL OR o.status_id = @statusId)
                                     AND (@startFrom IS NULL OR oa.StartDate >= @startFrom)
                                     AND (@startTo IS NULL OR oa.StartDate <= @startTo)
                                     AND (@endFrom IS NULL OR oa.NearestReturnDate >= @endFrom)
                                     AND (@endTo IS NULL OR oa.NearestReturnDate <= @endTo)
                                   """;

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
            "number" => $"ORDER BY o.created_at {direction}",
            "customer" => $"ORDER BY c.last_name {direction}, c.first_name {direction}, c.middle_name {direction}",
            "user" => $"ORDER BY u.last_name {direction}, u.first_name {direction}, u.middle_name {direction}",
            "status" => $"ORDER BY os.name {direction}",
            "orderdate" => $"ORDER BY o.order_date {direction}",
            "createdat" => $"ORDER BY o.created_at {direction}",
            "updatedat" => $"ORDER BY o.updated_at {direction}",
            "items" => $"ORDER BY oa.ItemsCount {direction}",
            "itemscount" => $"ORDER BY oa.ItemsCount {direction}",
            "deposit" => $"ORDER BY oa.DepositTotal {direction}",
            "deposittotal" => $"ORDER BY oa.DepositTotal {direction}",
            "amount" => $"ORDER BY oa.TotalCost {direction}",
            "totalcost" => $"ORDER BY oa.TotalCost {direction}",
            "start" => $"ORDER BY oa.StartDate {direction}",
            "startdate" => $"ORDER BY oa.StartDate {direction}",
            "end" => $"ORDER BY oa.NearestReturnDate {direction}",
            "plannedreturndate" => $"ORDER BY oa.NearestReturnDate {direction}",
            "tool" => $"ORDER BY oa.ToolsNames {direction}",
            "tools" => $"ORDER BY oa.ToolsNames {direction}",
            _ => "ORDER BY o.created_at DESC"
        };
    }
}