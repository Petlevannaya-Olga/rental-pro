using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using RentalPro.Contracts.Orders;
using RentalPro.Presentation.Client.Extensions;
using RentalPro.Shared;

namespace RentalPro.Presentation.Client.Services;

public sealed class OrdersService(
    HttpClient httpClient,
    IJSRuntime jsRuntime)
{
    public async Task<Result<PagedResult<OrderDto>, Errors>> GetOrdersAsync(
        GetOrdersRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
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

            var url = QueryHelpers.AddQueryString(
                "api/orders",
                parameters);

            var response = await httpClient.GetAsync(
                url,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось получить список заказов",
                    cancellationToken);

                return CommonErrors.LoadFailed(
                        "orders.load.failed",
                        message)
                    .ToErrors();
            }

            var orders = await response.Content
                .ReadFromJsonAsync<PagedResult<OrderDto>>(cancellationToken);

            if (orders is null)
            {
                return CommonErrors.EmptyResponse(
                        "orders.empty.response",
                        "Сервер вернул пустой список заказов")
                    .ToErrors();
            }

            return orders;
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "orders.load.failed",
                "Не удалось получить список заказов");
        }
    }

    public async Task<Result<OrderStatsDto, Errors>> GetStatsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync(
                "api/orders/stats",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось получить статистику заказов",
                    cancellationToken);

                return CommonErrors.LoadFailed(
                        "orders.stats.load.failed",
                        message)
                    .ToErrors();
            }

            var stats = await response.Content
                .ReadFromJsonAsync<OrderStatsDto>(cancellationToken);

            if (stats is null)
            {
                return CommonErrors.EmptyResponse(
                        "orders.stats.empty.response",
                        "Сервер вернул пустую статистику заказов")
                    .ToErrors();
            }

            return stats;
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "orders.stats.load.failed",
                "Не удалось получить статистику заказов");
        }
    }

    public async Task<Result<CreateOrderResponse, Errors>> CreateOrderAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                "api/orders",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось создать заказ",
                    cancellationToken);

                return CommonErrors.CreateFailed(
                        "order.create.failed",
                        message)
                    .ToErrors();
            }

            var order = await response.Content
                .ReadFromJsonAsync<CreateOrderResponse>(cancellationToken);

            if (order is null)
            {
                return CommonErrors.EmptyResponse(
                        "order.create.empty.response",
                        "Сервер не вернул данные созданного заказа")
                    .ToErrors();
            }

            return order;
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "order.create.failed",
                "Не удалось создать заказ");
        }
    }

    public async Task<UnitResult<Errors>> UpdateOrderAsync(
        Guid id,
        UpdateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync(
                $"api/orders/{id}",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось сохранить изменения заказа",
                    cancellationToken);

                return CommonErrors.UpdateFailed(
                        "order.update.failed",
                        message)
                    .ToErrors();
            }

            return UnitResult.Success<Errors>();
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "order.update.failed",
                "Не удалось сохранить изменения заказа");
        }
    }

    public async Task<UnitResult<Errors>> UpdateRentalPeriodAsync(
        Guid orderItemId,
        UpdateOrderItemRentalPeriodRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync(
                $"api/orders/items/{orderItemId}/rental-period",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось изменить срок аренды",
                    cancellationToken);

                return CommonErrors.UpdateFailed(
                        "order.item.rental.period.update.failed",
                        message)
                    .ToErrors();
            }

            return UnitResult.Success<Errors>();
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "order.item.rental.period.update.failed",
                "Не удалось изменить срок аренды");
        }
    }

    public async Task<UnitResult<Errors>> CompleteOrderAsync(
        Guid id,
        CompleteOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                $"api/orders/{id}/complete",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось завершить заказ",
                    cancellationToken);

                return CommonErrors.UpdateFailed(
                        "order.complete.failed",
                        message)
                    .ToErrors();
            }

            return UnitResult.Success<Errors>();
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "order.complete.failed",
                "Не удалось завершить заказ");
        }
    }

    public async Task<UnitResult<Errors>> DeleteOrderAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.DeleteAsync(
                $"api/orders/{id}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось удалить заказ",
                    cancellationToken);

                return CommonErrors.DeleteFailed(
                        "order.delete.failed",
                        message)
                    .ToErrors();
            }

            return UnitResult.Success<Errors>();
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "order.delete.failed",
                "Не удалось удалить заказ");
        }
    }

    public async Task<Result<bool, Errors>> ExportOrdersAsync(
        ExportOrdersRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
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

            var url = QueryHelpers.AddQueryString(
                "api/orders/export",
                parameters);

            var response = await httpClient.GetAsync(
                url,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось выгрузить заказы",
                    cancellationToken);

                return CommonErrors.LoadFailed(
                        "orders.export.failed",
                        message)
                    .ToErrors();
            }

            var bytes = await response.Content
                .ReadAsByteArrayAsync(cancellationToken);

            await jsRuntime.InvokeVoidAsync(
                "downloadFile",
                cancellationToken,
                "orders.xlsx",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                bytes);

            return true;
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "orders.export.failed",
                "Не удалось выгрузить заказы");
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
    
    public async Task<Result<OrderDetailsDto, Errors>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync(
                $"api/orders/{id}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось загрузить заказ",
                    cancellationToken);

                return CommonErrors.LoadFailed(
                        "get.order.failed",
                        message)
                    .ToErrors();
            }

            var order = await response.Content
                .ReadFromJsonAsync<OrderDetailsDto>(cancellationToken);

            if (order is null)
            {
                return CommonErrors.Failure(
                        "order.details.is.null",
                        "Не удалось получить заказ")
                    .ToErrors();
            }

            return order;
        }
        catch (OperationCanceledException)
        {
            return CommonErrors
                .OperationCancelled("get.order.by.id.was.cancelled")
                .ToErrors();
        }
    }
    
    public async Task<Result<bool, Errors>> ExportContractAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync(
                $"api/orders/{orderId}/contract",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось сформировать договор",
                    cancellationToken);

                return CommonErrors.LoadFailed(
                        "contract.export.failed",
                        message)
                    .ToErrors();
            }

            var bytes = await response.Content
                .ReadAsByteArrayAsync(cancellationToken);

            await jsRuntime.InvokeVoidAsync(
                "downloadFile",
                cancellationToken,
                $"contract_{DateTime.Now:yyyyMMdd_HHmmss}.docx",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                bytes);

            return true;
        }
        catch (OperationCanceledException)
        {
            return CommonErrors
                .OperationCancelled("contract.export.was.cancelled")
                .ToErrors();
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "contract.export.failed",
                "Не удалось сформировать договор");
        }
    }
    
    public async Task<Result<bool, Errors>> ExportContractPdfAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync(
                $"api/orders/{orderId}/contract/pdf",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось сформировать PDF договора",
                    cancellationToken);

                return CommonErrors.LoadFailed(
                        "contract.pdf.export.failed",
                        message)
                    .ToErrors();
            }

            var bytes = await response.Content
                .ReadAsByteArrayAsync(cancellationToken);

            await jsRuntime.InvokeVoidAsync(
                "downloadFile",
                cancellationToken,
                $"contract_{DateTime.Now:yyyyMMdd_HHmmss}.pdf",
                "application/pdf",
                bytes);

            return true;
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "contract.pdf.export.failed",
                "Не удалось сформировать PDF договора");
        }
    }
    
    public async Task<Result<bool, Errors>> ExportTransferActAsync(
        Guid orderId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"api/orders/{orderId}/transfer-act?date={date:yyyy-MM-dd}";

            var response = await httpClient.GetAsync(
                url,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось сформировать акт выдачи",
                    cancellationToken);

                return CommonErrors.LoadFailed(
                        "transfer.act.export.failed",
                        message)
                    .ToErrors();
            }

            var bytes = await response.Content
                .ReadAsByteArrayAsync(cancellationToken);

            await jsRuntime.InvokeVoidAsync(
                "downloadFile",
                cancellationToken,
                $"transfer_act_{date:yyyyMMdd}.docx",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                bytes);

            return true;
        }
        catch (OperationCanceledException)
        {
            return CommonErrors
                .OperationCancelled("transfer.act.export.was.cancelled")
                .ToErrors();
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "transfer.act.export.failed",
                "Не удалось сформировать акт выдачи");
        }
    }
    
    public async Task<Result<IReadOnlyList<OrderDocumentDto>, Errors>> GetDocumentsAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync(
                $"api/orders/{orderId}/documents",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось загрузить документы заказа",
                    cancellationToken);

                return CommonErrors.LoadFailed(
                        "order.documents.load.failed",
                        message)
                    .ToErrors();
            }

            var documents = await response.Content
                .ReadFromJsonAsync<IReadOnlyList<OrderDocumentDto>>(cancellationToken);

            return Result.Success<IReadOnlyList<OrderDocumentDto>, Errors>(
                documents ?? []);
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "order.documents.load.failed",
                "Не удалось загрузить документы заказа");
        }
    }
    
    public async Task<Result<bool, Errors>> ExportReturnActAsync(
        Guid orderId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"api/orders/{orderId}/return-act?date={date:yyyy-MM-dd}";

            var response = await httpClient.GetAsync(
                url,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось сформировать акт возврата",
                    cancellationToken);

                return CommonErrors.LoadFailed(
                        "return.act.export.failed",
                        message)
                    .ToErrors();
            }

            var bytes = await response.Content
                .ReadAsByteArrayAsync(cancellationToken);

            await jsRuntime.InvokeVoidAsync(
                "downloadFile",
                cancellationToken,
                $"return_act_{date:yyyyMMdd}.docx",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                bytes);

            return true;
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "return.act.export.failed",
                "Не удалось сформировать акт возврата");
        }
    }
}