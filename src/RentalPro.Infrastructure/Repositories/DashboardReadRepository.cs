using CSharpFunctionalExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Repositories;
using RentalPro.Contracts.Dashboard;
using RentalPro.Shared;

namespace RentalPro.Infrastructure.Repositories;

public sealed class DashboardReadRepository(
    ApplicationDbContext dbContext,
    ILogger<DashboardReadRepository> logger)
    : IDashboardReadRepository
{
    private const string AvailableStatus = "Доступен";
    private const string RentedStatus = "В аренде";
    private const string RepairStatus = "На ремонте";
    private const string BookedStatus = "Забронирован";

    private const string RentPaymentType = "Аренда";

    private const string CompletedOrderStatus = "Завершен";
    private const string WaitingDepositRefundStatus = "Ожидает возврата залога";

    public async Task<Result<DashboardDto, Errors>> GetAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = await GetStatsAsync(cancellationToken);
            var recentOrders = await GetRecentOrdersAsync(cancellationToken);
            var toolStatuses = await GetToolStatusesAsync(cancellationToken);
            var upcomingReturns = await GetUpcomingReturnsAsync(cancellationToken);
            var overdueReturns = await GetOverdueReturnsAsync(cancellationToken);

            return new DashboardDto(
                stats,
                recentOrders,
                toolStatuses,
                upcomingReturns,
                overdueReturns);
        }
        catch (OperationCanceledException)
        {
            logger.LogError("Dashboard retrieval operation was cancelled");

            return CommonErrors
                .OperationCancelled("get.dashboard.was.cancelled")
                .ToErrors();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get dashboard data");

            return CommonErrors.Db(
                    "get.dashboard.from.db.exception",
                    "Failed to get dashboard data")
                .ToErrors();
        }
    }

    private async Task<DashboardStatsDto> GetStatsAsync(
        CancellationToken cancellationToken)
    {
        var now = DateTime.Now;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        const string sql = """
                           SELECT
                               ISNULL((SELECT COUNT(*) FROM tools WHERE deleted_at IS NULL), 0) AS TotalTools,

                               ISNULL((
                                   SELECT COUNT(*)
                                   FROM tools t
                                   INNER JOIN tool_statuses ts ON ts.id = t.status_id
                                   WHERE t.deleted_at IS NULL
                                     AND ts.name = @rentedStatus
                               ), 0) AS RentedTools,

                               ISNULL((
                                   SELECT COUNT(*)
                                   FROM tools t
                                   INNER JOIN tool_statuses ts ON ts.id = t.status_id
                                   WHERE t.deleted_at IS NULL
                                     AND ts.name = @bookedStatus
                               ), 0) AS BookedTools,

                               ISNULL((
                                   SELECT COUNT(*)
                                   FROM tools t
                                   INNER JOIN tool_statuses ts ON ts.id = t.status_id
                                   WHERE t.deleted_at IS NULL
                                     AND ts.name = @repairStatus
                               ), 0) AS ToolsInRepair,

                               ISNULL((SELECT COUNT(*) FROM customers WHERE deleted_at IS NULL), 0) AS TotalCustomers,

                               ISNULL((
                                   SELECT SUM(p.amount)
                                   FROM payments p
                                   INNER JOIN payment_types pt ON pt.id = p.payment_type_id
                                   WHERE p.deleted_at IS NULL
                                     AND pt.name = @rentPaymentType
                                     AND p.payment_date >= @startOfMonth
                               ), 0) AS MonthlyRevenue
                           """;

        return await dbContext.Database
            .SqlQueryRaw<DashboardStatsDto>(
                sql,
                CreateParameter("@rentedStatus", RentedStatus),
                CreateParameter("@bookedStatus", BookedStatus),
                CreateParameter("@repairStatus", RepairStatus),
                CreateParameter("@rentPaymentType", RentPaymentType),
                CreateParameter("@startOfMonth", startOfMonth))
            .SingleAsync(cancellationToken);
    }

    private async Task<DashboardToolStatusesDto> GetToolStatusesAsync(
        CancellationToken cancellationToken)
    {
        const string sql = """
                           SELECT
                               ISNULL(SUM(CASE WHEN ts.name = @availableStatus THEN 1 ELSE 0 END), 0) AS Available,
                               ISNULL(SUM(CASE WHEN ts.name = @rentedStatus THEN 1 ELSE 0 END), 0) AS Rented,
                               ISNULL(SUM(CASE WHEN ts.name = @bookedStatus THEN 1 ELSE 0 END), 0) AS Booked,
                               ISNULL(SUM(CASE WHEN ts.name = @repairStatus THEN 1 ELSE 0 END), 0) AS InRepair
                           FROM tools t
                           INNER JOIN tool_statuses ts ON ts.id = t.status_id
                           WHERE t.deleted_at IS NULL
                           """;

        return await dbContext.Database
            .SqlQueryRaw<DashboardToolStatusesDto>(
                sql,
                CreateParameter("@availableStatus", AvailableStatus),
                CreateParameter("@rentedStatus", RentedStatus),
                CreateParameter("@bookedStatus", BookedStatus),
                CreateParameter("@repairStatus", RepairStatus))
            .SingleAsync(cancellationToken);
    }

    private async Task<List<DashboardRecentOrderDto>> GetRecentOrdersAsync(
        CancellationToken cancellationToken)
    {
        const string sql = """
                           SELECT TOP 5
                               o.id AS Id,
                               o.number AS Number,
                               CONCAT(c.last_name, N' ', c.first_name, N' ', c.middle_name) AS CustomerFullName,
                               COUNT(oi.id) AS ToolsCount,
                               MIN(oi.start_date) AS StartDate,
                               MAX(oi.planned_return_date) AS PlannedReturnDate,
                               os.name AS StatusName,
                               CAST(ISNULL(SUM(
                                   oi.rental_price_per_day * DATEDIFF(day, oi.start_date, oi.planned_return_date)
                                   + oi.deposit_amount
                               ), 0) AS decimal(18, 2)) AS TotalCost
                           FROM orders o
                           INNER JOIN customers c ON c.id = o.customer_id
                           INNER JOIN order_statuses os ON os.id = o.status_id
                           LEFT JOIN order_items oi
                               ON oi.order_id = o.id
                              AND oi.deleted_at IS NULL
                           WHERE o.deleted_at IS NULL
                           GROUP BY
                               o.id,
                               o.number,
                               c.last_name,
                               c.first_name,
                               c.middle_name,
                               os.name,
                               o.created_at
                           ORDER BY o.created_at DESC
                           """;

        return await dbContext.Database
            .SqlQueryRaw<DashboardRecentOrderDto>(sql)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<DashboardReturnDto>> GetUpcomingReturnsAsync(
        CancellationToken cancellationToken)
    {
        var today = DateTime.Today;
        var maxDate = today.AddDays(3);

        const string sql = """
                           SELECT TOP 5
                               o.id AS OrderId,
                               CONCAT(c.last_name, N' ', c.first_name, N' ', c.middle_name) AS CustomerFullName,
                               STRING_AGG(t.name, N', ') AS ToolsNames,
                               MAX(oi.planned_return_date) AS PlannedReturnDate,
                               DATEDIFF(day, @today, MAX(oi.planned_return_date)) AS Days
                           FROM orders o
                           INNER JOIN customers c ON c.id = o.customer_id
                           INNER JOIN order_statuses os ON os.id = o.status_id
                           INNER JOIN order_items oi
                               ON oi.order_id = o.id
                              AND oi.deleted_at IS NULL
                           INNER JOIN tools t ON t.id = oi.tool_id
                           WHERE o.deleted_at IS NULL
                             AND os.name NOT IN (@completedStatus, @waitingDepositRefundStatus)
                           GROUP BY
                               o.id,
                               c.last_name,
                               c.first_name,
                               c.middle_name
                           HAVING MAX(oi.planned_return_date) >= @today
                              AND MAX(oi.planned_return_date) <= @maxDate
                           ORDER BY MAX(oi.planned_return_date)
                           """;

        return await dbContext.Database
            .SqlQueryRaw<DashboardReturnDto>(
                sql,
                CreateParameter("@today", today),
                CreateParameter("@maxDate", maxDate),
                CreateParameter("@completedStatus", CompletedOrderStatus),
                CreateParameter("@waitingDepositRefundStatus", WaitingDepositRefundStatus))
            .ToListAsync(cancellationToken);
    }

    private async Task<List<DashboardReturnDto>> GetOverdueReturnsAsync(
        CancellationToken cancellationToken)
    {
        var today = DateTime.Today;

        const string sql = """
                           SELECT TOP 5
                               o.id AS OrderId,
                               CONCAT(c.last_name, N' ', c.first_name, N' ', c.middle_name) AS CustomerFullName,
                               STRING_AGG(t.name, N', ') AS ToolsNames,
                               MAX(oi.planned_return_date) AS PlannedReturnDate,
                               DATEDIFF(day, MAX(oi.planned_return_date), @today) AS Days
                           FROM orders o
                           INNER JOIN customers c ON c.id = o.customer_id
                           INNER JOIN order_statuses os ON os.id = o.status_id
                           INNER JOIN order_items oi
                               ON oi.order_id = o.id
                              AND oi.deleted_at IS NULL
                           INNER JOIN tools t ON t.id = oi.tool_id
                           WHERE o.deleted_at IS NULL
                             AND os.name NOT IN (@completedStatus, @waitingDepositRefundStatus)
                           GROUP BY
                               o.id,
                               c.last_name,
                               c.first_name,
                               c.middle_name
                           HAVING MAX(oi.planned_return_date) < @today
                           ORDER BY MAX(oi.planned_return_date)
                           """;

        return await dbContext.Database
            .SqlQueryRaw<DashboardReturnDto>(
                sql,
                CreateParameter("@today", today),
                CreateParameter("@completedStatus", CompletedOrderStatus),
                CreateParameter("@waitingDepositRefundStatus", WaitingDepositRefundStatus))
            .ToListAsync(cancellationToken);
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