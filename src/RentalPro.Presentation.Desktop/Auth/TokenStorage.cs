using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RentalPro.Presentation.Desktop.Auth;

public sealed class TokenStorage
{
    public string? Token { get; private set; }

    public bool IsAuthenticated =>
        !string.IsNullOrWhiteSpace(Token);

    public Guid? UserId
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Token))
                return null;

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(Token);

            var claim = jwt.Claims.FirstOrDefault(x =>
                x.Type == ClaimTypes.NameIdentifier ||
                x.Type == "sub" ||
                x.Type == "nameid");

            return Guid.TryParse(claim?.Value, out var id)
                ? id
                : null;
        }
    }

    public void SetToken(string token)
    {
        Token = token;
    }

    public void Clear()
    {
        Token = null;
    }
}