using Microsoft.JSInterop;
using RentalPro.Shared;

namespace RentalPro.Presentation.Client.Extensions;

public static class ExceptionExtensions
{
    public static string ToUserFriendlyMessage(
        this Exception exception,
        string defaultMessage)
    {
        return exception switch
        {
            HttpRequestException =>
                $"{defaultMessage}. Проверьте, что сервер запущен",

            TaskCanceledException =>
                "Сервер не ответил вовремя. Попробуйте позже",

            OperationCanceledException =>
                "Операция была отменена",

            JSException =>
                "Ошибка взаимодействия с браузером",

            _ =>
                defaultMessage
        };
    }
    
    public static Errors ToErrors(
        this Exception exception,
        string code,
        string defaultMessage)
    {
        return CommonErrors.Failure(
                code,
                exception.ToUserFriendlyMessage(defaultMessage))
            .ToErrors();
    }
}