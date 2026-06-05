using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using RentalPro.Presentation.Client.Extensions;
using RentalPro.Shared;

namespace RentalPro.Presentation.Client.Services;

public sealed class DictionaryCrudService(HttpClient httpClient)
{
    public async Task<Result<List<TDto>, Errors>> GetListAsync<TDto>(
        string url,
        string errorCode,
        string defaultMessage,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync(
                url,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    defaultMessage,
                    cancellationToken);

                return CommonErrors.LoadFailed(
                        errorCode,
                        message)
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
            return ex.ToErrors(errorCode, defaultMessage);
        }
    }

    public async Task<UnitResult<Errors>> CreateAsync<TRequest>(
        string url,
        TRequest request,
        string errorCode,
        string defaultMessage,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                url,
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    defaultMessage,
                    cancellationToken);

                return CommonErrors.CreateFailed(
                        errorCode,
                        message)
                    .ToErrors();
            }

            return UnitResult.Success<Errors>();
        }
        catch (Exception ex)
        {
            return ex.ToErrors(errorCode, defaultMessage);
        }
    }

    public async Task<UnitResult<Errors>> UpdateAsync<TRequest>(
        string url,
        TRequest request,
        string errorCode,
        string defaultMessage,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync(
                url,
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    defaultMessage,
                    cancellationToken);

                return CommonErrors.UpdateFailed(
                        errorCode,
                        message)
                    .ToErrors();
            }

            return UnitResult.Success<Errors>();
        }
        catch (Exception ex)
        {
            return ex.ToErrors(errorCode, defaultMessage);
        }
    }

    public async Task<UnitResult<Errors>> DeleteAsync(
        string url,
        string errorCode,
        string defaultMessage,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.DeleteAsync(
                url,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    defaultMessage,
                    cancellationToken);

                return CommonErrors.DeleteFailed(
                        errorCode,
                        message)
                    .ToErrors();
            }

            return UnitResult.Success<Errors>();
        }
        catch (Exception ex)
        {
            return ex.ToErrors(errorCode, defaultMessage);
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