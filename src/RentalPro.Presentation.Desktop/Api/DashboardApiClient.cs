using System.Net.Http;
using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using RentalPro.Contracts.Dashboard;
using RentalPro.Shared;

namespace RentalPro.Presentation.Desktop.Api;

public sealed class DashboardApiClient(IHttpClientFactory httpClientFactory)
{
    public async Task<Result<DashboardDto, Errors>> GetDashboardAsync(
        CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient("Api");

        try
        {
            var response = await httpClient.GetAsync(
                "api/dashboard",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await ReadErrorsAsync(
                    response,
                    "dashboard.get.failed",
                    "Не удалось загрузить данные панели управления",
                    cancellationToken);
            }

            var dashboard = await response.Content
                .ReadFromJsonAsync<DashboardDto>(cancellationToken);

            if (dashboard is null)
            {
                return CommonErrors
                    .Failure(
                        "dashboard.empty.response",
                        "Сервер вернул пустой ответ")
                    .ToErrors();
            }

            return dashboard;
        }
        catch (HttpRequestException)
        {
            return CommonErrors
                .Failure(
                    "dashboard.connection.failed",
                    "Не удалось подключиться к серверу")
                .ToErrors();
        }
    }
    
    public async Task<Result<DashboardStatsDto, Errors>> GetStatsAsync(
        CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient("Api");

        try
        {
            var response = await httpClient.GetAsync(
                "api/dashboard/stats",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await ReadErrorsAsync(
                    response,
                    "dashboard.stats.load.failed",
                    "Не удалось загрузить статистику",
                    cancellationToken);
            }

            var stats = await response.Content
                .ReadFromJsonAsync<DashboardStatsDto>(cancellationToken);

            if (stats is null)
            {
                return CommonErrors
                    .EmptyResponse(
                        "dashboard.stats.empty.response",
                        "Сервер вернул пустую статистику")
                    .ToErrors();
            }

            return stats;
        }
        catch (HttpRequestException)
        {
            return CommonErrors
                .Failure(
                    "dashboard.connection.failed",
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
}