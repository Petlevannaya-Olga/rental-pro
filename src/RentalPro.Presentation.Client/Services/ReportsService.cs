using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using Microsoft.JSInterop;
using RentalPro.Contracts.Reports;
using RentalPro.Shared;

namespace RentalPro.Presentation.Client.Services;

public sealed class ReportsService(HttpClient httpClient, IJSRuntime jsRuntime)
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

    public Task<UnitResult<Errors>> ExportRevenueAsync(
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken cancellationToken = default)
    {
        var url = $"api/reports/revenue/export?dateFrom={FormatDate(dateFrom)}&dateTo={FormatDate(dateTo)}";

        return ExportAsync(
            url,
            "revenue-report.xlsx",
            "Не удалось экспортировать отчет по выручке",
            cancellationToken);
    }

    public Task<UnitResult<Errors>> ExportPopularToolsAsync(
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken cancellationToken = default)
    {
        var url = $"api/reports/popular-tools/export?dateFrom={FormatDate(dateFrom)}&dateTo={FormatDate(dateTo)}";

        return ExportAsync(
            url,
            "popular-tools-report.xlsx",
            "Не удалось экспортировать отчет по популярности инструментов",
            cancellationToken);
    }

    public Task<UnitResult<Errors>> ExportOverdueReturnsAsync(
        CancellationToken cancellationToken = default)
    {
        return ExportAsync(
            "api/reports/overdue-returns/export",
            "overdue-returns-report.xlsx",
            "Не удалось экспортировать отчет по просроченным возвратам",
            cancellationToken);
    }

    public Task<UnitResult<Errors>> ExportToolsAsync(
        CancellationToken cancellationToken = default)
    {
        return ExportAsync(
            "api/reports/tools/export",
            "tools-report.xlsx",
            "Не удалось экспортировать отчет по инструментам",
            cancellationToken);
    }

    public Task<UnitResult<Errors>> ExportCustomersAsync(
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken cancellationToken = default)
    {
        var url = $"api/reports/customers/export?dateFrom={FormatDate(dateFrom)}&dateTo={FormatDate(dateTo)}";

        return ExportAsync(
            url,
            "customers-report.xlsx",
            "Не удалось экспортировать отчет по клиентам",
            cancellationToken);
    }

    public Task<UnitResult<Errors>> ExportPaymentsAsync(
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken cancellationToken = default)
    {
        var url = $"api/reports/payments/export?dateFrom={FormatDate(dateFrom)}&dateTo={FormatDate(dateTo)}";

        return ExportAsync(
            url,
            "payments-report.xlsx",
            "Не удалось экспортировать отчет по платежам",
            cancellationToken);
    }

    private async Task<UnitResult<Errors>> ExportAsync(
        string url,
        string fileName,
        string errorMessage,
        CancellationToken cancellationToken)
    {
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

            var bytes = await response.Content
                .ReadAsByteArrayAsync(cancellationToken);

            await jsRuntime.InvokeVoidAsync(
                "downloadFile",
                fileName,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                bytes);

            return UnitResult.Success<Errors>();
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
        try
        {
            var response = await httpClient.GetAsync(
                url,
                cancellationToken);

            if (response.IsSuccessStatusCode is false)
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