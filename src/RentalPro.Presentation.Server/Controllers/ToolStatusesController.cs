using Microsoft.AspNetCore.Mvc;
using RentalPro.Application.ToolStatuses.CreateToolStatusCommand;
using RentalPro.Application.ToolStatuses.DeleteToolStatusCommand;
using RentalPro.Application.ToolStatuses.GetToolStatusesQuery;
using RentalPro.Application.ToolStatuses.UpdateToolStatusCommand;
using RentalPro.Contracts.ToolStatuses;

namespace RentalPro.Presentation.Server.Controllers;

[ApiController]
[Route("api/tool-statuses")]
public sealed class ToolStatusesController(
    GetToolStatusesQueryHandler getHandler,
    CreateToolStatusCommandHandler createHandler,
    UpdateToolStatusCommandHandler updateHandler,
    DeleteToolStatusCommandHandler deleteHandler)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ToolStatusDto>>> Get(
        CancellationToken cancellationToken)
    {
        var result = await getHandler.Handle(
            new GetToolStatusesQuery(),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateToolStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await createHandler.Handle(
            new CreateToolStatusCommand(request.Name),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateToolStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await updateHandler.Handle(
            new UpdateToolStatusCommand(
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
            new DeleteToolStatusCommand(id),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }
}