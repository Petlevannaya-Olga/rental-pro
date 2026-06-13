using Microsoft.AspNetCore.Mvc;
using RentalPro.Application.Payments;
using RentalPro.Application.Payments.CreatePaymentCommand;
using RentalPro.Application.Payments.ExportPaymentsQuery;
using RentalPro.Application.Payments.GetPaymentsQuery;
using RentalPro.Application.Payments.GetPaymentStatsQuery;
using RentalPro.Contracts.Payments;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Presentation.Server.Controllers;

[ApiController]
[Route("api/payments")]
public sealed class PaymentsController(
    ICommandHandler<Guid, CreatePaymentCommand> createPaymentHandler)
    : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CreatePaymentResponse>> CreatePayment(
        [FromBody] CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreatePaymentCommand(
            request.OrderId,
            request.PaymentTypeId,
            request.PaymentMethodId,
            request.Amount,
            request.PaymentDate,
            request.Comment);

        var result = await createPaymentHandler.Handle(
            command,
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(new CreatePaymentResponse(result.Value));
    }
    
    [HttpGet]
    public async Task<IActionResult> GetPayments(
        [FromServices] GetPaymentsQueryHandler handler,
        [FromQuery] GetPaymentsRequest filter,
        CancellationToken cancellationToken)
    {
        var query = new GetPaymentsQuery(
            filter.Search,
            filter.PaymentTypeId,
            filter.PaymentMethodId,
            filter.DateFrom,
            filter.DateTo,
            filter.SortBy,
            filter.Descending,
            filter.Page,
            filter.PageSize);

        var result = await handler.Handle(query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
    
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(
        [FromServices] GetPaymentStatsQueryHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(
            new GetPaymentStatsQuery(),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
    
    [HttpGet("export")]
    public async Task<IActionResult> Export(
        [FromQuery] ExportPaymentsRequest request,
        [FromServices] ExportPaymentsQueryHandler exportHandler,
        CancellationToken cancellationToken)
    {
        var result = await exportHandler.Handle(
            new ExportPaymentsQuery(
                request.Search,
                request.PaymentTypeId,
                request.PaymentMethodId,
                request.DateFrom,
                request.DateTo,
                request.SortBy,
                request.Descending),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return File(
            result.Value,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "payments.xlsx");
    }
}