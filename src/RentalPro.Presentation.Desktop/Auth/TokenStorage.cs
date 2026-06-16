namespace RentalPro.Presentation.Desktop.Auth;

public sealed class TokenStorage
{
    public string? Token { get; private set; }

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token);

    public void SetToken(string token)
    {
        Token = token;
    }

    public void Clear()
    {
        Token = null;
    }
}