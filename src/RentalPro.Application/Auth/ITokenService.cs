namespace RentalPro.Application.Auth;

public interface ITokenService
{
    string CreateToken(
        Guid userId,
        string login,
        string fullName,
        string role);
}