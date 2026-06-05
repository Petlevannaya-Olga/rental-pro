using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using RentalPro.Contracts.PaymentMethods;
using RentalPro.Presentation.Client.Extensions;
using RentalPro.Shared;

namespace RentalPro.Presentation.Client.Services;

public sealed class DictionariesService(HttpClient httpClient)
{
    public Task<Result<List<PaymentMethodDto>, Errors>> GetPaymentMethodsAsync(
        CancellationToken cancellationToken = default)
    {
        return GetListAsync<PaymentMethodDto>(
            "api/payment-methods",
            "payment.methods.load.failed",
            "Не удалось загрузить способы оплаты",
            cancellationToken);
    }

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
}