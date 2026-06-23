using System.Net.Http;
using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using RentalPro.Contracts.Reports;
using RentalPro.Shared;

namespace RentalPro.Presentation.Desktop.Api;

public sealed class ReportsApiClient(IHttpClientFactory httpClientFactory)
{
    public Task<Result<List<RevenueReportDto>, Errors>> GetRevenueAsync(
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken cancellationToken = default)
    {
        return GetListAsync<RevenueReportDto>(
            $"api/reports/revenue?dateFrom={FormatDate(dateFrom)}&dateTo={FormatDate(dateTo)}",
            "Не удалось загрузить отчет по выручке",
            cancellationToken);
    }

    public Task<Result<List<PopularToolReportDto>, Errors>> GetPopularToolsAsync(
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken cancellationToken = default)
    {
        return GetListAsync<PopularToolReportDto>(
            $"api/reports/popular-tools?dateFrom={FormatDate(dateFrom)}&dateTo={FormatDate(dateTo)}",
            "Не удалось загрузить отчет по популярности инструментов",
            cancellationToken);
    }

    public Task<Result<List<OverdueReturnReportDto>, Errors>> GetOverdueReturnsAsync(
        CancellationToken cancellationToken = default)
    {
        return GetListAsync<OverdueReturnReportDto>(
            "api/reports/overdue-returns",
            "Не удалось загрузить отчет по просроченным возвратам",
            cancellationToken);
    }

    public Task<Result<List<CustomerReportDto>, Errors>> GetCustomersAsync(
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken cancellationToken = default)
    {
        return GetListAsync<CustomerReportDto>(
            $"api/reports/customers?dateFrom={FormatDate(dateFrom)}&dateTo={FormatDate(dateTo)}",
            "Не удалось загрузить отчет по клиентам",
            cancellationToken);
    }

    public Task<Result<List<PaymentReportDto>, Errors>> GetPaymentsAsync(
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken cancellationToken = default)
    {
        return GetListAsync<PaymentReportDto>(
            $"api/reports/payments?dateFrom={FormatDate(dateFrom)}&dateTo={FormatDate(dateTo)}",
            "Не удалось загрузить отчет по платежам",
            cancellationToken);
    }

    public Task<Result<List<ToolReportDto>, Errors>> GetToolsAsync(
        CancellationToken cancellationToken = default)
    {
        return GetListAsync<ToolReportDto>(
            "api/reports/tools",
            "Не удалось загрузить отчет по инструментам",
            cancellationToken);
    }

    public Task<Result<byte[], Errors>> ExportRevenueAsync(
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken cancellationToken = default)
    {
        return ExportAsync(
            $"api/reports/revenue/export?dateFrom={FormatDate(dateFrom)}&dateTo={FormatDate(dateTo)}",
            "Не удалось экспортировать отчет по выручке",
            cancellationToken);
    }

    public Task<Result<byte[], Errors>> ExportPopularToolsAsync(
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken cancellationToken = default)
    {
        return ExportAsync(
            $"api/reports/popular-tools/export?dateFrom={FormatDate(dateFrom)}&dateTo={FormatDate(dateTo)}",
            "Не удалось экспортировать отчет по популярности инструментов",
            cancellationToken);
    }

    public Task<Result<byte[], Errors>> ExportOverdueReturnsAsync(
        CancellationToken cancellationToken = default)
    {
        return ExportAsync(
            "api/reports/overdue-returns/export",
            "Не удалось экспортировать отчет по просроченным возвратам",
            cancellationToken);
    }

    public Task<Result<byte[], Errors>> ExportToolsAsync(
        CancellationToken cancellationToken = default)
    {
        return ExportAsync(
            "api/reports/tools/export",
            "Не удалось экспортировать отчет по инструментам",
            cancellationToken);
    }

    public Task<Result<byte[], Errors>> ExportCustomersAsync(
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken cancellationToken = default)
    {
        return ExportAsync(
            $"api/reports/customers/export?dateFrom={FormatDate(dateFrom)}&dateTo={FormatDate(dateTo)}",
            "Не удалось экспортировать отчет по клиентам",
            cancellationToken);
    }

    public Task<Result<byte[], Errors>> ExportPaymentsAsync(
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken cancellationToken = default)
    {
        return ExportAsync(
            $"api/reports/payments/export?dateFrom={FormatDate(dateFrom)}&dateTo={FormatDate(dateTo)}",
            "Не удалось экспортировать отчет по платежам",
            cancellationToken);
    }

    private async Task<Result<byte[], Errors>> ExportAsync(
        string url,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        var httpClient = httpClientFactory.CreateClient("Api");

        try
        {
            var response = await httpClient.GetAsync(
                url,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errors = await response.Content
                    .ReadFromJsonAsync<Errors>(cancellationToken);

                return errors ?? CommonErrors
                    .Failure("report.export.failed", errorMessage)
                    .ToErrors();
            }

            return await response.Content
                .ReadAsByteArrayAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return CommonErrors
                .OperationCancelled("report.export.was.cancelled")
                .ToErrors();
        }
        catch
        {
            return CommonErrors
                .Failure("report.export.failed", errorMessage)
                .ToErrors();
        }
    }

    private async Task<Result<List<T>, Errors>> GetListAsync<T>(
        string url,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        var httpClient = httpClientFactory.CreateClient("Api");

        try
        {
            var response = await httpClient.GetAsync(
                url,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errors = await response.Content
                    .ReadFromJsonAsync<Errors>(cancellationToken);

                return errors ?? CommonErrors
                    .LoadFailed("report.load.failed", errorMessage)
                    .ToErrors();
            }

            var result = await response.Content
                .ReadFromJsonAsync<List<T>>(cancellationToken);

            if (result is null)
            {
                return CommonErrors
                    .EmptyResponse("report.empty.response", "Сервер вернул пустой ответ")
                    .ToErrors();
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            return CommonErrors
                .OperationCancelled("report.load.was.cancelled")
                .ToErrors();
        }
        catch
        {
            return CommonErrors
                .LoadFailed("report.load.failed", errorMessage)
                .ToErrors();
        }
    }

    private static string FormatDate(DateTime date)
    {
        return date.ToString("yyyy-MM-dd");
    }
}