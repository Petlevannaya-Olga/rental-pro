using System.Text.Json;
using RentalPro.Presentation.Client.Models;

namespace RentalPro.Presentation.Client.Extensions;

public static class StringExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static string ExtractErrorMessage(
        this string content,
        string defaultMessage)
    {
        if (string.IsNullOrWhiteSpace(content))
            return defaultMessage;

        var arrayMessage = TryReadErrorArrayMessage(content);

        if (!string.IsNullOrWhiteSpace(arrayMessage))
            return arrayMessage;

        var objectMessage = TryReadErrorObjectMessage(content);

        if (!string.IsNullOrWhiteSpace(objectMessage))
            return objectMessage;

        return defaultMessage;
    }

    private static string? TryReadErrorArrayMessage(string content)
    {
        try
        {
            var errors = JsonSerializer.Deserialize<List<ErrorResponse>>(
                content,
                JsonOptions);

            return errors?
                .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Message))
                ?.Message;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? TryReadErrorObjectMessage(string content)
    {
        try
        {
            var error = JsonSerializer.Deserialize<ErrorResponse>(
                content,
                JsonOptions);

            return error?.Message;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}