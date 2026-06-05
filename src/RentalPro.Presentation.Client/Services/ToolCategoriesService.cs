using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using RentalPro.Contracts.ToolCategories;
using RentalPro.Presentation.Client.Extensions;
using RentalPro.Shared;

namespace RentalPro.Presentation.Client.Services;

public sealed class ToolCategoriesService(HttpClient httpClient)
{
    public async Task<Result<List<ToolCategoryDto>, Errors>> GetToolCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync(
                "api/tool-categories",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось загрузить категории инструментов",
                    cancellationToken);

                return CommonErrors.LoadFailed(
                        "tool.categories.load.failed",
                        message)
                    .ToErrors();
            }

            var categories = await response.Content
                .ReadFromJsonAsync<List<ToolCategoryDto>>(cancellationToken);

            if (categories is null)
            {
                return CommonErrors.EmptyResponse(
                        "tool.categories.empty.response",
                        "Сервер вернул пустой список категорий инструментов")
                    .ToErrors();
            }

            return categories;
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "tool.categories.load.failed",
                "Не удалось загрузить категории инструментов");
        }
    }

    public async Task<UnitResult<Errors>> CreateToolCategoryAsync(
        CreateToolCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                "api/tool-categories",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось создать категорию инструмента",
                    cancellationToken);

                return CommonErrors.CreateFailed(
                        "tool.category.create.failed",
                        message)
                    .ToErrors();
            }

            return UnitResult.Success<Errors>();
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "tool.category.create.failed",
                "Не удалось создать категорию инструмента");
        }
    }

    public async Task<UnitResult<Errors>> UpdateToolCategoryAsync(
        Guid id,
        UpdateToolCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync(
                $"api/tool-categories/{id}",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось обновить категорию инструмента",
                    cancellationToken);

                return CommonErrors.UpdateFailed(
                        "tool.category.update.failed",
                        message)
                    .ToErrors();
            }

            return UnitResult.Success<Errors>();
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "tool.category.update.failed",
                "Не удалось обновить категорию инструмента");
        }
    }

    public async Task<UnitResult<Errors>> DeleteToolCategoryAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.DeleteAsync(
                $"api/tool-categories/{id}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось удалить категорию инструмента",
                    cancellationToken);

                return CommonErrors.DeleteFailed(
                        "tool.category.delete.failed",
                        message)
                    .ToErrors();
            }

            return UnitResult.Success<Errors>();
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "tool.category.delete.failed",
                "Не удалось удалить категорию инструмента");
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