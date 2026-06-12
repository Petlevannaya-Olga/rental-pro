using Microsoft.AspNetCore.Mvc;
using RentalPro.Application.Orders.CompleteOrderCommand;
using RentalPro.Application.Orders.CreateOrderCommand;
using RentalPro.Application.Orders.DeleteOrderCommand;
using RentalPro.Application.Orders.ExportOrdersQuery;
using RentalPro.Application.Orders.GetOrderByIdQuery;
using RentalPro.Application.Orders.GetOrdersQuery;
using RentalPro.Application.Orders.GetOrderStatsQuery;
using RentalPro.Application.Orders.UpdateOrderCommand;
using RentalPro.Application.Orders.UpdateOrderItemRentalPeriodCommand;
using RentalPro.Contracts.Orders;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Presentation.Server.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController(
    IQueryHandler<PagedResult<OrderDto>, GetOrdersQuery> getOrdersHandler,
    IQueryHandler<OrderStatsDto, GetOrderStatsQuery> getStatsHandler,
    ICommandHandler<Guid, CreateOrderCommand> createOrderHandler,
    ICommandHandler<UpdateOrderCommand> updateOrderHandler,
    ICommandHandler<UpdateOrderItemRentalPeriodCommand> updateRentalPeriodHandler,
    ICommandHandler<CompleteOrderCommand> completeOrderHandler,
    ICommandHandler<DeleteOrderCommand> deleteOrderHandler,
    IQueryHandler<byte[], ExportOrdersQuery> exportOrdersHandler,
    IQueryHandler<OrderDetailsDto, GetOrderByIdQuery> queryHandler)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<OrderDto>>> GetOrders(
        [FromQuery] GetOrdersRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetOrdersQuery(
            request.Search,
            request.StatusId,
            request.StartFrom,
            request.StartTo,
            request.EndFrom,
            request.EndTo,
            request.SortBy,
            request.Descending,
            request.Page,
            request.PageSize);

        var result = await getOrdersHandler.Handle(
            query,
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("stats")]
    public async Task<ActionResult<OrderStatsDto>> GetStats(
        CancellationToken cancellationToken)
    {
        var result = await getStatsHandler.Handle(
            new GetOrderStatsQuery(),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<ActionResult<CreateOrderResponse>> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateOrderCommand(
            request.UserId,
            request.CustomerId,
            request.StatusId,
            request.OrderDate,
            request.DepositTotal,
            request.Comment,
            request.Items);

        var result = await createOrderHandler.Handle(
            command,
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(new CreateOrderResponse(result.Value));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateOrder(
        Guid id,
        [FromBody] UpdateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateOrderCommand(
            id,
            request.StatusId,
            request.Comment);

        var result = await updateOrderHandler.Handle(
            command,
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }

    [HttpPut("items/{orderItemId:guid}/rental-period")]
    public async Task<IActionResult> UpdateRentalPeriod(
        Guid orderItemId,
        [FromBody] UpdateOrderItemRentalPeriodRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateOrderItemRentalPeriodCommand(
            orderItemId,
            request.StartDate,
            request.PlannedReturnDate);

        var result = await updateRentalPeriodHandler.Handle(
            command,
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> CompleteOrder(
        Guid id,
        [FromBody] CompleteOrderRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CompleteOrderCommand(
            id,
            request.CompletedStatusId);

        var result = await completeOrderHandler.Handle(
            command,
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteOrder(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await deleteOrderHandler.Handle(
            new DeleteOrderCommand(id),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportOrders(
        [FromQuery] ExportOrdersRequest request,
        CancellationToken cancellationToken)
    {
        var query = new ExportOrdersQuery(
            request.Search,
            request.StatusId,
            request.StartFrom,
            request.StartTo,
            request.EndFrom,
            request.EndTo,
            request.SortBy,
            request.Descending);

        var result = await exportOrdersHandler.Handle(
            query,
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return File(
            result.Value,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "orders.xlsx");
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetOrderByIdQuery(id);

        var result = await queryHandler.Handle(
            query,
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
}