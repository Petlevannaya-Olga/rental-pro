namespace RentalPro.Contracts.Auth;

public sealed class LoginResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public string? Token { get; set; }

    public string? UserName { get; set; }

    public string? Role { get; set; }

    public static LoginResponse Ok(
        string token,
        string userName,
        string role)
    {
        return new LoginResponse
        {
            Success = true,
            Message = "Авторизация успешна",
            Token = token,
            UserName = userName,
            Role = role
        };
    }

    public static LoginResponse Fail(string message)
    {
        return new LoginResponse
        {
            Success = false,
            Message = message
        };
    }
}