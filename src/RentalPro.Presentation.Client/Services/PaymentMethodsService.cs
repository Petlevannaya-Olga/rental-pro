using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using RentalPro.Contracts.PaymentMethods;
using RentalPro.Presentation.Client.Extensions;
using RentalPro.Shared;

namespace RentalPro.Presentation.Client.Services;

public sealed class PaymentMethodsService(HttpClient httpClient)
{
    public async Task<Result<List<PaymentMethodDto>, Errors>> GetPaymentMethodsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync(
                "api/payment-methods",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось загрузить способы оплаты",
                    cancellationToken);

                return CommonErrors.LoadFailed(
                        "payment.methods.load.failed",
                        message)
                    .ToErrors();
            }

            var paymentMethods = await response.Content
                .ReadFromJsonAsync<List<PaymentMethodDto>>(cancellationToken);

            if (paymentMethods is null)
            {
                return CommonErrors.EmptyResponse(
                        "payment.methods.empty.response",
                        "Сервер вернул пустой список способов оплаты")
                    .ToErrors();
            }

            return paymentMethods;
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "payment.methods.load.failed",
                "Не удалось загрузить способы оплаты");
        }
    }

    public async Task<UnitResult<Errors>> UpdatePaymentMethodAsync(
        Guid id,
        UpdatePaymentMethodRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync(
                $"api/payment-methods/{id}",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось обновить способ оплаты",
                    cancellationToken);

                return CommonErrors.UpdateFailed(
                        "payment.method.update.failed",
                        message)
                    .ToErrors();
            }

            return UnitResult.Success<Errors>();
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "payment.method.update.failed",
                "Не удалось обновить способ оплаты");
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
    
    public async Task<UnitResult<Errors>> DeletePaymentMethodAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.DeleteAsync(
                $"api/payment-methods/{id}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось удалить способ оплаты",
                    cancellationToken);

                return CommonErrors.DeleteFailed(
                        "payment.method.delete.failed",
                        message)
                    .ToErrors();
            }

            return UnitResult.Success<Errors>();
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "payment.method.delete.failed",
                "Не удалось удалить способ оплаты");
        }
    }
    
    public async Task<UnitResult<Errors>> CreatePaymentMethodAsync(
        CreatePaymentMethodRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                "api/payment-methods",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось создать способ оплаты",
                    cancellationToken);

                return CommonErrors.CreateFailed(
                        "payment.method.create.failed",
                        message)
                    .ToErrors();
            }

            return UnitResult.Success<Errors>();
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "payment.method.create.failed",
                "Не удалось создать способ оплаты");
        }
    }
}