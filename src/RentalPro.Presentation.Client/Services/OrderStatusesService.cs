using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using RentalPro.Contracts.OrderStatuses;
using RentalPro.Presentation.Client.Extensions;
using RentalPro.Shared;

namespace RentalPro.Presentation.Client.Services;

public sealed class OrderStatusesService(HttpClient httpClient)
{
    public async Task<Result<List<OrderStatusDto>, Errors>> GetOrderStatusesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync(
                "api/order-statuses",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось загрузить статусы заказов",
                    cancellationToken);

                return CommonErrors.LoadFailed(
                        "order.statuses.load.failed",
                        message)
                    .ToErrors();
            }

            var statuses = await response.Content
                .ReadFromJsonAsync<List<OrderStatusDto>>(cancellationToken);

            if (statuses is null)
            {
                return CommonErrors.EmptyResponse(
                        "order.statuses.empty.response",
                        "Сервер вернул пустой список статусов заказов")
                    .ToErrors();
            }

            return statuses;
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "order.statuses.load.failed",
                "Не удалось загрузить статусы заказов");
        }
    }

    public async Task<UnitResult<Errors>> CreateOrderStatusAsync(
        CreateOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                "api/order-statuses",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось создать статус заказа",
                    cancellationToken);

                return CommonErrors.CreateFailed(
                        "order.status.create.failed",
                        message)
                    .ToErrors();
            }

            return UnitResult.Success<Errors>();
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "order.status.create.failed",
                "Не удалось создать статус заказа");
        }
    }

    public async Task<UnitResult<Errors>> UpdateOrderStatusAsync(
        Guid id,
        UpdateOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync(
                $"api/order-statuses/{id}",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось обновить статус заказа",
                    cancellationToken);

                return CommonErrors.UpdateFailed(
                        "order.status.update.failed",
                        message)
                    .ToErrors();
            }

            return UnitResult.Success<Errors>();
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "order.status.update.failed",
                "Не удалось обновить статус заказа");
        }
    }

    public async Task<UnitResult<Errors>> DeleteOrderStatusAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.DeleteAsync(
                $"api/order-statuses/{id}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось удалить статус заказа",
                    cancellationToken);

                return CommonErrors.DeleteFailed(
                        "order.status.delete.failed",
                        message)
                    .ToErrors();
            }

            return UnitResult.Success<Errors>();
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "order.status.delete.failed",
                "Не удалось удалить статус заказа");
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
}