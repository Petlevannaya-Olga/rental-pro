using Microsoft.AspNetCore.Mvc;
using RentalPro.Application.Tools.ChangeToolStatusCommand;
using RentalPro.Application.Tools.CreateToolCommand;
using RentalPro.Application.Tools.DeleteToolCommand;
using RentalPro.Application.Tools.ExportToolsQuery;
using RentalPro.Application.Tools.GetToolsQuery;
using RentalPro.Application.Tools.GetToolStatsQuery;
using RentalPro.Application.Tools.UpdateToolCommand;
using RentalPro.Application.Tools.UploadToolImageCommand;
using RentalPro.Contracts.Tools;
using RentalPro.Shared;

namespace RentalPro.Presentation.Server.Controllers;

[ApiController]
[Route("api/tools")]
public sealed class ToolsController(
    GetToolsQueryHandler getToolsHandler,
    GetToolStatsQueryHandler getStatsHandler,
    CreateToolCommandHandler createHandler,
    UpdateToolCommandHandler updateHandler,
    DeleteToolCommandHandler deleteHandler,
    ExportToolsQueryHandler exportHandler,
    UploadToolImageCommandHandler uploadImageHandler,
    ChangeToolStatusCommandHandler changeToolStatusHandler)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ToolDto>>> Get(
        [FromQuery] GetToolsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await getToolsHandler.Handle(
            new GetToolsQuery(
                request.Search,
                request.CategoryId,
                request.ManufacturerId,
                request.StatusId,
                request.SortBy,
                request.Descending,
                request.Page,
                request.PageSize),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ToolStatsDto>> GetStats(
        CancellationToken cancellationToken)
    {
        var result = await getStatsHandler.Handle(
            new GetToolStatsQuery(),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(
        CreateToolRequest request,
        CancellationToken cancellationToken)
    {
        var result = await createHandler.Handle(
            new CreateToolCommand(
                request.ArticleNumber,
                request.Name,
                request.Description,
                request.CategoryId,
                request.ManufacturerId,
                request.StatusId,
                request.RentalPricePerDay,
                request.DepositAmount,
                request.SerialNumber,
                request.InventoryNumber,
                request.CurrentCondition),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateToolRequest request,
        CancellationToken cancellationToken)
    {
        var result = await updateHandler.Handle(
            new UpdateToolCommand(
                id,
                request.ArticleNumber,
                request.Name,
                request.Description,
                request.CategoryId,
                request.ManufacturerId,
                request.RentalPricePerDay,
                request.DepositAmount,
                request.SerialNumber,
                request.InventoryNumber,
                request.CurrentCondition),
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
            new DeleteToolCommand(id),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }
    
    [HttpGet("export")]
    public async Task<IActionResult> Export(
        [FromQuery] ExportToolsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await exportHandler.Handle(
            new ExportToolsQuery(
                request.Search,
                request.CategoryId,
                request.ManufacturerId,
                request.StatusId,
                request.SortBy,
                request.Descending),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return File(
            result.Value,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "tools.xlsx");
    }
    
    [HttpPost("{id:guid}/image")]
    public async Task<IActionResult> UploadImage(
        Guid id,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();

        var result = await uploadImageHandler.Handle(
            new UploadToolImageCommand(
                id,
                stream,
                file.FileName,
                file.Length),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }
    
    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(
        Guid id,
        [FromBody] ChangeToolStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await changeToolStatusHandler.Handle(
            new ChangeToolStatusCommand(
                id,
                request.StatusId),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }
}