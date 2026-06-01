using Microsoft.AspNetCore.Mvc;
using RentalPro.Application.Auth;
using RentalPro.Contracts.Auth;

namespace RentalPro.Presentation.Server.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(
            request,
            cancellationToken);

        if (!result.Success)
            return Unauthorized(result);

        return Ok(result);
    }
}