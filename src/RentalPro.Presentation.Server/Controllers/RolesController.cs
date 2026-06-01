using Microsoft.AspNetCore.Mvc;
using RentalPro.Application.Roles.GetRolesQuery;
using RentalPro.Contracts.Roles;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Presentation.Server.Controllers;

[ApiController]
[Route("api/roles")]
public sealed class RolesController(
    IQueryHandler<List<RoleDto>, GetRolesQuery> getRolesHandler)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetRoles(
        CancellationToken cancellationToken)
    {
        var result = await getRolesHandler.Handle(
            new GetRolesQuery(),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
}