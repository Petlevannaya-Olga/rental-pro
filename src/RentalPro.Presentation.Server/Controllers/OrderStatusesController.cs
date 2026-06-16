using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalPro.Application.OrderStatuses.CreateOrderStatusCommand;
using RentalPro.Application.OrderStatuses.DeleteOrderStatusCommand;
using RentalPro.Application.OrderStatuses.GetOrderStatusesQuery;
using RentalPro.Application.OrderStatuses.UpdateOrderStatusCommand;
using RentalPro.Contracts.OrderStatuses;

namespace RentalPro.Presentation.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/order-statuses")]
public sealed class OrderStatusesController(
    GetOrderStatusesQueryHandler getHandler,
    CreateOrderStatusCommandHandler createHandler,
    UpdateOrderStatusCommandHandler updateHandler,
    DeleteOrderStatusCommandHandler deleteHandler)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<OrderStatusDto>>> Get(
        CancellationToken cancellationToken)
    {
        var result = await getHandler.Handle(
            new GetOrderStatusesQuery(),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await createHandler.Handle(
            new CreateOrderStatusCommand(request.Name),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await updateHandler.Handle(
            new UpdateOrderStatusCommand(
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
            new DeleteOrderStatusCommand(id),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }
}