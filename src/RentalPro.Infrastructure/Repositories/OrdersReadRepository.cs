using CSharpFunctionalExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Repositories;
using RentalPro.Contracts.Orders;
using RentalPro.Contracts.Payments;
using RentalPro.Domain.Customers;
using RentalPro.Domain.Orders;
using RentalPro.Domain.Payments;
using RentalPro.Shared;

namespace RentalPro.Infrastructure.Repositories;

public sealed class OrdersReadRepository(
    ApplicationDbContext dbContext,
    ILogger<OrdersReadRepository> logger)
    : IOrdersReadRepository
{
    private sealed record OrderDetailsHeaderDto(
        Guid Id,
        string Number,
        Guid CustomerId,
        string CustomerFullName,
        Guid UserId,
        string UserFullName,
        Guid StatusId,
        string StatusName,
        DateTime OrderDate,
        string? Comment,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        decimal TotalCost,
        decimal DepositTotal,
        decimal PaidRentalAmount,
        decimal PaidDepositAmount,
        decimal TotalPaidAmount,
        decimal RemainingRentalAmount,
        decimal RemainingDepositAmount,
        decimal TotalRemainingAmount,
        decimal RefundedDepositAmount,
        decimal RemainingDepositRefundAmount,
        bool AllItemsReturned,
        decimal PlannedRentalAmount,
        decimal ActualRentalAmount,
        decimal RentalBalanceAmount,
        decimal RentalRefundAmount,
        decimal RentalAdditionalPaymentAmount);

    private sealed record RentalContractSqlDto(
        Guid OrderId,
        string ContractNumber,
        DateTime ContractDate,
        string UserFullName,
        string CustomerFullName,
        string CustomerPassport,
        string CustomerAddress,
        string CustomerPhone,
        string CustomerEmail,
        decimal TotalRentalPrice,
        decimal TotalDeposit,
        decimal TotalAmount);

    private sealed record OrderDocumentDateDto(
        DateTime Date,
        int Type);

    private sealed record TransferActHeaderSqlDto(
        Guid OrderId,
        string ContractNumber,
        DateTime ContractDate,
        string UserFullName,
        string CustomerFullName,
        string CustomerPassport,
        string CustomerAddress,
        string CustomerPhone,
        string CustomerEmail);

    private sealed record TransferActItemSqlDto(
        Guid OrderItemId,
        Guid ToolId,
        string ToolName,
        string InventoryNumber,
        string SerialNumber,
        DateTime StartDate,
        DateTime PlannedReturnDate,
        int RentalDays,
        decimal RentalPrice,
        decimal DepositAmount,
        string? Comment);

    private sealed record ReturnActHeaderSqlDto(
        Guid OrderId,
        string ContractNumber,
        DateTime ContractDate,
        string UserFullName,
        string CustomerFullName,
        string CustomerPassport,
        string CustomerAddress,
        string CustomerPhone,
        string CustomerEmail);

    private sealed record ReturnActItemSqlDto(
        Guid OrderItemId,
        Guid ToolId,
        string ToolName,
        string InventoryNumber,
        string SerialNumber,
        DateTime StartDate,
        DateTime PlannedReturnDate,
        DateTime ActualReturnedDate,
        string? ReturnCondition,
        string? DamageComment);

    private sealed record PaymentFiscalizationSqlDto(
        Guid PaymentId,
        string PaymentNumber,
        Guid OrderId,
        string OrderNumber,
        string CustomerFullName,
        string CustomerEmail,
        string CustomerPhone,
        string PaymentTypeName,
        string PaymentMethodName,
        decimal Amount,
        DateTime PaymentDate);

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
            page = NormalizePage(page);
            pageSize = NormalizePageSize(pageSize);

            var searchData = CreateSearchData(search);
            var orderBy = GetOrderBy(sortBy, descending);
            var offset = (page - 1) * pageSize;

            var totalCount = await GetOrdersCountAsync(
                searchData,
                statusId,
                startFrom,
                startTo,
                endFrom,
                endTo,
                cancellationToken);

            var sql = $"""
                       SELECT
                           o.id AS Id,
                           o.number AS Number,

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
                           oa.EndDate AS PlannedReturnDate,
                           ISNULL(oa.RentalDays, 0) AS RentalDays,

                           ISNULL(oa.ActualRentalAmount, 0) AS TotalCost,
                           ISNULL(oa.RemainingDepositAmount, 0) AS DepositTotal,

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
                    CreateFilterParameters(
                        searchData,
                        statusId,
                        startFrom,
                        startTo,
                        endFrom,
                        endTo,
                        CreateParameter("@offset", offset),
                        CreateParameter("@pageSize", pageSize)))
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

    public async Task<Result<OrderDetailsDto, Errors>> GetByIdAsync(
        OrderId orderId,
        CancellationToken cancellationToken)
    {
        try
        {
            var headerResult = await GetOrderDetailsHeaderAsync(
                orderId,
                cancellationToken);

            if (headerResult.IsFailure)
                return headerResult.Error;

            var itemsResult = await GetOrderDetailsItemsAsync(
                orderId,
                cancellationToken);

            if (itemsResult.IsFailure)
                return itemsResult.Error;

            var paymentsResult = await GetOrderDetailsPaymentsAsync(
                orderId,
                cancellationToken);

            if (paymentsResult.IsFailure)
                return paymentsResult.Error;

            var header = headerResult.Value;
            var items = itemsResult.Value;
            var payments = paymentsResult.Value;

            return new OrderDetailsDto(
                Id: header.Id,
                Number: header.Number,
                CustomerId: header.CustomerId,
                CustomerFullName: header.CustomerFullName,
                UserId: header.UserId,
                UserFullName: header.UserFullName,
                StatusId: header.StatusId,
                StatusName: header.StatusName,
                OrderDate: header.OrderDate,
                Comment: header.Comment,
                CreatedAt: header.CreatedAt,
                UpdatedAt: header.UpdatedAt,
                TotalCost: header.TotalCost,
                DepositTotal: header.DepositTotal,
                PaidRentalAmount: header.PaidRentalAmount,
                PaidDepositAmount: header.PaidDepositAmount,
                TotalPaidAmount: header.TotalPaidAmount,
                RemainingRentalAmount: header.RemainingRentalAmount,
                RemainingDepositAmount: header.RemainingDepositAmount,
                TotalRemainingAmount: header.TotalRemainingAmount,
                RefundedDepositAmount: header.RefundedDepositAmount,
                RemainingDepositRefundAmount: header.RemainingDepositRefundAmount,
                AllItemsReturned: header.AllItemsReturned,
                PlannedRentalAmount: header.PlannedRentalAmount,
                ActualRentalAmount: header.ActualRentalAmount,
                RentalBalanceAmount: header.RentalBalanceAmount,
                RentalRefundAmount: header.RentalRefundAmount,
                RentalAdditionalPaymentAmount: header.RentalAdditionalPaymentAmount,
                Payments: payments,
                Items: items);
        }
        catch (OperationCanceledException)
        {
            return CommonErrors
                .OperationCancelled("get.order.by.id.was.cancelled")
                .ToErrors();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to get order by id {OrderId}",
                orderId.Value);

            return CommonErrors.Db(
                    "get.order.by.id.from.db.exception",
                    "Failed to get order by id")
                .ToErrors();
        }
    }

    private async Task<Result<IReadOnlyList<OrderDetailsPaymentDto>, Errors>> GetOrderDetailsPaymentsAsync(
        OrderId orderId,
        CancellationToken cancellationToken)
    {
        try
        {
            var sql = """
                      SELECT
                          p.id AS Id,
                          pt.name AS PaymentTypeName,
                          pm.name AS PaymentMethodName,
                          p.amount AS Amount,
                          p.payment_date AS PaymentDate,
                          p.comment AS Comment
                      FROM payments p
                      INNER JOIN payment_types pt ON pt.id = p.payment_type_id
                      INNER JOIN payment_methods pm ON pm.id = p.payment_method_id
                      WHERE p.order_id = @orderId
                        AND p.deleted_at IS NULL
                        AND pt.deleted_at IS NULL
                        AND pm.deleted_at IS NULL
                      ORDER BY p.payment_date, p.created_at
                      """;

            var payments = await dbContext.Database
                .SqlQueryRaw<OrderDetailsPaymentDto>(
                    sql,
                    CreateParameter("@orderId", orderId.Value))
                .ToListAsync(cancellationToken);

            return payments;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to get payments for order {OrderId}",
                orderId.Value);

            return CommonErrors.Db(
                    "get.order.payments.from.db.exception",
                    "Failed to get order payments")
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
                          ISNULL(SUM(CASE WHEN os.name = N'Завершен' THEN 1 ELSE 0 END), 0) AS CompletedCount,
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
            var searchData = CreateSearchData(search);
            var orderBy = GetOrderBy(sortBy, descending);

            var sql = $"""
                       SELECT
                           o.id AS Id,
                           o.number AS Number,

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
                           oa.EndDate AS PlannedReturnDate,
                           ISNULL(oa.RentalDays, 0) AS RentalDays,

                           ISNULL(oa.ActualRentalAmount, 0) AS TotalCost,
                           ISNULL(oa.RemainingDepositAmount, 0) AS DepositTotal,

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
                    CreateFilterParameters(
                        searchData,
                        statusId,
                        startFrom,
                        startTo,
                        endFrom,
                        endTo))
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

    private static string GetExportOrderBy(
        string? sortBy,
        bool descending)
    {
        var direction = descending
            ? "DESC"
            : "ASC";

        var column = sortBy?.Trim().ToLowerInvariant() switch
        {
            "number" => "o.number",
            "customerfullname" => "CONCAT_WS(' ', c.last_name, c.first_name, c.middle_name)",
            "userfullname" => "CONCAT_WS(' ', u.last_name, u.first_name, u.middle_name)",
            "statusname" => "os.name",
            "itemscount" => "ISNULL(oa.ItemsCount, 0)",
            "toolsnames" => "ISNULL(oa.ToolsNames, N'')",
            "orderdate" => "o.order_date",
            "startdate" => "oa.StartDate",
            "plannedreturndate" => "oa.NearestReturnDate",

            "rentalamount" => "ISNULL(oa.RentalAmount, 0)",
            "depositamount" => "ISNULL(oa.DepositAmount, 0)",
            "totalamount" => "ISNULL(oa.RentalAmount, 0) + ISNULL(oa.DepositAmount, 0)",

            "createdat" => "o.created_at",
            "updatedat" => "o.updated_at",
            _ => "o.order_date"
        };

        return $"ORDER BY {column} {direction}";
    }

    private async Task<int> GetOrdersCountAsync(
        SearchData searchData,
        OrderStatusId? statusId,
        DateOnly? startFrom,
        DateOnly? startTo,
        DateOnly? endFrom,
        DateOnly? endTo,
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
                    statusId,
                    startFrom,
                    startTo,
                    endFrom,
                    endTo))
            .SingleAsync(cancellationToken);
    }

    private async Task<Result<OrderDetailsHeaderDto, Errors>> GetOrderDetailsHeaderAsync(
        OrderId orderId,
        CancellationToken cancellationToken)
    {
        try
        {
            var sql = """
                      SELECT
                          o.id AS Id,
                          o.number AS Number,

                          o.customer_id AS CustomerId,
                          CONCAT_WS(' ', c.last_name, c.first_name, c.middle_name) AS CustomerFullName,

                          o.user_id AS UserId,
                          CONCAT_WS(' ', u.last_name, u.first_name, u.middle_name) AS UserFullName,

                          o.status_id AS StatusId,
                          os.name AS StatusName,

                          o.order_date AS OrderDate,
                          o.comment AS Comment,
                          o.created_at AS CreatedAt,
                          o.updated_at AS UpdatedAt,

                          ISNULL(totals.PlannedRentalAmount, 0) AS TotalCost,
                          ISNULL(totals.DepositTotal, 0) AS DepositTotal,

                          ISNULL(payments.PaidRentalAmount, 0) AS PaidRentalAmount,
                          ISNULL(payments.PaidDepositAmount, 0) AS PaidDepositAmount,

                          ISNULL(payments.PaidRentalAmount, 0)
                          + ISNULL(payments.PaidDepositAmount, 0) AS TotalPaidAmount,

                          CASE
                              WHEN ISNULL(totals.PlannedRentalAmount, 0) - ISNULL(payments.PaidRentalAmount, 0) < 0
                                  THEN 0
                              ELSE ISNULL(totals.PlannedRentalAmount, 0) - ISNULL(payments.PaidRentalAmount, 0)
                          END AS RemainingRentalAmount,

                          CASE
                              WHEN ISNULL(totals.DepositTotal, 0) - ISNULL(payments.PaidDepositAmount, 0) < 0
                                  THEN 0
                              ELSE ISNULL(totals.DepositTotal, 0) - ISNULL(payments.PaidDepositAmount, 0)
                          END AS RemainingDepositAmount,

                          CASE
                              WHEN ISNULL(totals.PlannedRentalAmount, 0)
                                   + ISNULL(totals.DepositTotal, 0)
                                   - (
                                        ISNULL(payments.PaidRentalAmount, 0)
                                        + ISNULL(payments.PaidDepositAmount, 0)
                                     ) < 0
                                  THEN 0
                              ELSE ISNULL(totals.PlannedRentalAmount, 0)
                                   + ISNULL(totals.DepositTotal, 0)
                                   - (
                                        ISNULL(payments.PaidRentalAmount, 0)
                                        + ISNULL(payments.PaidDepositAmount, 0)
                                     )
                          END AS TotalRemainingAmount,

                          ISNULL(payments.RefundedDepositAmount, 0) AS RefundedDepositAmount,

                          CASE
                              WHEN ISNULL(payments.PaidDepositAmount, 0) - ISNULL(payments.RefundedDepositAmount, 0) < 0
                                  THEN 0
                              ELSE ISNULL(payments.PaidDepositAmount, 0) - ISNULL(payments.RefundedDepositAmount, 0)
                          END AS RemainingDepositRefundAmount,

                          ISNULL(totals.PlannedRentalAmount, 0) AS PlannedRentalAmount,
                          ISNULL(totals.ActualRentalAmount, 0) AS ActualRentalAmount,

                          ISNULL(totals.ActualRentalAmount, 0)
                          - ISNULL(payments.PaidRentalAmount, 0)
                          + ISNULL(payments.RefundedRentalAmount, 0) AS RentalBalanceAmount,

                          CASE
                              WHEN ISNULL(totals.ActualRentalAmount, 0)
                                   - ISNULL(payments.PaidRentalAmount, 0)
                                   + ISNULL(payments.RefundedRentalAmount, 0) < 0
                                  THEN ABS(
                                      ISNULL(totals.ActualRentalAmount, 0)
                                      - ISNULL(payments.PaidRentalAmount, 0)
                                      + ISNULL(payments.RefundedRentalAmount, 0)
                                  )
                              ELSE 0
                          END AS RentalRefundAmount,

                          CASE
                              WHEN ISNULL(totals.ActualRentalAmount, 0)
                                   - ISNULL(payments.PaidRentalAmount, 0)
                                   + ISNULL(payments.RefundedRentalAmount, 0) > 0
                                  THEN ISNULL(totals.ActualRentalAmount, 0)
                                       - ISNULL(payments.PaidRentalAmount, 0)
                                       + ISNULL(payments.RefundedRentalAmount, 0)
                              ELSE 0
                          END AS RentalAdditionalPaymentAmount,

                          CASE
                              WHEN EXISTS (
                                  SELECT 1
                                  FROM order_items oi_check
                                  WHERE oi_check.order_id = o.id
                                    AND oi_check.deleted_at IS NULL
                                    AND oi_check.actual_returned_date IS NULL
                              )
                                  THEN CAST(0 AS bit)
                              ELSE CAST(1 AS bit)
                          END AS AllItemsReturned

                      FROM orders o
                      INNER JOIN customers c ON c.id = o.customer_id
                      INNER JOIN users u ON u.id = o.user_id
                      INNER JOIN order_statuses os ON os.id = o.status_id

                      OUTER APPLY
                      (
                          SELECT
                              SUM(
                                  oi.rental_price_per_day *
                                  CASE
                                      WHEN DATEDIFF(DAY, oi.start_date, oi.planned_return_date) <= 0
                                          THEN 1
                                      ELSE DATEDIFF(DAY, oi.start_date, oi.planned_return_date)
                                  END
                              ) AS PlannedRentalAmount,

                              SUM(
                          oi.rental_price_per_day *
                          CASE
                              WHEN oi.actual_returned_date IS NULL
                                  THEN
                                      CASE
                                          WHEN DATEDIFF(DAY, oi.start_date, oi.planned_return_date) <= 0
                                              THEN 1
                                          ELSE DATEDIFF(DAY, oi.start_date, oi.planned_return_date)
                                      END
                              ELSE
                                  CASE
                                      WHEN DATEDIFF(DAY, oi.start_date, oi.actual_returned_date) < 0
                                          THEN 0
                                      ELSE DATEDIFF(DAY, oi.start_date, oi.actual_returned_date)
                                  END
                          END
                      ) AS ActualRentalAmount,

                              SUM(oi.deposit_amount) AS DepositTotal
                          FROM order_items oi
                          WHERE oi.order_id = o.id
                            AND oi.deleted_at IS NULL
                      ) totals

                      OUTER APPLY
                      (
                          SELECT
                              SUM(CASE
                                  WHEN pt.name = N'Аренда'
                                      THEN p.amount
                                  ELSE 0
                              END) AS PaidRentalAmount,

                              SUM(CASE
                                  WHEN pt.name = N'Залог'
                                      THEN p.amount
                                  ELSE 0
                              END) AS PaidDepositAmount,

                              SUM(CASE
                                  WHEN pt.name = N'Возврат залога'
                                      THEN p.amount
                                  ELSE 0
                              END) AS RefundedDepositAmount,

                              SUM(CASE
                                  WHEN pt.name = N'Возврат аренды'
                                      THEN p.amount
                                  ELSE 0
                              END) AS RefundedRentalAmount

                          FROM payments p
                          INNER JOIN payment_types pt ON pt.id = p.payment_type_id
                          WHERE p.order_id = o.id
                            AND p.deleted_at IS NULL
                            AND pt.deleted_at IS NULL
                      ) payments

                      WHERE o.id = @orderId
                        AND o.deleted_at IS NULL
                        AND c.deleted_at IS NULL
                        AND u.deleted_at IS NULL
                        AND os.deleted_at IS NULL
                      """;

            var header = await dbContext.Database
                .SqlQueryRaw<OrderDetailsHeaderDto>(
                    sql,
                    CreateParameter("@orderId", orderId.Value))
                .FirstOrDefaultAsync(cancellationToken);

            if (header is null)
            {
                return CommonErrors.NotFound(
                        "order.was.not.found",
                        "Заказ не найден",
                        orderId.Value)
                    .ToErrors();
            }

            return header;
        }
        catch (OperationCanceledException)
        {
            return CommonErrors
                .OperationCancelled("get.order.details.header.was.cancelled")
                .ToErrors();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to get order details header by id {OrderId}",
                orderId.Value);

            return CommonErrors.Db(
                    "get.order.details.header.from.db.exception",
                    "Failed to get order details header")
                .ToErrors();
        }
    }

    private async Task<Result<List<OrderDetailsItemDto>, Errors>> GetOrderDetailsItemsAsync(
        OrderId orderId,
        CancellationToken cancellationToken)
    {
        var sql = """
                  SELECT
                      oi.id AS Id,
                      t.id AS ToolId,
                      t.name AS ToolName,

                      oi.start_date AS StartDate,
                      oi.planned_return_date AS PlannedReturnDate,
                      oi.actual_returned_date AS ActualReturnedDate,

                      oi.rental_price_per_day AS RentalPricePerDay,
                      oi.deposit_amount AS DepositAmount,

                      CAST(
                          oi.rental_price_per_day *
                          DATEDIFF(
                              DAY,
                              oi.start_date,
                              ISNULL(
                                  oi.actual_returned_date,
                                  oi.planned_return_date
                              )
                          )
                          AS decimal(18, 2)
                      ) AS TotalAmount

                  FROM order_items oi

                  INNER JOIN tools t
                      ON t.id = oi.tool_id

                  WHERE oi.order_id = @orderId
                    AND oi.deleted_at IS NULL
                    AND t.deleted_at IS NULL

                  ORDER BY t.name
                  """;

        return await dbContext.Database
            .SqlQueryRaw<OrderDetailsItemDto>(
                sql,
                CreateParameter("@orderId", orderId.Value))
            .ToListAsync(cancellationToken);
    }

    public async Task<Result<TransferActDto, Errors>> GetTransferActDataAsync(
        OrderId orderId,
        DateOnly actDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var headerSql = """
                            SELECT
                                o.id AS OrderId,
                                o.number AS ContractNumber,
                                o.order_date AS ContractDate,

                                CONCAT_WS(' ', u.last_name, u.first_name, u.middle_name) AS UserFullName,

                                CONCAT_WS(' ', c.last_name, c.first_name, c.middle_name) AS CustomerFullName,
                                CONCAT_WS(' ', c.passport_series, c.passport_number) AS CustomerPassport,

                                CONCAT(
                                    c.postal_code,
                                    N', ',
                                    c.region,
                                    N', ',
                                    c.city,
                                    N', ',
                                    c.street,
                                    N', д. ',
                                    c.house,
                                    CASE
                                        WHEN c.building IS NULL OR LTRIM(RTRIM(c.building)) = N''
                                            THEN N''
                                        ELSE CONCAT(N', корп. ', c.building)
                                    END,
                                    CASE
                                        WHEN c.apartment IS NULL OR LTRIM(RTRIM(c.apartment)) = N''
                                            THEN N''
                                        ELSE CONCAT(N', кв./офис ', c.apartment)
                                    END
                                ) AS CustomerAddress,

                                c.phone_number AS CustomerPhone,
                                c.email AS CustomerEmail

                            FROM orders o
                            INNER JOIN customers c ON c.id = o.customer_id
                            INNER JOIN users u ON u.id = o.user_id
                            WHERE o.id = @orderId
                              AND o.deleted_at IS NULL
                              AND c.deleted_at IS NULL
                              AND u.deleted_at IS NULL
                            """;

            var header = await dbContext.Database
                .SqlQueryRaw<TransferActHeaderSqlDto>(
                    headerSql,
                    CreateParameter("@orderId", orderId.Value))
                .FirstOrDefaultAsync(cancellationToken);

            if (header is null)
            {
                return CommonErrors.NotFound(
                        "order.not.found",
                        "Заказ не найден",
                        orderId.Value)
                    .ToErrors();
            }

            var itemsSql = """
                           SELECT
                               oi.id AS OrderItemId,
                               t.id AS ToolId,
                               t.name AS ToolName,
                               t.inventory_number AS InventoryNumber,
                               t.serial_number AS SerialNumber,

                               oi.start_date AS StartDate,
                               oi.planned_return_date AS PlannedReturnDate,

                               DATEDIFF(DAY, oi.start_date, oi.planned_return_date) AS RentalDays,

                               oi.rental_price_per_day *
                               DATEDIFF(DAY, oi.start_date, oi.planned_return_date) AS RentalPrice,

                               oi.deposit_amount AS DepositAmount,

                               o.comment AS Comment

                           FROM order_items oi
                           INNER JOIN orders o ON o.id = oi.order_id
                           INNER JOIN tools t ON t.id = oi.tool_id
                           WHERE oi.order_id = @orderId
                             AND oi.start_date = @actDate
                             AND oi.deleted_at IS NULL
                             AND o.deleted_at IS NULL
                             AND t.deleted_at IS NULL
                           ORDER BY t.name
                           """;

            var items = await dbContext.Database
                .SqlQueryRaw<TransferActItemSqlDto>(
                    itemsSql,
                    CreateParameter("@orderId", orderId.Value),
                    CreateParameter("@actDate", actDate.ToDateTime(TimeOnly.MinValue)))
                .ToListAsync(cancellationToken);

            if (items.Count == 0)
            {
                return CommonErrors.NotFound(
                        "transfer.act.items.not.found",
                        "На выбранную дату выдачи нет инструментов",
                        orderId.Value)
                    .ToErrors();
            }

            var actItems = items
                .Select(item => new TransferActItemDto(
                    OrderItemId: item.OrderItemId,
                    ToolId: item.ToolId,
                    ToolName: item.ToolName,
                    InventoryNumber: item.InventoryNumber,
                    SerialNumber: item.SerialNumber,
                    StartDate: DateOnly.FromDateTime(item.StartDate),
                    PlannedReturnDate: DateOnly.FromDateTime(item.PlannedReturnDate),
                    RentalDays: item.RentalDays <= 0 ? 1 : item.RentalDays,
                    RentalPrice: item.RentalPrice,
                    DepositAmount: item.DepositAmount,
                    Condition: "Исправно, без видимых повреждений",
                    Comment: item.Comment))
                .ToList();

            var totalRentalPrice = actItems.Sum(x => x.RentalPrice);
            var totalDeposit = actItems.Sum(x => x.DepositAmount);

            var act = new TransferActDto(
                OrderId: header.OrderId,
                ActNumber: $"ACT-TR-{header.ContractNumber}-{actDate:yyyyMMdd}",
                ActDate: actDate,
                ContractNumber: header.ContractNumber,
                ContractDate: DateOnly.FromDateTime(header.ContractDate),
                UserFullName: header.UserFullName,
                Customer: new CustomerContractDto(
                    FullName: header.CustomerFullName,
                    Passport: header.CustomerPassport,
                    Address: header.CustomerAddress,
                    Phone: header.CustomerPhone,
                    Email: header.CustomerEmail),
                TotalRentalPrice: totalRentalPrice,
                TotalDeposit: totalDeposit,
                Items: actItems);

            return act;
        }
        catch (OperationCanceledException)
        {
            return CommonErrors
                .OperationCancelled("get.transfer.act.data.was.cancelled")
                .ToErrors();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to get transfer act data for order {OrderId} and date {ActDate}",
                orderId.Value,
                actDate);

            return CommonErrors.Db(
                    "get.transfer.act.data.from.db.exception",
                    "Failed to get transfer act data")
                .ToErrors();
        }
    }

    public async Task<Result<ReturnActDto, Errors>> GetReturnActDataAsync(
        OrderId orderId,
        DateOnly actDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var headerSql = """
                            SELECT
                                o.id AS OrderId,
                                o.number AS ContractNumber,
                                o.order_date AS ContractDate,

                                CONCAT_WS(' ', u.last_name, u.first_name, u.middle_name) AS UserFullName,

                                CONCAT_WS(' ', c.last_name, c.first_name, c.middle_name) AS CustomerFullName,
                                CONCAT_WS(' ', c.passport_series, c.passport_number) AS CustomerPassport,

                                CONCAT(
                                    c.postal_code,
                                    N', ',
                                    c.region,
                                    N', ',
                                    c.city,
                                    N', ',
                                    c.street,
                                    N', д. ',
                                    c.house,
                                    CASE
                                        WHEN c.building IS NULL OR LTRIM(RTRIM(c.building)) = N''
                                            THEN N''
                                        ELSE CONCAT(N', корп. ', c.building)
                                    END,
                                    CASE
                                        WHEN c.apartment IS NULL OR LTRIM(RTRIM(c.apartment)) = N''
                                            THEN N''
                                        ELSE CONCAT(N', кв./офис ', c.apartment)
                                    END
                                ) AS CustomerAddress,

                                c.phone_number AS CustomerPhone,
                                c.email AS CustomerEmail

                            FROM orders o
                            INNER JOIN customers c ON c.id = o.customer_id
                            INNER JOIN users u ON u.id = o.user_id
                            WHERE o.id = @orderId
                              AND o.deleted_at IS NULL
                              AND c.deleted_at IS NULL
                              AND u.deleted_at IS NULL
                            """;

            var header = await dbContext.Database
                .SqlQueryRaw<ReturnActHeaderSqlDto>(
                    headerSql,
                    CreateParameter("@orderId", orderId.Value))
                .FirstOrDefaultAsync(cancellationToken);

            if (header is null)
            {
                return CommonErrors.NotFound(
                        "order.not.found",
                        "Заказ не найден",
                        orderId.Value)
                    .ToErrors();
            }

            var itemsSql = """
                           SELECT
                               oi.id AS OrderItemId,
                               t.id AS ToolId,
                               t.name AS ToolName,
                               t.inventory_number AS InventoryNumber,
                               t.serial_number AS SerialNumber,

                               oi.start_date AS StartDate,
                               oi.planned_return_date AS PlannedReturnDate,

                               COALESCE(
                                   oi.actual_returned_date,
                                   oi.planned_return_date) AS ActualReturnedDate,

                               COALESCE(
                                   oi.return_condition,
                                   N'Осмотр при возврате') AS ReturnCondition,

                               COALESCE(
                                   oi.damage_comment,
                                   N'Заполняется при возврате') AS DamageComment

                           FROM order_items oi
                           INNER JOIN tools t ON t.id = oi.tool_id
                           WHERE oi.order_id = @orderId
                             AND COALESCE(
                           oi.actual_returned_date,
                           oi.planned_return_date) = @actDate
                             AND oi.deleted_at IS NULL
                             AND t.deleted_at IS NULL
                           ORDER BY t.name
                           """;

            var items = await dbContext.Database
                .SqlQueryRaw<ReturnActItemSqlDto>(
                    itemsSql,
                    CreateParameter("@orderId", orderId.Value),
                    CreateParameter("@actDate", actDate.ToDateTime(TimeOnly.MinValue)))
                .ToListAsync(cancellationToken);

            if (items.Count == 0)
            {
                return CommonErrors.NotFound(
                        "return.act.items.not.found",
                        "На выбранную дату возврата нет инструментов",
                        orderId.Value)
                    .ToErrors();
            }

            var actItems = items
                .Select(item => new ReturnActItemDto(
                    OrderItemId: item.OrderItemId,
                    ToolId: item.ToolId,
                    ToolName: item.ToolName,
                    InventoryNumber: item.InventoryNumber,
                    SerialNumber: item.SerialNumber,
                    StartDate: DateOnly.FromDateTime(item.StartDate),
                    PlannedReturnDate: DateOnly.FromDateTime(item.PlannedReturnDate),
                    ActualReturnedDate: DateOnly.FromDateTime(item.ActualReturnedDate),
                    ReturnCondition: string.IsNullOrWhiteSpace(item.ReturnCondition)
                        ? "Исправно"
                        : item.ReturnCondition,
                    DamageComment: string.IsNullOrWhiteSpace(item.DamageComment)
                        ? "Отсутствуют"
                        : item.DamageComment))
                .ToList();

            var act = new ReturnActDto(
                OrderId: header.OrderId,
                ActNumber: $"ACT-RET-{header.ContractNumber}-{actDate:yyyyMMdd}",
                ActDate: actDate,
                ContractNumber: header.ContractNumber,
                ContractDate: DateOnly.FromDateTime(header.ContractDate),
                UserFullName: header.UserFullName,
                Customer: new CustomerContractDto(
                    FullName: header.CustomerFullName,
                    Passport: header.CustomerPassport,
                    Address: header.CustomerAddress,
                    Phone: header.CustomerPhone,
                    Email: header.CustomerEmail),
                Items: actItems);

            return act;
        }
        catch (OperationCanceledException)
        {
            return CommonErrors
                .OperationCancelled("get.return.act.data.was.cancelled")
                .ToErrors();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to get return act data for order {OrderId} and date {ActDate}",
                orderId.Value,
                actDate);

            return CommonErrors.Db(
                    "get.return.act.data.from.db.exception",
                    "Failed to get return act data")
                .ToErrors();
        }
    }

    public async Task<Result<PaymentFiscalizationDto, Errors>> GetPaymentFiscalizationDataAsync(
        PaymentId paymentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var sql = """
                      SELECT
                          p.id AS PaymentId,
                          CONCAT(N'PAY-', CONVERT(nvarchar(36), p.id)) AS PaymentNumber,

                          o.id AS OrderId,
                          o.number AS OrderNumber,

                          CONCAT_WS(' ', c.last_name, c.first_name, c.middle_name) AS CustomerFullName,
                          c.email AS CustomerEmail,
                          c.phone_number AS CustomerPhone,

                          pt.name AS PaymentTypeName,
                          pm.name AS PaymentMethodName,

                          p.amount AS Amount,
                          p.payment_date AS PaymentDate

                      FROM payments p
                      INNER JOIN orders o ON o.id = p.order_id
                      INNER JOIN customers c ON c.id = o.customer_id
                      INNER JOIN payment_types pt ON pt.id = p.payment_type_id
                      INNER JOIN payment_methods pm ON pm.id = p.payment_method_id

                      WHERE p.id = @paymentId
                        AND p.deleted_at IS NULL
                        AND o.deleted_at IS NULL
                        AND c.deleted_at IS NULL
                        AND pt.deleted_at IS NULL
                        AND pm.deleted_at IS NULL
                      """;

            var data = await dbContext.Database
                .SqlQueryRaw<PaymentFiscalizationSqlDto>(
                    sql,
                    CreateParameter("@paymentId", paymentId.Value))
                .FirstOrDefaultAsync(cancellationToken);

            if (data is null)
            {
                return CommonErrors.NotFound(
                        "payment.fiscalization.data.not.found",
                        "Данные для фискализации платежа не найдены",
                        paymentId.Value)
                    .ToErrors();
            }

            return new PaymentFiscalizationDto(
                PaymentId: data.PaymentId,
                PaymentNumber: data.PaymentNumber,
                OrderId: data.OrderId,
                OrderNumber: data.OrderNumber,
                CustomerFullName: data.CustomerFullName,
                CustomerEmail: data.CustomerEmail,
                CustomerPhone: data.CustomerPhone,
                PaymentTypeName: data.PaymentTypeName,
                PaymentMethodName: data.PaymentMethodName,
                Amount: data.Amount,
                PaymentDate: data.PaymentDate);
        }
        catch (OperationCanceledException)
        {
            return CommonErrors
                .OperationCancelled("get.payment.fiscalization.data.was.cancelled")
                .ToErrors();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to get payment fiscalization data by payment id {PaymentId}",
                paymentId.Value);

            return CommonErrors.Db(
                    "get.payment.fiscalization.data.from.db.exception",
                    "Failed to get payment fiscalization data")
                .ToErrors();
        }
    }

    public async Task<Result<bool, Errors>> CustomerHasOrdersAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var customerIdValue = CustomerId.Restore(customerId);

            var hasOrders = await dbContext.Orders
                .AnyAsync(
                    order => order.CustomerId == customerIdValue &&
                             order.DeletedAt == null,
                    cancellationToken);

            return hasOrders;
        }
        catch (OperationCanceledException)
        {
            return CommonErrors
                .OperationCancelled("check.customer.orders.was.cancelled")
                .ToErrors();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to check orders for customer {CustomerId}",
                customerId);

            return CommonErrors.Db(
                    "check.customer.orders.from.db.exception",
                    "Не удалось проверить наличие заказов у клиента")
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
        OrderStatusId? statusId,
        DateOnly? startFrom,
        DateOnly? startTo,
        DateOnly? endFrom,
        DateOnly? endTo,
        params SqlParameter[] additionalParameters)
    {
        return
        [
            CreateParameter("@search", searchData.SearchText),
            CreateParameter("@searchPattern", searchData.SearchPattern),
            CreateParameter("@fullNameSearchPattern", searchData.FullNameSearchPattern),
            CreateParameter("@statusId", statusId?.Value),
            CreateParameter("@startFrom", startFrom),
            CreateParameter("@startTo", startTo),
            CreateParameter("@endFrom", endFrom),
            CreateParameter("@endTo", endTo),
            ..additionalParameters
        ];
    }

    public async Task<Result<IReadOnlyList<OrderDocumentDto>, Errors>> GetDocumentsAsync(
        OrderId orderId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var orderExistsSql = """
                                 SELECT COUNT(*) AS Value
                                 FROM orders o
                                 WHERE o.id = @orderId
                                   AND o.deleted_at IS NULL
                                 """;

            var orderExists = await dbContext.Database
                .SqlQueryRaw<int>(
                    orderExistsSql,
                    CreateParameter("@orderId", orderId.Value))
                .SingleAsync(cancellationToken);

            if (orderExists == 0)
            {
                return CommonErrors.NotFound(
                        "order.not.found",
                        "Заказ не найден",
                        orderId.Value)
                    .ToErrors();
            }

            var sql = """
                      SELECT DISTINCT
                          CAST(oi.start_date AS date) AS Date,
                          2 AS Type
                      FROM order_items oi
                      INNER JOIN tools t ON t.id = oi.tool_id
                      WHERE oi.order_id = @orderId
                        AND oi.deleted_at IS NULL
                        AND t.deleted_at IS NULL

                      UNION

                      SELECT DISTINCT
                          CAST(oi.actual_returned_date AS date) AS Date,
                          3 AS Type
                      FROM order_items oi
                      INNER JOIN tools t ON t.id = oi.tool_id
                      WHERE oi.order_id = @orderId
                        AND oi.actual_returned_date IS NOT NULL
                        AND oi.deleted_at IS NULL
                        AND t.deleted_at IS NULL

                      ORDER BY Date, Type
                      """;

            var dates = await dbContext.Database
                .SqlQueryRaw<OrderDocumentDateDto>(
                    sql,
                    CreateParameter("@orderId", orderId.Value))
                .ToListAsync(cancellationToken);

            var documents = new List<OrderDocumentDto>
            {
                new(
                    OrderDocumentType.Contract,
                    null,
                    "Договор аренды")
            };

            foreach (var item in dates)
            {
                var date = DateOnly.FromDateTime(item.Date);
                var type = (OrderDocumentType)item.Type;

                var title = type switch
                {
                    OrderDocumentType.TransferAct =>
                        $"Акт выдачи от {date:dd.MM.yyyy}",

                    OrderDocumentType.ReturnAct =>
                        $"Акт возврата от {date:dd.MM.yyyy}",

                    _ => "Документ"
                };

                documents.Add(new OrderDocumentDto(
                    type,
                    date,
                    title));
            }

            return documents;
        }
        catch (OperationCanceledException)
        {
            return CommonErrors
                .OperationCancelled("get.order.documents.was.cancelled")
                .ToErrors();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to get documents for order {OrderId}",
                orderId.Value);

            return CommonErrors.Db(
                    "get.order.documents.from.db.exception",
                    "Failed to get order documents")
                .ToErrors();
        }
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
            "number" => $"ORDER BY o.number {direction}",
            "customer" => $"ORDER BY c.last_name {direction}, c.first_name {direction}, c.middle_name {direction}",
            "user" => $"ORDER BY u.last_name {direction}, u.first_name {direction}, u.middle_name {direction}",
            "status" => $"ORDER BY os.name {direction}",
            "orderdate" => $"ORDER BY o.order_date {direction}",
            "createdat" => $"ORDER BY o.created_at {direction}",
            "updatedat" => $"ORDER BY o.updated_at {direction}",

            "items" => $"ORDER BY oa.ItemsCount {direction}",
            "itemscount" => $"ORDER BY oa.ItemsCount {direction}",

            "amount" => $"ORDER BY oa.ActualRentalAmount {direction}",
            "totalcost" => $"ORDER BY oa.ActualRentalAmount {direction}",

            "deposit" => $"ORDER BY oa.RemainingDepositAmount {direction}",
            "deposittotal" => $"ORDER BY oa.RemainingDepositAmount {direction}",

            "start" => $"ORDER BY oa.StartDate {direction}",
            "startdate" => $"ORDER BY oa.StartDate {direction}",

            "end" => $"ORDER BY oa.EndDate {direction}",
            "plannedreturndate" => $"ORDER BY oa.EndDate {direction}",

            "tool" => $"ORDER BY oa.ToolsNames {direction}",
            "tools" => $"ORDER BY oa.ToolsNames {direction}",

            _ => "ORDER BY o.created_at DESC"
        };
    }

    private sealed record SearchData(
        string? SearchText,
        string? SearchPattern,
        string? FullNameSearchPattern);

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

                                              CASE
                                                  WHEN SUM(
                                                      CASE
                                                          WHEN oi.actual_returned_date IS NULL THEN 1
                                                          ELSE 0
                                                      END
                                                  ) = 0
                                                      THEN MAX(oi.actual_returned_date)
                                                  ELSE MAX(oi.planned_return_date)
                                              END AS EndDate,

                                              CASE
                                                  WHEN MIN(oi.start_date) IS NULL
                                                      THEN 0

                                                  WHEN DATEDIFF(
                                                      DAY,
                                                      MIN(oi.start_date),
                                                      CASE
                                                          WHEN SUM(
                                                              CASE
                                                                  WHEN oi.actual_returned_date IS NULL THEN 1
                                                                  ELSE 0
                                                              END
                                                          ) = 0
                                                              THEN MAX(oi.actual_returned_date)
                                                          ELSE MAX(oi.planned_return_date)
                                                      END
                                                  ) < 0
                                                      THEN 0

                                                  ELSE DATEDIFF(
                                                      DAY,
                                                      MIN(oi.start_date),
                                                      CASE
                                                          WHEN SUM(
                                                              CASE
                                                                  WHEN oi.actual_returned_date IS NULL THEN 1
                                                                  ELSE 0
                                                              END
                                                          ) = 0
                                                              THEN MAX(oi.actual_returned_date)
                                                          ELSE MAX(oi.planned_return_date)
                                                      END
                                                  )
                                              END AS RentalDays,

                                              SUM(
                                                  oi.rental_price_per_day *
                                                  CASE
                                                      WHEN oi.actual_returned_date IS NULL
                                                          THEN
                                                              CASE
                                                                  WHEN DATEDIFF(
                                                                      DAY,
                                                                      oi.start_date,
                                                                      oi.planned_return_date
                                                                  ) <= 0
                                                                      THEN 1
                                                                  ELSE DATEDIFF(
                                                                      DAY,
                                                                      oi.start_date,
                                                                      oi.planned_return_date
                                                                  )
                                                              END

                                                      ELSE
                                                          CASE
                                                              WHEN DATEDIFF(
                                                                  DAY,
                                                                  oi.start_date,
                                                                  oi.actual_returned_date
                                                              ) < 0
                                                                  THEN 0
                                                              ELSE DATEDIFF(
                                                                  DAY,
                                                                  oi.start_date,
                                                                  oi.actual_returned_date
                                                              )
                                                          END
                                                  END
                                              ) AS ActualRentalAmount,

                                              CASE
                                                  WHEN SUM(oi.deposit_amount)
                                                       - ISNULL(MAX(payments.RefundedDepositAmount), 0) < 0
                                                      THEN 0

                                                  ELSE SUM(oi.deposit_amount)
                                                       - ISNULL(MAX(payments.RefundedDepositAmount), 0)
                                              END AS RemainingDepositAmount

                                          FROM order_items oi
                                          INNER JOIN tools t
                                              ON t.id = oi.tool_id

                                          OUTER APPLY
                                          (
                                              SELECT
                                                  SUM(
                                                      CASE
                                                          WHEN pt.name = N'Возврат залога'
                                                              THEN p.amount
                                                          ELSE 0
                                                      END
                                                  ) AS RefundedDepositAmount
                                              FROM payments p
                                              INNER JOIN payment_types pt
                                                  ON pt.id = p.payment_type_id
                                              WHERE p.order_id = oi.order_id
                                                AND p.deleted_at IS NULL
                                                AND pt.deleted_at IS NULL
                                          ) payments

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
                                             OR o.number LIKE @searchPattern
                                             OR CONCAT_WS(' ', c.last_name, c.first_name, c.middle_name) LIKE @fullNameSearchPattern
                                             OR CONCAT_WS(' ', u.last_name, u.first_name, u.middle_name) LIKE @fullNameSearchPattern
                                             OR oa.ToolsNames LIKE @searchPattern
                                         )
                                         AND (@statusId IS NULL OR o.status_id = @statusId)
                                         AND (@startFrom IS NULL OR oa.StartDate >= @startFrom)
                                         AND (@startTo IS NULL OR oa.StartDate <= @startTo)
                                         AND (@endFrom IS NULL OR oa.EndDate >= @endFrom)
                                         AND (@endTo IS NULL OR oa.EndDate <= @endTo)
                                       """;

    public async Task<Result<RentalContractDto, Errors>> GetContractDataAsync(
        OrderId orderId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var sql = """
                      SELECT
                          o.id AS OrderId,
                          o.number AS ContractNumber,
                          o.order_date AS ContractDate,

                          CONCAT_WS(' ', u.last_name, u.first_name, u.middle_name) AS UserFullName,

                          CONCAT_WS(' ', c.last_name, c.first_name, c.middle_name) AS CustomerFullName,
                          CONCAT_WS(' ', c.passport_series, c.passport_number) AS CustomerPassport,
                          CONCAT(
                          c.postal_code,
                          N', ',
                          c.region,
                          N', ',
                          c.city,
                          N', ',
                          c.street,
                          N', д. ',
                          c.house,
                          CASE
                              WHEN c.building IS NULL OR LTRIM(RTRIM(c.building)) = N''
                                  THEN N''
                              ELSE CONCAT(N', корп. ', c.building)
                          END,
                          CASE
                              WHEN c.apartment IS NULL OR LTRIM(RTRIM(c.apartment)) = N''
                                  THEN N''
                              ELSE CONCAT(N', кв./офис ', c.apartment)
                          END
                      ) AS CustomerAddress,
                          c.phone_number AS CustomerPhone,
                          c.email AS CustomerEmail,

                          ISNULL(oa.TotalRentalPrice, 0) AS TotalRentalPrice,
                          ISNULL(oa.TotalDeposit, 0) AS TotalDeposit,
                          ISNULL(oa.TotalRentalPrice, 0) + ISNULL(oa.TotalDeposit, 0) AS TotalAmount

                      FROM orders o
                      INNER JOIN customers c ON c.id = o.customer_id
                      INNER JOIN users u ON u.id = o.user_id
                      OUTER APPLY
                      (
                          SELECT
                              SUM(
                                  oi.rental_price_per_day *
                                  DATEDIFF(DAY, oi.start_date, oi.planned_return_date)
                              ) AS TotalRentalPrice,
                              SUM(oi.deposit_amount) AS TotalDeposit
                          FROM order_items oi
                          WHERE oi.order_id = o.id
                            AND oi.deleted_at IS NULL
                      ) oa
                      WHERE o.id = @orderId
                        AND o.deleted_at IS NULL
                        AND c.deleted_at IS NULL
                        AND u.deleted_at IS NULL
                      """;

            var data = await dbContext.Database
                .SqlQueryRaw<RentalContractSqlDto>(
                    sql,
                    CreateParameter("@orderId", orderId.Value))
                .FirstOrDefaultAsync(cancellationToken);

            if (data is null)
            {
                return CommonErrors.NotFound(
                        "contract.was.not.found",
                        "Договор не найден",
                        orderId.Value)
                    .ToErrors();
            }

            var contract = new RentalContractDto(
                OrderId: data.OrderId,
                ContractNumber: data.ContractNumber,
                ContractDate: DateOnly.FromDateTime(data.ContractDate),
                UserFullName: data.UserFullName,
                Customer: new CustomerContractDto(
                    FullName: data.CustomerFullName,
                    Passport: data.CustomerPassport,
                    Address: data.CustomerAddress,
                    Phone: data.CustomerPhone,
                    Email: data.CustomerEmail),
                TotalRentalPrice: data.TotalRentalPrice,
                TotalDeposit: data.TotalDeposit,
                TotalAmount: data.TotalAmount);

            return contract;
        }
        catch (OperationCanceledException)
        {
            return CommonErrors
                .OperationCancelled("get.contract.data.was.cancelled")
                .ToErrors();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to get rental contract data by order id {OrderId}",
                orderId.Value);

            return CommonErrors.Db(
                    "get.contract.data.from.db.exception",
                    "Failed to get rental contract data")
                .ToErrors();
        }
    }

    public async Task<Result<CloseRentalCalculationDto, Errors>> GetCloseRentalCalculationAsync(
        OrderId orderId,
        CancellationToken cancellationToken)
    {
        try
        {
            var sql = """
                      SELECT
                          ISNULL(totals.PlannedRentalAmount, 0) AS PlannedRentalAmount,
                          ISNULL(totals.ActualRentalAmount, 0) AS ActualRentalAmount,

                          ISNULL(payments.PaidRentalAmount, 0) AS PaidRentalAmount,
                          ISNULL(payments.PaidDepositAmount, 0) AS PaidDepositAmount,

                          ISNULL(payments.RefundedRentalAmount, 0) AS RefundedRentalAmount,
                          ISNULL(payments.RefundedDepositAmount, 0) AS RefundedDepositAmount,

                          ISNULL(totals.ActualRentalAmount, 0)
                          - ISNULL(payments.PaidRentalAmount, 0)
                          + ISNULL(payments.RefundedRentalAmount, 0) AS RentalBalanceAmount,

                          CASE
                              WHEN ISNULL(totals.ActualRentalAmount, 0)
                                   - ISNULL(payments.PaidRentalAmount, 0)
                                   + ISNULL(payments.RefundedRentalAmount, 0) < 0
                                  THEN ABS(
                                      ISNULL(totals.ActualRentalAmount, 0)
                                      - ISNULL(payments.PaidRentalAmount, 0)
                                      + ISNULL(payments.RefundedRentalAmount, 0)
                                  )
                              ELSE 0
                          END AS RentalRefundAmount,

                          CASE
                              WHEN ISNULL(totals.ActualRentalAmount, 0)
                                   - ISNULL(payments.PaidRentalAmount, 0)
                                   + ISNULL(payments.RefundedRentalAmount, 0) > 0
                                  THEN ISNULL(totals.ActualRentalAmount, 0)
                                       - ISNULL(payments.PaidRentalAmount, 0)
                                       + ISNULL(payments.RefundedRentalAmount, 0)
                              ELSE 0
                          END AS RentalAdditionalPaymentAmount,

                          CASE
                              WHEN ISNULL(payments.PaidDepositAmount, 0)
                                   - ISNULL(payments.RefundedDepositAmount, 0) < 0
                                  THEN 0
                              ELSE ISNULL(payments.PaidDepositAmount, 0)
                                   - ISNULL(payments.RefundedDepositAmount, 0)
                          END AS DepositRefundAmount

                      FROM orders o

                      OUTER APPLY
                      (
                          SELECT
                              SUM(
                                  oi.rental_price_per_day *
                                  CASE
                                      WHEN DATEDIFF(DAY, oi.start_date, oi.planned_return_date) <= 0
                                          THEN 1
                                      ELSE DATEDIFF(DAY, oi.start_date, oi.planned_return_date)
                                  END
                              ) AS PlannedRentalAmount,

                              SUM(
                          oi.rental_price_per_day *
                          CASE
                              WHEN oi.actual_returned_date IS NULL
                                  THEN
                                      CASE
                                          WHEN DATEDIFF(DAY, oi.start_date, oi.planned_return_date) <= 0
                                              THEN 1
                                          ELSE DATEDIFF(DAY, oi.start_date, oi.planned_return_date)
                                      END
                              ELSE
                                  CASE
                                      WHEN DATEDIFF(DAY, oi.start_date, oi.actual_returned_date) < 0
                                          THEN 0
                                      ELSE DATEDIFF(DAY, oi.start_date, oi.actual_returned_date)
                                  END
                          END
                      ) AS ActualRentalAmount
                          FROM order_items oi
                          WHERE oi.order_id = o.id
                            AND oi.deleted_at IS NULL
                      ) totals

                      OUTER APPLY
                      (
                          SELECT
                              SUM(CASE WHEN pt.name = N'Аренда' THEN p.amount ELSE 0 END) AS PaidRentalAmount,
                              SUM(CASE WHEN pt.name = N'Залог' THEN p.amount ELSE 0 END) AS PaidDepositAmount,
                              SUM(CASE WHEN pt.name = N'Возврат аренды' THEN p.amount ELSE 0 END) AS RefundedRentalAmount,
                              SUM(CASE WHEN pt.name = N'Возврат залога' THEN p.amount ELSE 0 END) AS RefundedDepositAmount
                          FROM payments p
                          INNER JOIN payment_types pt ON pt.id = p.payment_type_id
                          WHERE p.order_id = o.id
                            AND p.deleted_at IS NULL
                            AND pt.deleted_at IS NULL
                      ) payments

                      WHERE o.id = @orderId
                        AND o.deleted_at IS NULL
                      """;

            var calculation = await dbContext.Database
                .SqlQueryRaw<CloseRentalCalculationDto>(
                    sql,
                    CreateParameter("@orderId", orderId.Value))
                .FirstOrDefaultAsync(cancellationToken);

            if (calculation is null)
            {
                return CommonErrors.NotFound(
                        "order.not.found",
                        "Заказ не найден",
                        orderId.Value)
                    .ToErrors();
            }

            return calculation;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to get close rental calculation for order {OrderId}",
                orderId.Value);

            return CommonErrors.Db(
                    "get.close.rental.calculation.from.db.exception",
                    "Failed to get close rental calculation")
                .ToErrors();
        }
    }
}