using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using RentalPro.Contracts.Auth;
using RentalPro.Shared;

namespace RentalPro.Presentation.Desktop.Api;

public sealed class AuthApiClient(HttpClient httpClient)
{
    public async Task<Result<LoginResponse, Errors>> LoginAsync(
        string login,
        string password,
        CancellationToken cancellationToken = default)
    {
        var request = new LoginRequest
        {
            Login = login,
            Password = password
        };

        HttpResponseMessage response;

        try
        {
            response = await httpClient.PostAsJsonAsync(
                "api/auth/login",
                request,
                cancellationToken);
        }
        catch (HttpRequestException)
        {
            return CommonErrors
                .Failure("auth.connection.error", "Не удалось подключиться к серверу.")
                .ToErrors();
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return CommonErrors
                .Validation("auth.invalid.credentials", "Неверный логин или пароль.")
                .ToErrors();
        }

        if (!response.IsSuccessStatusCode)
        {
            return CommonErrors
                .Failure("auth.login.failed", "Не удалось выполнить вход в систему.")
                .ToErrors();
        }

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>(
            cancellationToken);

        if (loginResponse is null)
        {
            return CommonErrors
                .Failure("auth.empty.response", "Сервер вернул пустой ответ.")
                .ToErrors();
        }

        return loginResponse;
    }
}