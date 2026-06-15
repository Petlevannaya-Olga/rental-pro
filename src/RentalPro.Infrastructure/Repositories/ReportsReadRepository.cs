using CSharpFunctionalExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Repositories;
using RentalPro.Contracts.Reports;
using RentalPro.Shared;

namespace RentalPro.Infrastructure.Repositories;

public sealed class ReportsReadRepository(
    ApplicationDbContext dbContext,
    ILogger<ReportsReadRepository> logger)
    : IReportsReadRepository
{
    private const string RentPaymentType = "Аренда";
    private const string CompletedOrderStatus = "Завершен";
    private const string WaitingDepositRefundStatus = "Ожидает закрытия аренды";

    public async Task<Result<IReadOnlyList<RevenueReportDto>, Errors>> GetRevenueAsync(
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken cancellationToken)
    {
        try
        {
            const string sql = """
                               SELECT
                                   CAST(p.payment_date AS date) AS Date,
                                   COUNT(p.id) AS PaymentsCount,
                                   CAST(ISNULL(SUM(p.amount), 0) AS decimal(18, 2)) AS Amount
                               FROM payments p
                               INNER JOIN payment_types pt
                                   ON pt.id = p.payment_type_id
                               WHERE p.deleted_at IS NULL
                                 AND pt.name = @rentPaymentType
                                 AND p.payment_date >= @dateFrom
                                 AND p.payment_date < @dateTo
                               GROUP BY CAST(p.payment_date AS date)
                               ORDER BY CAST(p.payment_date AS date)
                               """;

            var result = await dbContext.Database
                .SqlQueryRaw<RevenueReportDto>(
                    sql,
                    CreateParameter("@rentPaymentType", RentPaymentType),
                    CreateParameter("@dateFrom", dateFrom),
                    CreateParameter("@dateTo", dateTo.AddDays(1)))
                .ToListAsync(cancellationToken);

            return result;
        }
        catch (OperationCanceledException)
        {
            logger.LogError("Revenue report retrieval operation was cancelled");

            return CommonErrors
                .OperationCancelled("get.revenue.report.was.cancelled")
                .ToErrors();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get revenue report");

            return CommonErrors.Db(
                    "get.revenue.report.from.db.exception",
                    "Failed to get revenue report")
                .ToErrors();
        }
    }

    public async Task<Result<IReadOnlyList<PopularToolReportDto>, Errors>> GetPopularToolsAsync(
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken cancellationToken)
    {
        try
        {
            const string sql = """
                               SELECT
                                   t.id AS ToolId,
                                   t.name AS ToolName,
                                   COUNT(oi.id) AS RentalsCount,
                                   CAST(ISNULL(SUM(
                                       oi.rental_price_per_day *
                                       DATEDIFF(day, oi.start_date, oi.planned_return_date)
                                   ), 0) AS decimal(18, 2)) AS TotalAmount
                               FROM order_items oi
                               INNER JOIN orders o
                                   ON o.id = oi.order_id
                               INNER JOIN tools t
                                   ON t.id = oi.tool_id
                               WHERE oi.deleted_at IS NULL
                                 AND o.deleted_at IS NULL
                                 AND o.order_date >= @dateFrom
                                 AND o.order_date < @dateTo
                               GROUP BY
                                   t.id,
                                   t.name
                               ORDER BY RentalsCount DESC, TotalAmount DESC
                               """;

            var result = await dbContext.Database
                .SqlQueryRaw<PopularToolReportDto>(
                    sql,
                    CreateParameter("@dateFrom", dateFrom),
                    CreateParameter("@dateTo", dateTo.AddDays(1)))
                .ToListAsync(cancellationToken);

            return result;
        }
        catch (OperationCanceledException)
        {
            logger.LogError("Popular tools report retrieval operation was cancelled");

            return CommonErrors
                .OperationCancelled("get.popular.tools.report.was.cancelled")
                .ToErrors();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get popular tools report");

            return CommonErrors.Db(
                    "get.popular.tools.report.from.db.exception",
                    "Failed to get popular tools report")
                .ToErrors();
        }
    }

    public async Task<Result<IReadOnlyList<OverdueReturnReportDto>, Errors>> GetOverdueReturnsAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var today = DateTime.Today;

            const string sql = """
                               SELECT
                                   o.id AS OrderId,
                                   o.number AS OrderNumber,
                                   CONCAT(c.last_name, N' ', c.first_name, N' ', c.middle_name) AS CustomerFullName,
                                   STRING_AGG(t.name, N', ') AS ToolsNames,
                                   MAX(oi.planned_return_date) AS PlannedReturnDate,
                                   DATEDIFF(day, MAX(oi.planned_return_date), @today) AS OverdueDays
                               FROM orders o
                               INNER JOIN customers c
                                   ON c.id = o.customer_id
                               INNER JOIN order_statuses os
                                   ON os.id = o.status_id
                               INNER JOIN order_items oi
                                   ON oi.order_id = o.id
                                  AND oi.deleted_at IS NULL
                               INNER JOIN tools t
                                   ON t.id = oi.tool_id
                               WHERE o.deleted_at IS NULL
                                 AND os.name NOT IN (
                                     @completedStatus,
                                     @waitingDepositRefundStatus
                                 )
                               GROUP BY
                                   o.id,
                                   o.number,
                                   c.last_name,
                                   c.first_name,
                                   c.middle_name
                               HAVING MAX(oi.planned_return_date) < @today
                               ORDER BY MAX(oi.planned_return_date)
                               """;

            var result = await dbContext.Database
                .SqlQueryRaw<OverdueReturnReportDto>(
                    sql,
                    CreateParameter("@today", today),
                    CreateParameter("@completedStatus", CompletedOrderStatus),
                    CreateParameter("@waitingDepositRefundStatus", WaitingDepositRefundStatus))
                .ToListAsync(cancellationToken);

            return result;
        }
        catch (OperationCanceledException)
        {
            logger.LogError("Overdue returns report retrieval operation was cancelled");

            return CommonErrors
                .OperationCancelled("get.overdue.returns.report.was.cancelled")
                .ToErrors();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get overdue returns report");

            return CommonErrors.Db(
                    "get.overdue.returns.report.from.db.exception",
                    "Failed to get overdue returns report")
                .ToErrors();
        }
    }

    public async Task<Result<IReadOnlyList<CustomerReportDto>, Errors>> GetCustomersAsync(
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken cancellationToken)
    {
        try
        {
            const string sql = """
                               SELECT
                                   c.id AS CustomerId,
                                   CONCAT(c.last_name, N' ', c.first_name, N' ', c.middle_name) AS CustomerFullName,
                                   COUNT(DISTINCT o.id) AS OrdersCount,
                                   CAST(ISNULL(SUM(
                                       CASE
                                           WHEN pt.name = @rentPaymentType THEN p.amount
                                           ELSE 0
                                       END
                                   ), 0) AS decimal(18, 2)) AS RentAmount,
                                   MAX(o.order_date) AS LastOrderDate
                               FROM customers c
                               LEFT JOIN orders o
                                   ON o.customer_id = c.id
                                  AND o.deleted_at IS NULL
                                  AND o.order_date >= @dateFrom
                                  AND o.order_date < @dateTo
                               LEFT JOIN payments p
                                   ON p.order_id = o.id
                                  AND p.deleted_at IS NULL
                               LEFT JOIN payment_types pt
                                   ON pt.id = p.payment_type_id
                               WHERE c.deleted_at IS NULL
                               GROUP BY
                                   c.id,
                                   c.last_name,
                                   c.first_name,
                                   c.middle_name
                               HAVING COUNT(DISTINCT o.id) > 0
                               ORDER BY RentAmount DESC, OrdersCount DESC
                               """;

            var result = await dbContext.Database
                .SqlQueryRaw<CustomerReportDto>(
                    sql,
                    CreateParameter("@rentPaymentType", RentPaymentType),
                    CreateParameter("@dateFrom", dateFrom),
                    CreateParameter("@dateTo", dateTo.AddDays(1)))
                .ToListAsync(cancellationToken);

            return result;
        }
        catch (OperationCanceledException)
        {
            logger.LogError("Customers report retrieval operation was cancelled");

            return CommonErrors
                .OperationCancelled("get.customers.report.was.cancelled")
                .ToErrors();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get customers report");

            return CommonErrors.Db(
                    "get.customers.report.from.db.exception",
                    "Failed to get customers report")
                .ToErrors();
        }
    }

    public async Task<Result<IReadOnlyList<PaymentReportDto>, Errors>> GetPaymentsAsync(
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken cancellationToken)
    {
        try
        {
            const string sql = """
                               SELECT
                                   pt.name AS PaymentType,
                                   COUNT(p.id) AS PaymentsCount,
                                   CAST(ISNULL(SUM(p.amount), 0) AS decimal(18, 2)) AS Amount
                               FROM payments p
                               INNER JOIN payment_types pt
                                   ON pt.id = p.payment_type_id
                               WHERE p.deleted_at IS NULL
                                 AND p.payment_date >= @dateFrom
                                 AND p.payment_date < @dateTo
                               GROUP BY pt.name
                               ORDER BY pt.name
                               """;

            var result = await dbContext.Database
                .SqlQueryRaw<PaymentReportDto>(
                    sql,
                    CreateParameter("@dateFrom", dateFrom),
                    CreateParameter("@dateTo", dateTo.AddDays(1)))
                .ToListAsync(cancellationToken);

            return result;
        }
        catch (OperationCanceledException)
        {
            logger.LogError("Payments report retrieval operation was cancelled");

            return CommonErrors
                .OperationCancelled("get.payments.report.was.cancelled")
                .ToErrors();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get payments report");

            return CommonErrors.Db(
                    "get.payments.report.from.db.exception",
                    "Failed to get payments report")
                .ToErrors();
        }
    }

    public async Task<Result<IReadOnlyList<ToolReportDto>, Errors>> GetToolsAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            const string sql = """
                               SELECT
                                   t.id AS ToolId,
                                   t.article_number AS ArticleNumber,
                                   t.name AS ToolName,
                                   tc.name AS CategoryName,
                                   m.name AS ManufacturerName,
                                   ts.name AS StatusName,
                                   CAST(t.rental_price_per_day AS decimal(18, 2)) AS RentalPricePerDay,
                                   CAST(t.deposit_amount AS decimal(18, 2)) AS DepositAmount
                               FROM tools t
                               INNER JOIN tool_categories tc
                                   ON tc.id = t.category_id
                               INNER JOIN manufacturers m
                                   ON m.id = t.manufacturer_id
                               INNER JOIN tool_statuses ts
                                   ON ts.id = t.status_id
                               WHERE t.deleted_at IS NULL
                               ORDER BY
                                   tc.name,
                                   t.name
                               """;

            var result = await dbContext.Database
                .SqlQueryRaw<ToolReportDto>(sql)
                .ToListAsync(cancellationToken);

            return result;
        }
        catch (OperationCanceledException)
        {
            logger.LogError("Tools report retrieval operation was cancelled");

            return CommonErrors
                .OperationCancelled("get.tools.report.was.cancelled")
                .ToErrors();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get tools report");

            return CommonErrors.Db(
                    "get.tools.report.from.db.exception",
                    "Failed to get tools report")
                .ToErrors();
        }
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