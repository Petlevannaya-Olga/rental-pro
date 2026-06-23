using System.Net.Http;
using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using RentalPro.Contracts;
using RentalPro.Shared;

namespace RentalPro.Presentation.Desktop.Api;

public sealed class DictionariesApiClient(IHttpClientFactory httpClientFactory)
{
    public async Task<Result<List<TDto>, Errors>> GetListAsync<TDto>(
        string url,
        string errorCode,
        string errorMessage,
        CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient("Api");

        try
        {
            var response = await httpClient.GetAsync(
                url,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await ReadErrorsAsync(
                    response,
                    errorCode,
                    errorMessage,
                    cancellationToken);
            }

            var items = await response.Content
                .ReadFromJsonAsync<List<TDto>>(cancellationToken);

            if (items is null)
            {
                return CommonErrors
                    .EmptyResponse(
                        $"{errorCode}.empty",
                        "Сервер вернул пустой ответ")
                    .ToErrors();
            }

            return items;
        }
        catch (HttpRequestException)
        {
            return CommonErrors
                .Failure(
                    errorCode,
                    errorMessage)
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
    
    public async Task<Result<DictionaryStatsDto, Errors>> GetStatsAsync(
        CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient("Api");

        try
        {
            var response = await httpClient.GetAsync(
                "api/dictionaries/stats",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await ReadErrorsAsync(
                    response,
                    "dictionary.stats.load.failed",
                    "Не удалось загрузить статистику справочников",
                    cancellationToken);
            }

            var stats = await response.Content
                .ReadFromJsonAsync<DictionaryStatsDto>(cancellationToken);

            if (stats is null)
            {
                return CommonErrors
                    .EmptyResponse(
                        "dictionary.stats.empty.response",
                        "Сервер вернул пустую статистику справочников")
                    .ToErrors();
            }

            return stats;
        }
        catch (HttpRequestException)
        {
            return CommonErrors
                .Failure(
                    "dictionary.stats.load.failed",
                    "Не удалось подключиться к серверу")
                .ToErrors();
        }
    }
}