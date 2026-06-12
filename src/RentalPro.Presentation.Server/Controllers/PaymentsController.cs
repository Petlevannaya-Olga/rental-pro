using Microsoft.AspNetCore.Mvc;
using RentalPro.Application.Payments;
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
}