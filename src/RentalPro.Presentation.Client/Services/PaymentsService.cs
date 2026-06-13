using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using Microsoft.JSInterop;
using RentalPro.Contracts.Payments;
using RentalPro.Presentation.Client.Extensions;
using RentalPro.Shared;

namespace RentalPro.Presentation.Client.Services;

public sealed class PaymentsService(
    HttpClient httpClient,
    IJSRuntime jsRuntime)
{
    public async Task<Result<CreatePaymentResponse, Errors>> CreatePaymentAsync(
        CreatePaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                "api/payments",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось принять оплату",
                    cancellationToken);

                return CommonErrors.CreateFailed(
                        "payment.create.failed",
                        message)
                    .ToErrors();
            }

            var payment = await response.Content
                .ReadFromJsonAsync<CreatePaymentResponse>(cancellationToken);

            if (payment is null)
            {
                return CommonErrors.EmptyResponse(
                        "payment.create.empty.response",
                        "Сервер не вернул данные платежа")
                    .ToErrors();
            }

            return payment;
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "payment.create.failed",
                "Не удалось принять оплату");
        }
    }

    private static async Task<string> ReadErrorMessageAsync(
        HttpResponseMessage response,
        string defaultMessage,
        CancellationToken cancellationToken)
    {
        var content = await response.Content
            .ReadAsStringAsync(cancellationToken);

        return content.ExtractErrorMessage(defaultMessage);
    }
    
     public async Task<Result<PagedResult<PaymentDto>, Errors>> GetPaymentsAsync(
        GetPaymentsRequest filter,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQueryString(filter);

        var response = await httpClient.GetAsync(
            $"api/payments{query}",
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errors = await response.Content
                .ReadFromJsonAsync<Errors>(cancellationToken);

            return errors ?? CommonErrors
                .Failure("payments.get.failed", "Не удалось загрузить оплаты")
                .ToErrors();
        }

        var result = await response.Content
            .ReadFromJsonAsync<PagedResult<PaymentDto>>(cancellationToken);

        if (result is null)
        {
            return CommonErrors
                .Failure("payments.get.empty.response", "Сервер вернул пустой ответ")
                .ToErrors();
        }

        return result;
    }
     
    public async Task<Result<PaymentStatsDto, Errors>> GetStatsAsync(
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync(
            "api/payments/stats",
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errors = await response.Content
                .ReadFromJsonAsync<Errors>(cancellationToken);

            return errors ?? CommonErrors
                .Failure("payments.stats.get.failed", "Не удалось загрузить статистику оплат")
                .ToErrors();
        }

        var result = await response.Content
            .ReadFromJsonAsync<PaymentStatsDto>(cancellationToken);

        if (result is null)
        {
            return CommonErrors
                .Failure("payments.stats.get.empty.response", "Сервер вернул пустой ответ")
                .ToErrors();
        }

        return result;
    }
    
    public async Task<UnitResult<Errors>> ExportPaymentsAsync(
        ExportPaymentsRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = BuildExportQueryString(request);

        var response = await httpClient.GetAsync(
            $"api/payments/export{query}",
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errors = await response.Content
                .ReadFromJsonAsync<Errors>(cancellationToken);

            return errors ?? CommonErrors
                .Failure("payments.export.failed", "Не удалось экспортировать оплаты")
                .ToErrors();
        }

        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

        await jsRuntime.InvokeVoidAsync(
            "downloadFile",
            "payments.xlsx",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            bytes);

        return UnitResult.Success<Errors>();
    }

    private static string BuildExportQueryString(ExportPaymentsRequest request)
    {
        var parameters = new List<string>();

        Add(parameters, "search", request.Search);
        Add(parameters, "paymentTypeId", request.PaymentTypeId);
        Add(parameters, "paymentMethodId", request.PaymentMethodId);
        Add(parameters, "dateFrom", request.DateFrom);
        Add(parameters, "dateTo", request.DateTo);
        Add(parameters, "sortBy", request.SortBy);

        parameters.Add($"descending={request.Descending.ToString().ToLowerInvariant()}");

        return parameters.Count == 0
            ? string.Empty
            : "?" + string.Join("&", parameters);
    }
    
    private static string BuildQueryString(GetPaymentsRequest filter)
    {
        var parameters = new List<string>();

        Add(parameters, "search", filter.Search);
        Add(parameters, "paymentTypeId", filter.PaymentTypeId);
        Add(parameters, "paymentMethodId", filter.PaymentMethodId);
        Add(parameters, "dateFrom", filter.DateFrom);
        Add(parameters, "dateTo", filter.DateTo);
        Add(parameters, "sortBy", filter.SortBy);

        parameters.Add($"descending={filter.Descending.ToString().ToLowerInvariant()}");
        parameters.Add($"page={filter.Page}");
        parameters.Add($"pageSize={filter.PageSize}");

        return parameters.Count == 0
            ? string.Empty
            : "?" + string.Join("&", parameters);
    }

    private static void Add(
        List<string> parameters,
        string name,
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        parameters.Add($"{name}={Uri.EscapeDataString(value)}");
    }

    private static void Add(
        List<string> parameters,
        string name,
        Guid? value)
    {
        if (!value.HasValue)
            return;

        parameters.Add($"{name}={value.Value}");
    }

    private static void Add(
        List<string> parameters,
        string name,
        DateTime? value)
    {
        if (!value.HasValue)
            return;

        parameters.Add($"{name}={value.Value:yyyy-MM-dd}");
    }
}