using System.Net.Http;
using System.Net.Http.Headers;

namespace RentalPro.Presentation.Desktop.Auth;

public sealed class AuthHeaderHandler(TokenStorage tokenStorage) 
    : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(tokenStorage.Token))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenStorage.Token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}