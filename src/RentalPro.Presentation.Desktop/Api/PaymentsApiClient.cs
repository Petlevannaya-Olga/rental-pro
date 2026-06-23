using System.Net.Http;
using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using RentalPro.Contracts.Payments;
using RentalPro.Shared;

namespace RentalPro.Presentation.Desktop.Api;

public sealed class PaymentsApiClient(IHttpClientFactory httpClientFactory)
{
    public async Task<Result<CreatePaymentResponse, Errors>> CreatePaymentAsync(
        CreatePaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient("Api");

        try
        {
            var response = await httpClient.PostAsJsonAsync(
                "api/payments",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await ReadErrorsAsync(
                    response,
                    "payment.create.failed",
                    "Не удалось принять оплату",
                    cancellationToken);
            }

            var payment = await response.Content
                .ReadFromJsonAsync<CreatePaymentResponse>(cancellationToken);

            if (payment is null)
            {
                return CommonErrors
                    .EmptyResponse(
                        "payment.create.empty.response",
                        "Сервер не вернул данные платежа")
                    .ToErrors();
            }

            return payment;
        }
        catch (HttpRequestException)
        {
            return CommonErrors
                .Failure(
                    "payments.connection.failed",
                    "Не удалось подключиться к серверу")
                .ToErrors();
        }
    }

    public async Task<Result<PagedResult<PaymentDto>, Errors>> GetPaymentsAsync(
        GetPaymentsRequest request,
        CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient("Api");

        var url = BuildUrl("api/payments", new Dictionary<string, string?>
        {
            ["search"] = request.Search,
            ["paymentTypeId"] = request.PaymentTypeId?.ToString(),
            ["paymentMethodId"] = request.PaymentMethodId?.ToString(),
            ["dateFrom"] = request.DateFrom?.ToString("yyyy-MM-dd"),
            ["dateTo"] = request.DateTo?.ToString("yyyy-MM-dd"),
            ["sortBy"] = request.SortBy,
            ["descending"] = request.Descending.ToString(),
            ["page"] = request.Page.ToString(),
            ["pageSize"] = request.PageSize.ToString()
        });

        try
        {
            var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await ReadErrorsAsync(
                    response,
                    "payments.get.failed",
                    "Не удалось загрузить оплаты",
                    cancellationToken);
            }

            var result = await response.Content
                .ReadFromJsonAsync<PagedResult<PaymentDto>>(cancellationToken);

            if (result is null)
            {
                return CommonErrors
                    .EmptyResponse(
                        "payments.get.empty.response",
                        "Сервер вернул пустой список оплат")
                    .ToErrors();
            }

            return result;
        }
        catch (HttpRequestException)
        {
            return CommonErrors
                .Failure(
                    "payments.connection.failed",
                    "Не удалось подключиться к серверу")
                .ToErrors();
        }
    }

    public async Task<Result<PaymentStatsDto, Errors>> GetStatsAsync(
        CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient("Api");

        try
        {
            var response = await httpClient.GetAsync(
                "api/payments/stats",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await ReadErrorsAsync(
                    response,
                    "payments.stats.get.failed",
                    "Не удалось загрузить статистику оплат",
                    cancellationToken);
            }

            var result = await response.Content
                .ReadFromJsonAsync<PaymentStatsDto>(cancellationToken);

            if (result is null)
            {
                return CommonErrors
                    .EmptyResponse(
                        "payments.stats.get.empty.response",
                        "Сервер вернул пустую статистику оплат")
                    .ToErrors();
            }

            return result;
        }
        catch (HttpRequestException)
        {
            return CommonErrors
                .Failure(
                    "payments.connection.failed",
                    "Не удалось подключиться к серверу")
                .ToErrors();
        }
    }

    public async Task<Result<byte[], Errors>> ExportPaymentsAsync(
        ExportPaymentsRequest request,
        CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient("Api");

        var url = BuildUrl("api/payments/export", new Dictionary<string, string?>
        {
            ["search"] = request.Search,
            ["paymentTypeId"] = request.PaymentTypeId?.ToString(),
            ["paymentMethodId"] = request.PaymentMethodId?.ToString(),
            ["dateFrom"] = request.DateFrom?.ToString("yyyy-MM-dd"),
            ["dateTo"] = request.DateTo?.ToString("yyyy-MM-dd"),
            ["sortBy"] = request.SortBy,
            ["descending"] = request.Descending.ToString()
        });

        try
        {
            var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await ReadErrorsAsync(
                    response,
                    "payments.export.failed",
                    "Не удалось экспортировать оплаты",
                    cancellationToken);
            }

            return await response.Content
                .ReadAsByteArrayAsync(cancellationToken);
        }
        catch (HttpRequestException)
        {
            return CommonErrors
                .Failure(
                    "payments.connection.failed",
                    "Не удалось подключиться к серверу")
                .ToErrors();
        }
    }

    private static async Task<Errors> ReadErrorsAsync(
        HttpResponseMessage response,
        string fallbackCode,
        string fallbackMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            var errors = await response.Content
                .ReadFromJsonAsync<Errors>(cancellationToken);

            if (errors is not null)
                return errors;
        }
        catch
        {
            // ignored
        }

        return CommonErrors
            .Failure(fallbackCode, fallbackMessage)
            .ToErrors();
    }

    private static string BuildUrl(
        string path,
        Dictionary<string, string?> parameters)
    {
        var query = parameters
            .Where(x => !string.IsNullOrWhiteSpace(x.Value))
            .Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value!)}")
            .ToList();

        return query.Count == 0
            ? path
            : $"{path}?{string.Join("&", query)}";
    }
}