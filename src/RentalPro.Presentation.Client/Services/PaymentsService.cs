using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using RentalPro.Contracts.Payments;
using RentalPro.Presentation.Client.Extensions;
using RentalPro.Shared;

namespace RentalPro.Presentation.Client.Services;

public sealed class PaymentsService(HttpClient httpClient)
{
    public async Task<Result<CreatePaymentResponse, Errors>> CreatePaymentAsync(
        CreatePaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                "api/payments",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось принять оплату",
                    cancellationToken);

                return CommonErrors.CreateFailed(
                        "payment.create.failed",
                        message)
                    .ToErrors();
            }

            var payment = await response.Content
                .ReadFromJsonAsync<CreatePaymentResponse>(cancellationToken);

            if (payment is null)
            {
                return CommonErrors.EmptyResponse(
                        "payment.create.empty.response",
                        "Сервер не вернул данные платежа")
                    .ToErrors();
            }

            return payment;
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "payment.create.failed",
                "Не удалось принять оплату");
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