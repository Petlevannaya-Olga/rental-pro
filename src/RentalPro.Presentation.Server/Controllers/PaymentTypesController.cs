using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalPro.Application.PaymentTypes.CreatePaymentTypeCommand;
using RentalPro.Application.PaymentTypes.DeletePaymentTypeCommand;
using RentalPro.Application.PaymentTypes.GetPaymentTypeQuery;
using RentalPro.Application.PaymentTypes.UpdatePaymentTypeCommand;
using RentalPro.Contracts.PaymentTypes;

namespace RentalPro.Presentation.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/payment-types")]
public sealed class PaymentTypesController(
    GetPaymentTypesHandler getPaymentTypesHandler,
    UpdatePaymentTypeCommandHandler updatePaymentTypeCommandHandler,
    DeletePaymentTypeCommandHandler deletePaymentTypeCommandHandler,
    CreatePaymentTypeCommandHandler createPaymentTypeCommandHandler)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<PaymentTypeDto>>> Get(
        CancellationToken cancellationToken)
    {
        var result = await getPaymentTypesHandler.Handle(
            new GetPaymentTypesQuery(),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
    
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdatePaymentTypeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await updatePaymentTypeCommandHandler.Handle(
            new UpdatePaymentTypeCommand(
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
        var result = await deletePaymentTypeCommandHandler.Handle(
            new DeletePaymentTypeCommand(id),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }
    
    [HttpPost]
    public async Task<IActionResult> Create(
        CreatePaymentTypeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await createPaymentTypeCommandHandler.Handle(
            new CreatePaymentTypeCommand(request.Name),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }
}