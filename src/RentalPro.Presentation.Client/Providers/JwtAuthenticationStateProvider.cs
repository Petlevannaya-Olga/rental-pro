using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace RentalPro.Presentation.Client.Providers;

public sealed class JwtAuthenticationStateProvider(
    ILocalStorageService localStorage)
    : AuthenticationStateProvider
{
    private readonly ClaimsPrincipal _anonymous =
        new(new ClaimsIdentity());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await localStorage.GetItemAsync<string>("authToken");

        if (string.IsNullOrWhiteSpace(token))
        {
            return new AuthenticationState(_anonymous);
        }

        var claims = ParseClaims(token);

        var user = new ClaimsPrincipal(
            new ClaimsIdentity(claims, "jwt"));

        return new AuthenticationState(user);
    }

    public void NotifyUserAuthentication(string token)
    {
        var claims = ParseClaims(token);

        var user = new ClaimsPrincipal(
            new ClaimsIdentity(claims, "jwt"));

        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(user)));
    }

    private static IEnumerable<Claim> ParseClaims(string token)
    {
        var jwt = new JwtSecurityTokenHandler()
            .ReadJwtToken(token);

        return jwt.Claims;
    }
    
    public void NotifyUserLogout()
    {
        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());

        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(anonymousUser)));
    }
}