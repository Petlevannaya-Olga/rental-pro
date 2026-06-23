using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using CSharpFunctionalExtensions;
using RentalPro.Contracts.Customers;
using RentalPro.Shared;

namespace RentalPro.Presentation.Desktop.Api;

public sealed class CustomersApiClient(IHttpClientFactory httpClientFactory)
{
    public async Task<Result<PagedResult<CustomerDto>, Errors>> GetCustomersAsync(
        GetCustomersRequest request,
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

        if (request.HasOrders.HasValue)
            parameters["hasOrders"] = request.HasOrders.Value.ToString();

        if (request.HasActiveOrders.HasValue)
            parameters["hasActiveOrders"] = request.HasActiveOrders.Value.ToString();

        if (request.IsRegular.HasValue)
            parameters["isRegular"] = request.IsRegular.Value.ToString();

        var url = BuildUrl("api/customers", parameters);

        try
        {
            var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await ReadErrorsAsync(
                    response,
                    "customers.get.failed",
                    "Не удалось загрузить список клиентов",
                    cancellationToken);
            }

            var result = await response.Content
                .ReadFromJsonAsync<PagedResult<CustomerDto>>(cancellationToken);

            if (result is null)
            {
                return CommonErrors
                    .Failure(
                        "customers.empty.response",
                        "Сервер вернул пустой список клиентов")
                    .ToErrors();
            }

            return result;
        }
        catch (HttpRequestException)
        {
            return CommonErrors
                .Failure(
                    "customers.connection.failed",
                    "Не удалось подключиться к серверу")
                .ToErrors();
        }
    }

    public async Task<Result<CustomerStatsDto, Errors>> GetStatsAsync(
        CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient("Api");

        try
        {
            var response = await httpClient.GetAsync(
                "api/customers/stats",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await ReadErrorsAsync(
                    response,
                    "customers.stats.failed",
                    "Не удалось загрузить статистику клиентов",
                    cancellationToken);
            }

            var result = await response.Content
                .ReadFromJsonAsync<CustomerStatsDto>(cancellationToken);

            if (result is null)
            {
                return CommonErrors
                    .Failure(
                        "customers.empty.stats",
                        "Сервер вернул пустую статистику клиентов")
                    .ToErrors();
            }

            return result;
        }
        catch (HttpRequestException)
        {
            return CommonErrors
                .Failure(
                    "customers.connection.failed",
                    "Не удалось подключиться к серверу")
                .ToErrors();
        }
    }

    public async Task<Result<CreateCustomerResponse, Errors>> CreateCustomerAsync(
        CreateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient("Api");

        try
        {
            var response = await httpClient.PostAsJsonAsync(
                "api/customers",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await ReadErrorsAsync(
                    response,
                    "customers.create.failed",
                    "Не удалось создать клиента",
                    cancellationToken);
            }

            var createdCustomer = await response.Content
                .ReadFromJsonAsync<CreateCustomerResponse>(cancellationToken);

            if (createdCustomer is null)
            {
                return CommonErrors
                    .Failure(
                        "customers.create.empty.response",
                        "Сервер не вернул данные созданного клиента")
                    .ToErrors();
            }

            return createdCustomer;
        }
        catch (HttpRequestException)
        {
            return CommonErrors
                .Failure(
                    "customers.connection.failed",
                    "Не удалось подключиться к серверу")
                .ToErrors();
        }
    }

    public async Task<UnitResult<Errors>> UpdateCustomerAsync(
        Guid id,
        UpdateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient("Api");

        try
        {
            var response = await httpClient.PutAsJsonAsync(
                $"api/customers/{id}",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await ReadErrorsAsync(
                    response,
                    "customers.update.failed",
                    "Не удалось обновить клиента",
                    cancellationToken);
            }

            return UnitResult.Success<Errors>();
        }
        catch (HttpRequestException)
        {
            return CommonErrors
                .Failure(
                    "customers.connection.failed",
                    "Не удалось подключиться к серверу")
                .ToErrors();
        }
    }

    public async Task<Result<byte[], Errors>> ExportCustomersAsync(
        ExportCustomersRequest request,
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

        if (request.HasOrders.HasValue)
            parameters["hasOrders"] = request.HasOrders.Value.ToString();

        if (request.HasActiveOrders.HasValue)
            parameters["hasActiveOrders"] = request.HasActiveOrders.Value.ToString();

        if (request.IsRegular.HasValue)
            parameters["isRegular"] = request.IsRegular.Value.ToString();

        var url = BuildUrl("api/customers/export", parameters);

        try
        {
            var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await ReadErrorsAsync(
                    response,
                    "customers.export.failed",
                    "Не удалось экспортировать клиентов",
                    cancellationToken);
            }

            var fileBytes = await response.Content
                .ReadAsByteArrayAsync(cancellationToken);

            return fileBytes;
        }
        catch (HttpRequestException)
        {
            return CommonErrors
                .Failure(
                    "customers.connection.failed",
                    "Не удалось подключиться к серверу")
                .ToErrors();
        }
    }

    public async Task<UnitResult<Errors>> DeleteCustomerAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient("Api");

        try
        {
            var response = await httpClient.DeleteAsync(
                $"api/customers/{id}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await ReadErrorsAsync(
                    response,
                    "customers.delete.failed",
                    "Не удалось удалить клиента",
                    cancellationToken);
            }

            return UnitResult.Success<Errors>();
        }
        catch (HttpRequestException)
        {
            return CommonErrors
                .Failure(
                    "customers.connection.failed",
                    "Не удалось подключиться к серверу")
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
    
    public async Task<Result<List<CustomerOrderHistoryItemDto>, Errors>> GetOrderHistoryAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient("Api");

        try
        {
            var response = await httpClient.GetAsync(
                $"api/customers/{customerId}/order-history",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await ReadErrorsAsync(
                    response,
                    "customer.order.history.load.failed",
                    "Не удалось загрузить историю заказов клиента",
                    cancellationToken);
            }

            var history = await response.Content
                .ReadFromJsonAsync<List<CustomerOrderHistoryItemDto>>(cancellationToken);

            return history ?? [];
        }
        catch (HttpRequestException)
        {
            return CommonErrors
                .Failure(
                    "customer.order.history.connection.failed",
                    "Не удалось подключиться к серверу")
                .ToErrors();
        }
    }
}