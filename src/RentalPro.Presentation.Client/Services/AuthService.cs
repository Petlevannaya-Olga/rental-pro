using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using RentalPro.Contracts.Auth;
using RentalPro.Presentation.Client.Extensions;
using RentalPro.Shared;

namespace RentalPro.Presentation.Client.Services;

public class AuthService(HttpClient httpClient)
{
    public async Task<Result<LoginResponse, Errors>> LoginAsync(
        string login,
        string password,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new LoginRequest
            {
                Login = login,
                Password = password
            };

            var response = await httpClient.PostAsJsonAsync(
                "api/auth/login",
                request,
                cancellationToken);

            var loginResponse = await response.Content
                .ReadFromJsonAsync<LoginResponse>(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return CommonErrors.LoadFailed(
                        "auth.login.failed",
                        loginResponse?.Message ?? "Неверный логин или пароль")
                    .ToErrors();
            }

            if (loginResponse is null)
            {
                return CommonErrors.EmptyResponse(
                        "auth.login.empty.response",
                        "Сервер вернул пустой ответ")
                    .ToErrors();
            }

            return loginResponse;
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "auth.login.failed",
                "Не удалось выполнить вход");
        }
    }
}