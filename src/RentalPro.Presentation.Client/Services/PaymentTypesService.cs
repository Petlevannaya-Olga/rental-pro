using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using RentalPro.Contracts.PaymentTypes;
using RentalPro.Presentation.Client.Extensions;
using RentalPro.Shared;

namespace RentalPro.Presentation.Client.Services;

public sealed class PaymentTypesService(HttpClient httpClient)
{
    public async Task<Result<List<PaymentTypeDto>, Errors>> GetPaymentTypesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync(
                "api/payment-types",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось загрузить способы оплаты",
                    cancellationToken);

                return CommonErrors.LoadFailed(
                        "payment.types.load.failed",
                        message)
                    .ToErrors();
            }

            var paymentTypes = await response.Content
                .ReadFromJsonAsync<List<PaymentTypeDto>>(cancellationToken);

            if (paymentTypes is null)
            {
                return CommonErrors.EmptyResponse(
                        "payment.types.empty.response",
                        "Сервер вернул пустой список способов оплаты")
                    .ToErrors();
            }

            return paymentTypes;
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "payment.types.load.failed",
                "Не удалось загрузить способы оплаты");
        }
    }

    public async Task<UnitResult<Errors>> UpdatePaymentTypeAsync(
        Guid id,
        UpdatePaymentTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync(
                $"api/payment-types/{id}",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось обновить способ оплаты",
                    cancellationToken);

                return CommonErrors.UpdateFailed(
                        "payment.type.update.failed",
                        message)
                    .ToErrors();
            }

            return UnitResult.Success<Errors>();
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "payment.type.update.failed",
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
    
    public async Task<UnitResult<Errors>> DeletePaymentTypeAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.DeleteAsync(
                $"api/payment-types/{id}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось удалить способ оплаты",
                    cancellationToken);

                return CommonErrors.DeleteFailed(
                        "payment.type.delete.failed",
                        message)
                    .ToErrors();
            }

            return UnitResult.Success<Errors>();
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "payment.type.delete.failed",
                "Не удалось удалить способ оплаты");
        }
    }
    
    public async Task<UnitResult<Errors>> CreatePaymentTypeAsync(
        CreatePaymentTypeRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                "api/payment-types",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось создать способ оплаты",
                    cancellationToken);

                return CommonErrors.CreateFailed(
                        "payment.type.create.failed",
                        message)
                    .ToErrors();
            }

            return UnitResult.Success<Errors>();
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "payment.type.create.failed",
                "Не удалось создать способ оплаты");
        }
    }
}