using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using RentalPro.Contracts.Customers;
using RentalPro.Presentation.Client.Extensions;
using RentalPro.Shared;

namespace RentalPro.Presentation.Client.Services;

public sealed class CustomersService(
    HttpClient httpClient,
    IJSRuntime jsRuntime)
{
    public async Task<Result<PagedResult<CustomerDto>, Errors>> GetCustomersAsync(
        GetCustomersRequest request,
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

            if (request.HasOrders.HasValue)
                parameters["hasOrders"] = request.HasOrders.Value.ToString();

            if (request.HasActiveOrders.HasValue)
                parameters["hasActiveOrders"] = request.HasActiveOrders.Value.ToString();
            
            if (request.IsRegular.HasValue)
                parameters["isRegular"] = request.IsRegular.Value.ToString();

            var url = QueryHelpers.AddQueryString(
                "api/customers",
                parameters);

            var response = await httpClient.GetAsync(
                url,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось получить список клиентов",
                    cancellationToken);

                return CommonErrors.LoadFailed(
                        "customers.load.failed",
                        message)
                    .ToErrors();
            }

            var customers = await response.Content
                .ReadFromJsonAsync<PagedResult<CustomerDto>>(cancellationToken);

            if (customers is null)
            {
                return CommonErrors.EmptyResponse(
                        "customers.empty.response",
                        "Сервер вернул пустой список клиентов")
                    .ToErrors();
            }

            return customers;
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "customers.load.failed",
                "Не удалось получить список клиентов");
        }
    }

    public async Task<Result<CustomerStatsDto, Errors>> GetStatsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync(
                "api/customers/stats",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось получить статистику клиентов",
                    cancellationToken);

                return CommonErrors.LoadFailed(
                        "customers.stats.load.failed",
                        message)
                    .ToErrors();
            }

            var stats = await response.Content
                .ReadFromJsonAsync<CustomerStatsDto>(cancellationToken);

            if (stats is null)
            {
                return CommonErrors.EmptyResponse(
                        "customers.stats.empty.response",
                        "Сервер вернул пустую статистику клиентов")
                    .ToErrors();
            }

            return stats;
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "customers.stats.load.failed",
                "Не удалось получить статистику клиентов");
        }
    }

    public async Task<Result<CreateCustomerResponse, Errors>> CreateCustomerAsync(
        CreateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                "api/customers",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось добавить клиента",
                    cancellationToken);

                return CommonErrors.CreateFailed(
                        "customer.create.failed",
                        message)
                    .ToErrors();
            }

            var customer = await response.Content
                .ReadFromJsonAsync<CreateCustomerResponse>(cancellationToken);

            if (customer is null)
            {
                return CommonErrors.EmptyResponse(
                        "customer.create.empty.response",
                        "Сервер не вернул данные созданного клиента")
                    .ToErrors();
            }

            return customer;
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "customer.create.failed",
                "Не удалось добавить клиента");
        }
    }

    public async Task<UnitResult<Errors>> UpdateCustomerAsync(
        Guid id,
        UpdateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync(
                $"api/customers/{id}",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось сохранить изменения клиента",
                    cancellationToken);

                return CommonErrors.UpdateFailed(
                        "customer.update.failed",
                        message)
                    .ToErrors();
            }

            return UnitResult.Success<Errors>();
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "customer.update.failed",
                "Не удалось сохранить изменения клиента");
        }
    }

    public async Task<UnitResult<Errors>> DeleteCustomerAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.DeleteAsync(
                $"api/customers/{id}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось удалить карточку клиента",
                    cancellationToken);

                return CommonErrors.DeleteFailed(
                        "customer.delete.failed",
                        message)
                    .ToErrors();
            }

            return UnitResult.Success<Errors>();
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "customer.delete.failed",
                "Не удалось удалить карточку клиента");
        }
    }

    public async Task<Result<bool, Errors>> ExportCustomersAsync(
        ExportCustomersRequest request,
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

            if (request.HasOrders.HasValue)
                parameters["hasOrders"] = request.HasOrders.Value.ToString();

            if (request.HasActiveOrders.HasValue)
                parameters["hasActiveOrders"] = request.HasActiveOrders.Value.ToString();
            
            if (request.IsRegular.HasValue)
                parameters["isRegular"] = request.IsRegular.Value.ToString();

            var url = QueryHelpers.AddQueryString(
                "api/customers/export",
                parameters);

            var response = await httpClient.GetAsync(
                url,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось выгрузить клиентов",
                    cancellationToken);

                return CommonErrors.LoadFailed(
                        "customers.export.failed",
                        message)
                    .ToErrors();
            }

            var bytes = await response.Content
                .ReadAsByteArrayAsync(cancellationToken);

            await jsRuntime.InvokeVoidAsync(
                "downloadFile",
                cancellationToken,
                "customers.xlsx",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                bytes);

            return true;
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "customers.export.failed",
                "Не удалось выгрузить клиентов");
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
    
    public async Task<Result<List<CustomerOrderHistoryItemDto>, Errors>> GetOrderHistoryAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync(
                $"api/customers/{customerId}/order-history",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось загрузить историю заказов клиента",
                    cancellationToken);

                return CommonErrors.LoadFailed(
                        "customer.order.history.load.failed",
                        message)
                    .ToErrors();
            }

            var history = await response.Content
                .ReadFromJsonAsync<List<CustomerOrderHistoryItemDto>>(
                    cancellationToken);

            return history ?? [];
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "customer.order.history.load.failed",
                "Не удалось загрузить историю заказов клиента");
        }
    }
}