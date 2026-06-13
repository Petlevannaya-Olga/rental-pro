using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using RentalPro.Contracts.Dashboard;
using RentalPro.Shared;

namespace RentalPro.Presentation.Client.Services;

public sealed class DashboardService(HttpClient httpClient)
{
    public async Task<Result<DashboardDto, Errors>> GetDashboardAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync(
                "api/dashboard",
                cancellationToken);

            if (response.IsSuccessStatusCode is false)
            {
                var errors = await response.Content
                    .ReadFromJsonAsync<Errors>(cancellationToken);

                return errors ?? CommonErrors
                    .LoadFailed(
                        "dashboard.load.failed",
                        "Не удалось загрузить данные дашборда")
                    .ToErrors();
            }

            var dashboard = await response.Content
                .ReadFromJsonAsync<DashboardDto>(cancellationToken);

            if (dashboard is null)
            {
                return CommonErrors
                    .EmptyResponse(
                        "dashboard.empty.response",
                        "Сервер вернул пустой ответ")
                    .ToErrors();
            }

            return dashboard;
        }
        catch (OperationCanceledException)
        {
            return CommonErrors
                .OperationCancelled("dashboard.load.was.cancelled")
                .ToErrors();
        }
        catch
        {
            return CommonErrors
                .LoadFailed(
                    "dashboard.load.failed",
                    "Не удалось загрузить данные дашборда")
                .ToErrors();
        }
    }
}