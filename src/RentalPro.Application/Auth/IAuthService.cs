using RentalPro.Contracts.Auth;

namespace RentalPro.Application.Auth;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken);
}