using System.Net.Http.Headers;
using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using RentalPro.Contracts.Tools;
using RentalPro.Presentation.Client.Extensions;
using RentalPro.Shared;

namespace RentalPro.Presentation.Client.Services;

public sealed class ToolsService(HttpClient httpClient, IJSRuntime jsRuntime)
{
    public async Task<Result<PagedResult<ToolDto>, Errors>> GetToolsAsync(
        GetToolsRequest request,
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

            if (request.CategoryId.HasValue)
                parameters["categoryId"] = request.CategoryId.Value.ToString();

            if (request.ManufacturerId.HasValue)
                parameters["manufacturerId"] = request.ManufacturerId.Value.ToString();

            if (request.StatusId.HasValue)
                parameters["statusId"] = request.StatusId.Value.ToString();

            var url = QueryHelpers.AddQueryString(
                "api/tools",
                parameters);

            var response = await httpClient.GetAsync(
                url,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось загрузить инструменты",
                    cancellationToken);

                return CommonErrors.LoadFailed(
                        "tools.load.failed",
                        message)
                    .ToErrors();
            }

            var tools = await response.Content
                .ReadFromJsonAsync<PagedResult<ToolDto>>(cancellationToken);

            if (tools is null)
            {
                return CommonErrors.EmptyResponse(
                        "tools.empty.response",
                        "Сервер вернул пустой список инструментов")
                    .ToErrors();
            }

            return tools;
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "tools.load.failed",
                "Не удалось загрузить инструменты");
        }
    }

    public async Task<Result<ToolStatsDto, Errors>> GetStatsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync(
                "api/tools/stats",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось загрузить статистику инструментов",
                    cancellationToken);

                return CommonErrors.LoadFailed(
                        "tool.stats.load.failed",
                        message)
                    .ToErrors();
            }

            var stats = await response.Content
                .ReadFromJsonAsync<ToolStatsDto>(cancellationToken);

            if (stats is null)
            {
                return CommonErrors.EmptyResponse(
                        "tool.stats.empty.response",
                        "Сервер вернул пустую статистику инструментов")
                    .ToErrors();
            }

            return stats;
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "tool.stats.load.failed",
                "Не удалось загрузить статистику инструментов");
        }
    }

    public async Task<Result<Guid, Errors>> CreateToolAsync(
        CreateToolRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                "api/tools",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось создать инструмент",
                    cancellationToken);

                return CommonErrors.CreateFailed(
                        "tool.create.failed",
                        message)
                    .ToErrors();
            }

            var id = await response.Content
                .ReadFromJsonAsync<Guid>(cancellationToken);

            if (id == Guid.Empty)
            {
                return CommonErrors.EmptyResponse(
                        "tool.create.empty.response",
                        "Сервер не вернул идентификатор созданного инструмента")
                    .ToErrors();
            }

            return id;
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "tool.create.failed",
                "Не удалось создать инструмент");
        }
    }

    public async Task<UnitResult<Errors>> UpdateToolAsync(
        Guid id,
        UpdateToolRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync(
                $"api/tools/{id}",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось обновить инструмент",
                    cancellationToken);

                return CommonErrors.UpdateFailed(
                        "tool.update.failed",
                        message)
                    .ToErrors();
            }

            return UnitResult.Success<Errors>();
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "tool.update.failed",
                "Не удалось обновить инструмент");
        }
    }

    public async Task<UnitResult<Errors>> DeleteToolAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.DeleteAsync(
                $"api/tools/{id}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось удалить инструмент",
                    cancellationToken);

                return CommonErrors.DeleteFailed(
                        "tool.delete.failed",
                        message)
                    .ToErrors();
            }

            return UnitResult.Success<Errors>();
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "tool.delete.failed",
                "Не удалось удалить инструмент");
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
    
    public async Task<Result<bool, Errors>> ExportToolsAsync(
        ExportToolsRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = QueryHelpers.AddQueryString(
                "api/tools/export",
                new Dictionary<string, string?>
                {
                    ["search"] = request.Search,
                    ["categoryId"] = request.CategoryId?.ToString(),
                    ["manufacturerId"] = request.ManufacturerId?.ToString(),
                    ["statusId"] = request.StatusId?.ToString(),
                    ["sortBy"] = request.SortBy,
                    ["descending"] = request.Descending.ToString()
                });

            var response = await httpClient.GetAsync(
                url,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось экспортировать инструменты",
                    cancellationToken);

                return CommonErrors.LoadFailed(
                        "tools.export.failed",
                        message)
                    .ToErrors();
            }

            var bytes = await response.Content
                .ReadAsByteArrayAsync(cancellationToken);

            await jsRuntime.InvokeVoidAsync(
                "downloadFile",
                cancellationToken,
                "tools.xlsx",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                bytes);

            return true;
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "tools.export.failed",
                "Не удалось экспортировать инструменты");
        }
    }
    
    public async Task<Result<bool, Errors>> UploadToolImageAsync(
        Guid toolId,
        byte[] fileBytes,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
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
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось загрузить фото инструмента",
                    cancellationToken);

                return CommonErrors.LoadFailed(
                        "tool.image.upload.failed",
                        message)
                    .ToErrors();
            }

            return true;
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "tool.image.upload.failed",
                "Не удалось загрузить фото инструмента");
        }
    }
    
    public async Task<Result<bool, Errors>> ChangeToolStatusAsync(
        Guid toolId,
        Guid statusId,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync(
            $"api/tools/{toolId}/status",
            new ChangeToolStatusRequest(statusId),
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var message = await ReadErrorMessageAsync(
                response,
                "Не удалось изменить статус инструмента",
                cancellationToken);

            return CommonErrors.Failure(
                    "tool.status.change.failed",
                    message)
                .ToErrors();
        }

        return true;
    }
}