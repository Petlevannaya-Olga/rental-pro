using Microsoft.AspNetCore.Mvc;
using RentalPro.Application.Manufactures.CreateManufactureCommand;
using RentalPro.Application.Manufactures.DeleteManufactureCommand;
using RentalPro.Application.Manufactures.GetManufacturesQuery;
using RentalPro.Application.Manufactures.UpdateManufactureCommand;
using RentalPro.Contracts.Manufacturers;

namespace RentalPro.Presentation.Server.Controllers;

[ApiController]
[Route("api/manufacturers")]
public sealed class ManufacturersController(
    GetManufacturersQueryHandler getHandler,
    CreateManufacturerCommandHandler createHandler,
    UpdateManufacturerCommandHandler updateHandler,
    DeleteManufacturerCommandHandler deleteHandler)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ManufacturerDto>>> Get(
        CancellationToken cancellationToken)
    {
        var result = await getHandler.Handle(
            new GetManufacturersQuery(),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateManufacturerRequest request,
        CancellationToken cancellationToken)
    {
        var result = await createHandler.Handle(
            new CreateManufacturerCommand(request.Name, request.Country),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateManufacturerRequest request,
        CancellationToken cancellationToken)
    {
        var result = await updateHandler.Handle(
            new UpdateManufacturerCommand(
                id,
                request.Name,
                request.Country),
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
            new DeleteManufacturerCommand(id),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }
}