using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using RentalPro.Contracts;
using RentalPro.Contracts.PaymentMethods;
using RentalPro.Contracts.PaymentTypes;
using RentalPro.Presentation.Client.Extensions;
using RentalPro.Shared;

namespace RentalPro.Presentation.Client.Services;

public sealed class DictionariesService(HttpClient httpClient)
{
    private async Task<Result<List<TDto>, Errors>> GetListAsync<TDto>(
        string url,
        string errorCode,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                return CommonErrors.LoadFailed(
                        errorCode,
                        content.ExtractErrorMessage(errorMessage))
                    .ToErrors();
            }

            var items = await response.Content
                .ReadFromJsonAsync<List<TDto>>(cancellationToken);

            if (items is null)
            {
                return CommonErrors.EmptyResponse(
                        $"{errorCode}.empty",
                        "Сервер вернул пустой ответ")
                    .ToErrors();
            }

            return items;
        }
        catch (Exception ex)
        {
            return ex.ToErrors(errorCode, errorMessage);
        }
    }
    
    public async Task<Result<DictionaryStatsDto, Errors>> GetStatsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync(
                "api/dictionaries/stats",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content
                    .ReadAsStringAsync(cancellationToken);

                return CommonErrors.LoadFailed(
                        "dictionary.stats.load.failed",
                        content.ExtractErrorMessage("Не удалось загрузить статистику справочников"))
                    .ToErrors();
            }

            var stats = await response.Content
                .ReadFromJsonAsync<DictionaryStatsDto>(cancellationToken);

            if (stats is null)
            {
                return CommonErrors.EmptyResponse(
                        "dictionary.stats.empty.response",
                        "Сервер вернул пустую статистику справочников")
                    .ToErrors();
            }

            return stats;
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "dictionary.stats.load.failed",
                "Не удалось загрузить статистику справочников");
        }
    }
}