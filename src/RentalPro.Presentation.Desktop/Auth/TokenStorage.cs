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
    
    public string ManagerFullName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Token))
                return "Текущий пользователь";

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(Token);

            return jwt.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value
                   ?? jwt.Claims.FirstOrDefault(x => x.Type == "name")?.Value
                   ?? "Текущий пользователь";
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