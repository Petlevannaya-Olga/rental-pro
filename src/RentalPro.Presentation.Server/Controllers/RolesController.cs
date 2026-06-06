using Microsoft.AspNetCore.Mvc;
using RentalPro.Application.Roles.CreateRoleCommand;
using RentalPro.Application.Roles.DeleteRoleCommand;
using RentalPro.Application.Roles.GetRolesQuery;
using RentalPro.Application.Roles.UpdateRoleCommand;
using RentalPro.Contracts.Roles;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Presentation.Server.Controllers;

[ApiController]
[Route("api/roles")]
public sealed class RolesController(
    IQueryHandler<List<RoleDto>, GetRolesQuery> getRolesHandler,
    CreateRoleCommandHandler createHandler,
    UpdateRoleCommandHandler updateHandler,
    DeleteRoleCommandHandler deleteHandler)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        CancellationToken cancellationToken)
    {
        var result = await getRolesHandler.Handle(
            new GetRolesQuery(),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateRoleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await createHandler.Handle(
            new CreateRoleCommand(request.Name),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateRoleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await updateHandler.Handle(
            new UpdateRoleCommand(
                id,
                request.Name),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await deleteHandler.Handle(
            new DeleteRoleCommand(id),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }
}