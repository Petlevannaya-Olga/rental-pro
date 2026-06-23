using System.Net.Http;
using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using RentalPro.Contracts.Orders;
using RentalPro.Shared;

namespace RentalPro.Presentation.Desktop.Api;

public sealed class OrdersApiClient(IHttpClientFactory httpClientFactory)
{
    public async Task<Result<PagedResult<OrderDto>, Errors>> GetOrdersAsync(
        GetOrdersRequest request,
        CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient("Api");

        var parameters = new Dictionary<string, string?>
        {
            ["sortBy"] = request.SortBy,
            ["descending"] = request.Descending.ToString(),
            ["page"] = request.Page.ToString(),
            ["pageSize"] = request.PageSize.ToString()
        };

        if (!string.IsNullOrWhiteSpace(request.Search))
            parameters["search"] = request.Search.Trim();

        if (request.StatusId.HasValue)
            parameters["statusId"] = request.StatusId.Value.ToString();

        if (request.StartFrom.HasValue)
            parameters["startFrom"] = request.StartFrom.Value.ToString("yyyy-MM-dd");

        if (request.StartTo.HasValue)
            parameters["startTo"] = request.StartTo.Value.ToString("yyyy-MM-dd");

        if (request.EndFrom.HasValue)
            parameters["endFrom"] = request.EndFrom.Value.ToString("yyyy-MM-dd");

        if (request.EndTo.HasValue)
            parameters["endTo"] = request.EndTo.Value.ToString("yyyy-MM-dd");

        var url = BuildUrl("api/orders", parameters);

        try
        {
            var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await ReadErrorsAsync(
                    response,
                    "orders.load.failed",
                    "Не удалось получить список заказов",
                    cancellationToken);
            }

            var orders = await response.Content
                .ReadFromJsonAsync<PagedResult<OrderDto>>(cancellationToken);

            if (orders is null)
            {
                return CommonErrors
                    .EmptyResponse(
                        "orders.empty.response",
                        "Сервер вернул пустой список заказов")
                    .ToErrors();
            }

            return orders;
        }
        catch (HttpRequestException)
        {
            return CommonErrors
                .Failure(
                    "orders.load.failed",
                    "Не удалось получить список заказов")
                .ToErrors();
        }
    }

    public async Task<Result<OrderStatsDto, Errors>> GetStatsAsync(
        CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient("Api");

        try
        {
            var response = await httpClient.GetAsync(
                "api/orders/stats",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await ReadErrorsAsync(
                    response,
                    "orders.stats.load.failed",
                    "Не удалось получить статистику заказов",
                    cancellationToken);
            }

            var stats = await response.Content
                .ReadFromJsonAsync<OrderStatsDto>(cancellationToken);

            if (stats is null)
            {
                return CommonErrors
                    .EmptyResponse(
                        "orders.stats.empty.response",
                        "Сервер вернул пустую статистику заказов")
                    .ToErrors();
            }

            return stats;
        }
        catch (HttpRequestException)
        {
            return CommonErrors
                .Failure(
                    "orders.stats.load.failed",
                    "Не удалось получить статистику заказов")
                .ToErrors();
        }
    }

    public async Task<Result<CreateOrderResponse, Errors>> CreateOrderAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient("Api");

        try
        {
            var response = await httpClient.PostAsJsonAsync(
                "api/orders",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await ReadErrorsAsync(
                    response,
                    "order.create.failed",
                    "Не удалось создать заказ",
                    cancellationToken);
            }

            var order = await response.Content
                .ReadFromJsonAsync<CreateOrderResponse>(cancellationToken);

            if (order is null)
            {
                return CommonErrors
                    .EmptyResponse(
                        "order.create.empty.response",
                        "Сервер не вернул данные созданного заказа")
                    .ToErrors();
            }

            return order;
        }
        catch (HttpRequestException)
        {
            return CommonErrors
                .Failure(
                    "order.create.failed",
                    "Не удалось создать заказ")
                .ToErrors();
        }
    }

    public async Task<Result<byte[], Errors>> ExportOrdersAsync(
        ExportOrdersRequest request,
        CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient("Api");

        var parameters = new Dictionary<string, string?>
        {
            ["sortBy"] = request.SortBy,
            ["descending"] = request.Descending.ToString()
        };

        if (!string.IsNullOrWhiteSpace(request.Search))
            parameters["search"] = request.Search.Trim();

        if (request.StatusId.HasValue)
            parameters["statusId"] = request.StatusId.Value.ToString();

        if (request.StartFrom.HasValue)
            parameters["startFrom"] = request.StartFrom.Value.ToString("yyyy-MM-dd");

        if (request.StartTo.HasValue)
            parameters["startTo"] = request.StartTo.Value.ToString("yyyy-MM-dd");

        if (request.EndFrom.HasValue)
            parameters["endFrom"] = request.EndFrom.Value.ToString("yyyy-MM-dd");

        if (request.EndTo.HasValue)
            parameters["endTo"] = request.EndTo.Value.ToString("yyyy-MM-dd");

        var url = BuildUrl("api/orders/export", parameters);

        try
        {
            var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await ReadErrorsAsync(
                    response,
                    "orders.export.failed",
                    "Не удалось выгрузить заказы",
                    cancellationToken);
            }

            return await response.Content
                .ReadAsByteArrayAsync(cancellationToken);
        }
        catch (HttpRequestException)
        {
            return CommonErrors
                .Failure(
                    "orders.export.failed",
                    "Не удалось выгрузить заказы")
                .ToErrors();
        }
    }

    private static string BuildUrl(
        string path,
        Dictionary<string, string?> parameters)
    {
        var query = parameters
            .Where(x => !string.IsNullOrWhiteSpace(x.Value))
            .Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value!)}");

        return $"{path}?{string.Join("&", query)}";
    }

    private static async Task<Errors> ReadErrorsAsync(
        HttpResponseMessage response,
        string fallbackCode,
        string fallbackMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            var errors = await response.Content.ReadFromJsonAsync<Error[]>(
                cancellationToken);

            if (errors is { Length: > 0 })
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

    public async Task<UnitResult<Errors>> CancelOrderAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient("Api");

        try
        {
            var response = await httpClient.PostAsync(
                $"api/orders/{orderId}/cancel",
                null,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await ReadErrorsAsync(
                    response,
                    "order.cancel.failed",
                    "Не удалось отменить заказ",
                    cancellationToken);
            }

            return UnitResult.Success<Errors>();
        }
        catch (HttpRequestException)
        {
            return CommonErrors
                .Failure(
                    "order.cancel.failed",
                    "Не удалось отменить заказ")
                .ToErrors();
        }
    }
    
    public async Task<Result<OrderDetailsDto, Errors>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient("Api");

        try
        {
            var response = await httpClient.GetAsync(
                $"api/orders/{id}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await ReadErrorsAsync(
                    response,
                    "order.load.failed",
                    "Не удалось загрузить заказ",
                    cancellationToken);
            }

            var order = await response.Content
                .ReadFromJsonAsync<OrderDetailsDto>(cancellationToken);

            if (order is null)
            {
                return CommonErrors
                    .Failure(
                        "order.details.is.null",
                        "Не удалось получить заказ")
                    .ToErrors();
            }

            return order;
        }
        catch (HttpRequestException)
        {
            return CommonErrors
                .Failure(
                    "order.load.failed",
                    "Не удалось загрузить заказ")
                .ToErrors();
        }
    }
    
    public async Task<Result<IReadOnlyList<OrderDocumentDto>, Errors>> GetDocumentsAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient("Api");

        try
        {
            var response = await httpClient.GetAsync(
                $"api/orders/{orderId}/documents",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await ReadErrorsAsync(
                    response,
                    "order.documents.load.failed",
                    "Не удалось загрузить документы заказа",
                    cancellationToken);
            }

            var documents = await response.Content
                .ReadFromJsonAsync<IReadOnlyList<OrderDocumentDto>>(cancellationToken);

            return Result.Success<IReadOnlyList<OrderDocumentDto>, Errors>(
                documents ?? []);
        }
        catch (HttpRequestException)
        {
            return CommonErrors
                .Failure(
                    "order.documents.load.failed",
                    "Не удалось загрузить документы заказа")
                .ToErrors();
        }
    }
    
    public async Task<Result<byte[], Errors>> ExportContractAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        return await DownloadFileAsync(
            $"api/orders/{orderId}/contract",
            "order.contract.download.failed",
            "Не удалось скачать договор",
            cancellationToken);
    }

    public async Task<Result<byte[], Errors>> ExportTransferActAsync(
        Guid orderId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        return await DownloadFileAsync(
            $"api/orders/{orderId}/transfer-act?date={date:yyyy-MM-dd}",
            "order.transfer-act.download.failed",
            "Не удалось скачать акт выдачи",
            cancellationToken);
    }

    public async Task<Result<byte[], Errors>> ExportReturnActAsync(
        Guid orderId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        return await DownloadFileAsync(
            $"api/orders/{orderId}/return-act?date={date:yyyy-MM-dd}",
            "order.return-act.download.failed",
            "Не удалось скачать акт возврата",
            cancellationToken);
    }

    private async Task<Result<byte[], Errors>> DownloadFileAsync(
        string url,
        string fallbackCode,
        string fallbackMessage,
        CancellationToken cancellationToken)
    {
        var httpClient = httpClientFactory.CreateClient("Api");

        try
        {
            var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await ReadErrorsAsync(
                    response,
                    fallbackCode,
                    fallbackMessage,
                    cancellationToken);
            }

            return await response.Content.ReadAsByteArrayAsync(cancellationToken);
        }
        catch (HttpRequestException)
        {
            return CommonErrors
                .Failure(fallbackCode, fallbackMessage)
                .ToErrors();
        }
    }
}