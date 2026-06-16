using System.Net.Http;
using System.Net.Http.Json;
using RentalPro.Contracts.Auth;

namespace RentalPro.Presentation.Desktop.Api;

public sealed class AuthApiClient(HttpClient httpClient)
{
    public async Task<LoginResponse?> LoginAsync(
        string login,
        string password,
        CancellationToken cancellationToken = default)
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

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<LoginResponse>(
            cancellationToken);
    }
}