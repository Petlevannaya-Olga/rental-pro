using Microsoft.AspNetCore.Mvc;
using RentalPro.Application.PaymentMethods.CreatePaymentMethodCommand;
using RentalPro.Application.PaymentMethods.DeletePaymentMethodCommand;
using RentalPro.Application.PaymentMethods.GetPaymentMethodQuery;
using RentalPro.Application.PaymentMethods.UpdatePaymentMethodCommand;
using RentalPro.Contracts.PaymentMethods;

namespace RentalPro.Presentation.Server.Controllers;

[ApiController]
[Route("api/payment-methods")]
public sealed class PaymentMethodsController(
    GetPaymentMethodsHandler getPaymentMethodsHandler,
    UpdatePaymentMethodCommandHandler updatePaymentMethodCommandHandler,
    DeletePaymentMethodCommandHandler deletePaymentMethodCommandHandler,
    CreatePaymentMethodCommandHandler createPaymentMethodCommandHandler)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<PaymentMethodDto>>> Get(
        CancellationToken cancellationToken)
    {
        var result = await getPaymentMethodsHandler.Handle(
            new GetPaymentMethodsQuery(),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
    
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdatePaymentMethodRequest request,
        CancellationToken cancellationToken)
    {
        var result = await updatePaymentMethodCommandHandler.Handle(
            new UpdatePaymentMethodCommand(
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
        var result = await deletePaymentMethodCommandHandler.Handle(
            new DeletePaymentMethodCommand(id),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }
    
    [HttpPost]
    public async Task<IActionResult> Create(
        CreatePaymentMethodRequest request,
        CancellationToken cancellationToken)
    {
        var result = await createPaymentMethodCommandHandler.Handle(
            new CreatePaymentMethodCommand(request.Name),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }
}