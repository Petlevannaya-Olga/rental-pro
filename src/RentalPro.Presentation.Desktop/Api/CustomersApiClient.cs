using System.Net.Http;
using System.Net.Http.Json;
using RentalPro.Contracts.Customers;
using RentalPro.Shared;

namespace RentalPro.Presentation.Desktop.Api;

public sealed class CustomersApiClient(IHttpClientFactory httpClientFactory)
{
    public async Task<PagedResult<CustomerDto>> GetCustomersAsync(
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

        var response = await httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new Exception(await response.Content.ReadAsStringAsync(cancellationToken));

        return await response.Content.ReadFromJsonAsync<PagedResult<CustomerDto>>(cancellationToken)
               ?? throw new Exception("Сервер вернул пустой список клиентов");
    }

    public async Task<CustomerStatsDto> GetStatsAsync(
        CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient("Api");

        return await httpClient.GetFromJsonAsync<CustomerStatsDto>(
                   "api/customers/stats",
                   cancellationToken)
               ?? throw new Exception("Сервер вернул пустую статистику клиентов");
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
}