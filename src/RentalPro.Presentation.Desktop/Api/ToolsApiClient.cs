using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using RentalPro.Contracts.Tools;
using RentalPro.Shared;

namespace RentalPro.Presentation.Desktop.Api;

public sealed class ToolsApiClient(IHttpClientFactory httpClientFactory)
{
    public async Task<Result<PagedResult<ToolDto>, Errors>> GetToolsAsync(
        GetToolsRequest request,
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

        if (request.CategoryId.HasValue)
            parameters["categoryId"] = request.CategoryId.Value.ToString();

        if (request.ManufacturerId.HasValue)
            parameters["manufacturerId"] = request.ManufacturerId.Value.ToString();

        if (request.StatusId.HasValue)
            parameters["statusId"] = request.StatusId.Value.ToString();

        var url = BuildUrl("api/tools", parameters);

        try
        {
            var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await ReadErrorsAsync(
                    response,
                    "tools.load.failed",
                    "Не удалось загрузить инструменты",
                    cancellationToken);
            }

            var tools = await response.Content
                .ReadFromJsonAsync<PagedResult<ToolDto>>(cancellationToken);

            if (tools is null)
            {
                return CommonErrors
                    .EmptyResponse(
                        "tools.empty.response",
                        "Сервер вернул пустой список инструментов")
                    .ToErrors();
            }

            return tools;
        }
        catch (HttpRequestException)
        {
            return CommonErrors
                .Failure(
                    "tools.connection.failed",
                    "Не удалось подключиться к серверу")
                .ToErrors();
        }
    }

    public async Task<Result<ToolStatsDto, Errors>> GetStatsAsync(
        CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient("Api");

        try
        {
            var response = await httpClient.GetAsync(
                "api/tools/stats",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await ReadErrorsAsync(
                    response,
                    "tool.stats.load.failed",
                    "Не удалось загрузить статистику инструментов",
                    cancellationToken);
            }

            var stats = await response.Content
                .ReadFromJsonAsync<ToolStatsDto>(cancellationToken);

            if (stats is null)
            {
                return CommonErrors
                    .EmptyResponse(
                        "tool.stats.empty.response",
                        "Сервер вернул пустую статистику инструментов")
                    .ToErrors();
            }

            return stats;
        }
        catch (HttpRequestException)
        {
            return CommonErrors
                .Failure(
                    "tools.connection.failed",
                    "Не удалось подключиться к серверу")
                .ToErrors();
        }
    }

    public async Task<Result<Guid, Errors>> CreateToolAsync(
        CreateToolRequest request,
        CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient("Api");

        try
        {
            var response = await httpClient.PostAsJsonAsync(
                "api/tools",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await ReadErrorsAsync(
                    response,
                    "tool.create.failed",
                    "Не удалось создать инструмент",
                    cancellationToken);
            }

            var id = await response.Content
                .ReadFromJsonAsync<Guid>(cancellationToken);

            if (id == Guid.Empty)
            {
                return CommonErrors
                    .EmptyResponse(
                        "tool.create.empty.response",
                        "Сервер не вернул идентификатор созданного инструмента")
                    .ToErrors();
            }

            return id;
        }
        catch (HttpRequestException)
        {
            return CommonErrors
                .Failure(
                    "tools.connection.failed",
                    "Не удалось подключиться к серверу")
                .ToErrors();
        }
    }

    public async Task<UnitResult<Errors>> UpdateToolAsync(
        Guid id,
        UpdateToolRequest request,
        CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient("Api");

        try
        {
            var response = await httpClient.PutAsJsonAsync(
                $"api/tools/{id}",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await ReadErrorsAsync(
                    response,
                    "tool.update.failed",
                    "Не удалось обновить инструмент",
                    cancellationToken);
            }

            return UnitResult.Success<Errors>();
        }
        catch (HttpRequestException)
        {
            return CommonErrors
                .Failure(
                    "tools.connection.failed",
                    "Не удалось подключиться к серверу")
                .ToErrors();
        }
    }

    public async Task<UnitResult<Errors>> DeleteToolAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient("Api");

        try
        {
            var response = await httpClient.DeleteAsync(
                $"api/tools/{id}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await ReadErrorsAsync(
                    response,
                    "tool.delete.failed",
                    "Не удалось удалить инструмент",
                    cancellationToken);
            }

            return UnitResult.Success<Errors>();
        }
        catch (HttpRequestException)
        {
            return CommonErrors
                .Failure(
                    "tools.connection.failed",
                    "Не удалось подключиться к серверу")
                .ToErrors();
        }
    }

    public async Task<Result<byte[], Errors>> ExportToolsAsync(
        ExportToolsRequest request,
        CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient("Api");

        var parameters = new Dictionary<string, string?>
        {
            ["search"] = request.Search,
            ["categoryId"] = request.CategoryId?.ToString(),
            ["manufacturerId"] = request.ManufacturerId?.ToString(),
            ["statusId"] = request.StatusId?.ToString(),
            ["sortBy"] = request.SortBy,
            ["descending"] = request.Descending.ToString()
        };

        var url = BuildUrl("api/tools/export", parameters);

        try
        {
            var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await ReadErrorsAsync(
                    response,
                    "tools.export.failed",
                    "Не удалось экспортировать инструменты",
                    cancellationToken);
            }

            var bytes = await response.Content
                .ReadAsByteArrayAsync(cancellationToken);

            return bytes;
        }
        catch (HttpRequestException)
        {
            return CommonErrors
                .Failure(
                    "tools.export.failed",
                    "Не удалось экспортировать инструменты")
                .ToErrors();
        }
    }

    public async Task<UnitResult<Errors>> UploadToolImageAsync(
        Guid toolId,
        byte[] fileBytes,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient("Api");

        try
        {
            using var content = new MultipartFormDataContent();

            using var fileContent = new ByteArrayContent(fileBytes);

            fileContent.Headers.ContentType =
                new MediaTypeHeaderValue(contentType);

            content.Add(
                fileContent,
                "file",
                fileName);

            var response = await httpClient.PostAsync(
                $"api/tools/{toolId}/image",
                content,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await ReadErrorsAsync(
                    response,
                    "tool.image.upload.failed",
                    "Не удалось загрузить фото инструмента",
                    cancellationToken);
            }

            return UnitResult.Success<Errors>();
        }
        catch (HttpRequestException)
        {
            return CommonErrors
                .Failure(
                    "tool.image.upload.failed",
                    "Не удалось загрузить фото инструмента")
                .ToErrors();
        }
    }

    public async Task<UnitResult<Errors>> ChangeToolStatusAsync(
        Guid toolId,
        Guid statusId,
        CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient("Api");

        try
        {
            var response = await httpClient.PutAsJsonAsync(
                $"api/tools/{toolId}/status",
                new ChangeToolStatusRequest(statusId),
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await ReadErrorsAsync(
                    response,
                    "tool.status.change.failed",
                    "Не удалось изменить статус инструмента",
                    cancellationToken);
            }

            return UnitResult.Success<Errors>();
        }
        catch (HttpRequestException)
        {
            return CommonErrors
                .Failure(
                    "tool.status.change.failed",
                    "Не удалось изменить статус инструмента")
                .ToErrors();
        }
    }

    public async Task<Result<List<ToolRentalHistoryItemDto>, Errors>> GetRentalHistoryAsync(
        Guid toolId,
        CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient("Api");

        try
        {
            var response = await httpClient.GetAsync(
                $"api/tools/{toolId}/rental-history",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await ReadErrorsAsync(
                    response,
                    "tool.history.load.failed",
                    "Не удалось загрузить историю аренды",
                    cancellationToken);
            }

            var history = await response.Content
                .ReadFromJsonAsync<List<ToolRentalHistoryItemDto>>(
                    cancellationToken);

            return history ?? [];
        }
        catch (HttpRequestException)
        {
            return CommonErrors
                .Failure(
                    "tool.history.load.failed",
                    "Не удалось загрузить историю аренды")
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
}