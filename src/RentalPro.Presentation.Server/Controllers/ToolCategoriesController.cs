using Microsoft.AspNetCore.Mvc;
using RentalPro.Application.ToolCategories.CreateToolCategoriesCommand;
using RentalPro.Application.ToolCategories.DeleteToolCategoriesCommand;
using RentalPro.Application.ToolCategories.GetToolCategoriesQuery;
using RentalPro.Application.ToolCategories.UpdateToolCategoriesCommand;
using RentalPro.Contracts.ToolCategories;

namespace RentalPro.Presentation.Server.Controllers;

[ApiController]
[Route("api/tool-categories")]
public sealed class ToolCategoriesController(
    GetToolCategoriesQueryHandler getHandler,
    CreateToolCategoryCommandHandler createHandler,
    UpdateToolCategoryCommandHandler updateHandler,
    DeleteToolCategoryCommandHandler deleteHandler)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ToolCategoryDto>>> Get(
        CancellationToken cancellationToken)
    {
        var result = await getHandler.Handle(
            new GetToolCategoriesQuery(),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateToolCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await createHandler.Handle(
            new CreateToolCategoryCommand(request.Name),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateToolCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await updateHandler.Handle(
            new UpdateToolCategoryCommand(
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
            new DeleteToolCategoryCommand(id),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }
}