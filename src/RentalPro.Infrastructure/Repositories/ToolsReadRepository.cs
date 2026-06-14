using CSharpFunctionalExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Repositories;
using RentalPro.Contracts.Tools;
using RentalPro.Domain.Manufacturers;
using RentalPro.Domain.Tools;
using RentalPro.Shared;

namespace RentalPro.Infrastructure.Repositories;

public sealed class ToolsReadRepository(
    ApplicationDbContext dbContext,
    ILogger<ToolsReadRepository> logger)
    : IToolsReadRepository
{
    public async Task<Result<PagedResult<ToolDto>, Errors>> GetPagedAsync(
        string? search,
        ToolCategoryId? categoryId,
        ManufacturerId? manufacturerId,
        ToolStatusId? statusId,
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

            const string whereClause = """
                                       WHERE t.deleted_at IS NULL
                                         AND c.deleted_at IS NULL
                                         AND m.deleted_at IS NULL
                                         AND s.deleted_at IS NULL
                                         AND (
                                             @search IS NULL
                                             OR t.name LIKE @searchPattern
                                             OR t.article_number LIKE @searchPattern
                                             OR t.serial_number LIKE @searchPattern
                                             OR t.inventory_number LIKE @searchPattern
                                         )
                                         AND (@categoryId IS NULL OR t.category_id = @categoryId)
                                         AND (@manufacturerId IS NULL OR t.manufacturer_id = @manufacturerId)
                                         AND (@statusId IS NULL OR t.status_id = @statusId)
                                       """;

            var countSql = $"""
                            SELECT COUNT(*) AS Value
                            FROM tools t
                            INNER JOIN tool_categories c ON c.id = t.category_id
                            INNER JOIN manufacturers m ON m.id = t.manufacturer_id
                            INNER JOIN tool_statuses s ON s.id = t.status_id
                            {whereClause}
                            """;

            var totalCount = await dbContext.Database
                .SqlQueryRaw<int>(
                    countSql,
                    CreateParameter("@search", searchText),
                    CreateParameter("@searchPattern", searchPattern),
                    CreateParameter("@categoryId", categoryId?.Value),
                    CreateParameter("@manufacturerId", manufacturerId?.Value),
                    CreateParameter("@statusId", statusId?.Value))
                .SingleAsync(cancellationToken);

            var sql = $"""
                       SELECT
                           t.id AS Id,
                           t.article_number AS ArticleNumber,
                           t.name AS Name,
                           t.description AS Description,
                           t.category_id AS CategoryId,
                           c.name AS CategoryName,
                           t.manufacturer_id AS ManufacturerId,
                           m.name AS ManufacturerName,
                           t.status_id AS StatusId,
                           s.name AS StatusName,
                           t.rental_price_per_day AS RentalPricePerDay,
                           t.deposit_amount AS DepositAmount,
                           t.serial_number AS SerialNumber,
                           t.inventory_number AS InventoryNumber,
                           t.current_condition AS CurrentCondition,
                           t.photo_path AS PhotoPath,
                           t.created_at AS CreatedAt,
                           t.updated_at AS UpdatedAt
                       FROM tools t
                       INNER JOIN tool_categories c ON c.id = t.category_id
                       INNER JOIN manufacturers m ON m.id = t.manufacturer_id
                       INNER JOIN tool_statuses s ON s.id = t.status_id
                       {whereClause}
                       {orderBy}
                       OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY
                       """;

            var tools = await dbContext.Database
                .SqlQueryRaw<ToolDto>(
                    sql,
                    CreateParameter("@search", searchText),
                    CreateParameter("@searchPattern", searchPattern),
                    CreateParameter("@categoryId", categoryId?.Value),
                    CreateParameter("@manufacturerId", manufacturerId?.Value),
                    CreateParameter("@statusId", statusId?.Value),
                    CreateParameter("@offset", offset),
                    CreateParameter("@pageSize", pageSize))
                .ToListAsync(cancellationToken);

            return new PagedResult<ToolDto>(
                tools,
                page,
                pageSize,
                totalCount);
        }
        catch (OperationCanceledException)
        {
            logger.LogError("Tool page retrieval operation was cancelled");

            return CommonErrors
                .OperationCancelled("get.tools.page.was.cancelled")
                .ToErrors();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get tools page");

            return CommonErrors.Db(
                "get.tools.page.from.db.exception",
                "Failed to get tools page").ToErrors();
        }
    }

    public async Task<Result<ToolStatsDto, Errors>> GetStatsAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var sql = """
                      SELECT
                          COUNT(t.id) AS TotalCount,
                          ISNULL(SUM(CASE WHEN ts.name = N'Доступен' THEN 1 ELSE 0 END), 0) AS AvailableCount,
                          ISNULL(SUM(CASE WHEN ts.name = N'В аренде' THEN 1 ELSE 0 END), 0) AS RentedCount,
                          ISNULL(SUM(CASE WHEN ts.name = N'На ремонте' THEN 1 ELSE 0 END), 0) AS RepairCount
                      FROM tools t
                      LEFT JOIN tool_statuses ts 
                          ON ts.id = t.status_id
                         AND ts.deleted_at IS NULL
                      WHERE t.deleted_at IS NULL
                      """;

            var stats = await dbContext.Database
                .SqlQueryRaw<ToolStatsDto>(sql)
                .SingleAsync(cancellationToken);

            return stats;
        }
        catch (OperationCanceledException)
        {
            return CommonErrors
                .OperationCancelled("get.tools.stats.was.cancelled")
                .ToErrors();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get tools stats");

            return CommonErrors.Db(
                    "get.tools.stats.from.db.exception",
                    "Failed to get tools stats")
                .ToErrors();
        }
    }

    public async Task<Result<IReadOnlyList<ToolDto>, Errors>> GetForExportAsync(
        string? search,
        ToolCategoryId? categoryId,
        ManufacturerId? manufacturerId,
        ToolStatusId? statusId,
        string? sortBy,
        bool descending,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = dbContext.Tools
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Manufacturer)
                .Include(x => x.Status)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var trimmedSearch = search.Trim();

                query = query.Where(x =>
                    x.Name.Value.Contains(trimmedSearch) ||
                    x.ArticleNumber.Value.Contains(trimmedSearch) ||
                    x.SerialNumber.Value.Contains(trimmedSearch) ||
                    x.InventoryNumber.Value.Contains(trimmedSearch));
            }

            if (categoryId is not null)
                query = query.Where(x => x.CategoryId == categoryId);

            if (manufacturerId is not null)
                query = query.Where(x => x.ManufacturerId == manufacturerId);

            if (statusId is not null)
                query = query.Where(x => x.StatusId == statusId);

            query = ApplySorting(query, sortBy, descending);

            var items = await query
                .Select(x => new ToolDto(
                    x.Id.Value,
                    x.ArticleNumber.Value,
                    x.Name.Value,
                    x.Description == null ? null : x.Description.Value,
                    x.CategoryId.Value,
                    x.Category == null ? string.Empty : x.Category.Name.Value,
                    x.ManufacturerId.Value,
                    x.Manufacturer == null ? string.Empty : x.Manufacturer.Name.Value,
                    x.StatusId.Value,
                    x.Status == null ? string.Empty : x.Status.Name.Value,
                    x.RentalPricePerDay.Value,
                    x.DepositAmount.Value,
                    x.SerialNumber.Value,
                    x.InventoryNumber.Value,
                    x.CurrentCondition == null ? null : x.CurrentCondition.Value,
                    x.PhotoPath == null ? null : x.PhotoPath.Value,
                    x.CreatedAt,
                    x.UpdatedAt))
                .ToListAsync(cancellationToken);

            return items;
        }
        catch (OperationCanceledException)
        {
            logger.LogError("Tools export retrieval operation was cancelled");

            return CommonErrors.OperationCancelled(
                    "get.tools.for.export.was.cancelled")
                .ToErrors();
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to retrieve tools for export");

            return CommonErrors.Db(
                    "get.tools.for.export.from.db.exception",
                    "Failed to retrieve tools for export")
                .ToErrors();
        }
    }

    public async Task<Result<List<ToolRentalHistoryItemDto>, Errors>> GetRentalHistoryAsync(
        Guid toolId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            const string sql = """
                               SELECT
                                   o.id AS OrderId,
                                   o.number AS OrderNumber,

                                   c.id AS CustomerId,

                                   CONCAT(
                                       c.last_name,
                                       N' ',
                                       c.first_name,
                                       N' ',
                                       c.middle_name
                                   ) AS CustomerFullName,

                                   oi.start_date AS StartDate,

                                   oi.planned_return_date AS PlannedReturnDate,

                                   oi.actual_returned_date AS ActualReturnedDate,

                                   DATEDIFF(
                                       day,
                                       oi.start_date,
                                       ISNULL(
                                           oi.actual_returned_date,
                                           oi.planned_return_date
                                       )
                                   ) AS RentalDays,

                                   CAST(
                                       oi.rental_price_per_day *
                                       DATEDIFF(
                                           day,
                                           oi.start_date,
                                           ISNULL(
                                               oi.actual_returned_date,
                                               oi.planned_return_date
                                           )
                                       )
                                       AS decimal(18, 2)
                                   ) AS RentalAmount,

                                   os.name AS OrderStatusName

                               FROM order_items oi

                               INNER JOIN orders o
                                   ON o.id = oi.order_id

                               INNER JOIN customers c
                                   ON c.id = o.customer_id

                               INNER JOIN order_statuses os
                                   ON os.id = o.status_id

                               WHERE oi.tool_id = {0}
                                 AND oi.deleted_at IS NULL
                                 AND o.deleted_at IS NULL

                               ORDER BY oi.start_date DESC
                               """;

            var history = await dbContext.Database
                .SqlQueryRaw<ToolRentalHistoryItemDto>(
                    sql,
                    toolId)
                .ToListAsync(cancellationToken);

            return history;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to get rental history for tool {ToolId}",
                toolId);

            return CommonErrors.LoadFailed(
                    "tool.rental.history.load.failed",
                    "Не удалось получить историю аренды инструмента")
                .ToErrors();
        }
    }

    private static IQueryable<Domain.Tools.Tool> ApplySorting(
        IQueryable<Domain.Tools.Tool> query,
        string? sortBy,
        bool descending)
    {
        return sortBy?.ToLowerInvariant() switch
        {
            "name" => descending
                ? query.OrderByDescending(x => x.Name)
                : query.OrderBy(x => x.Name),

            "articlenumber" => descending
                ? query.OrderByDescending(x => x.ArticleNumber)
                : query.OrderBy(x => x.ArticleNumber),

            "serialnumber" => descending
                ? query.OrderByDescending(x => x.SerialNumber)
                : query.OrderBy(x => x.SerialNumber),

            "inventorynumber" => descending
                ? query.OrderByDescending(x => x.InventoryNumber)
                : query.OrderBy(x => x.InventoryNumber),

            "category" => descending
                ? query.OrderByDescending(x => x.Category!.Name)
                : query.OrderBy(x => x.Category!.Name),

            "manufacturer" => descending
                ? query.OrderByDescending(x => x.Manufacturer!.Name)
                : query.OrderBy(x => x.Manufacturer!.Name),

            "status" => descending
                ? query.OrderByDescending(x => x.Status!.Name)
                : query.OrderBy(x => x.Status!.Name),

            "rentalprice" => descending
                ? query.OrderByDescending(x => x.RentalPricePerDay)
                : query.OrderBy(x => x.RentalPricePerDay),

            "deposit" => descending
                ? query.OrderByDescending(x => x.DepositAmount)
                : query.OrderBy(x => x.DepositAmount),

            "createdat" or _ => descending
                ? query.OrderByDescending(x => x.CreatedAt)
                : query.OrderBy(x => x.CreatedAt)
        };
    }

    private static string GetOrderBy(
        string? sortBy,
        bool descending)
    {
        var direction = descending ? "DESC" : "ASC";

        return sortBy?.ToLowerInvariant() switch
        {
            "name" => $"ORDER BY t.name {direction}",
            "articlenumber" => $"ORDER BY t.article_number {direction}",
            "category" => $"ORDER BY c.name {direction}",
            "manufacturer" => $"ORDER BY m.name {direction}",
            "serialnumber" => $"ORDER BY t.serial_number {direction}",
            "inventorynumber" => $"ORDER BY t.inventory_number {direction}",
            "rentalprice" => $"ORDER BY t.rental_price_per_day {direction}",
            "deposit" => $"ORDER BY t.deposit_amount {direction}",
            "status" => $"ORDER BY s.name {direction}",
            "createdat" => $"ORDER BY t.created_at {direction}",
            _ => $"ORDER BY t.created_at {direction}"
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