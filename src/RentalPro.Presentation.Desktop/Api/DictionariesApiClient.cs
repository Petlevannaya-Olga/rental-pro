using System.Net.Http;
using System.Net.Http.Json;
using CSharpFunctionalExtensions;
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
}