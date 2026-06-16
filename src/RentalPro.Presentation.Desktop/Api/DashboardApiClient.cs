using System.Net.Http;
using System.Net.Http.Json;
using RentalPro.Contracts.Dashboard;

namespace RentalPro.Presentation.Desktop.Api;

public sealed class DashboardApiClient(IHttpClientFactory httpClientFactory)
{
    public async Task<DashboardDto?> GetDashboardAsync(
        CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient("Api");

        return await httpClient.GetFromJsonAsync<DashboardDto>(
            "api/dashboard",
            cancellationToken);
    }
}