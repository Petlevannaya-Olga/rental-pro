using System.Net.Http.Json;
using RentalPro.Contracts.Auth;

namespace RentalPro.Presentation.Client.Services;

public class AuthService(HttpClient httpClient)
{
    public async Task<LoginResponse?> LoginAsync(
        string login,
        string password)
    {
        var request = new LoginRequest
        {
            Login = login,
            Password = password
        };

        var response = await httpClient.PostAsJsonAsync(
            "api/auth/login",
            request);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content
            .ReadFromJsonAsync<LoginResponse>();
    }
}