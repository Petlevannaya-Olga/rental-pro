using Microsoft.AspNetCore.Mvc;
using RentalPro.Application.Customers.CreateCustomerCommand;
using RentalPro.Application.Customers.DeleteCustomerCommand;
using RentalPro.Application.Customers.ExportCustomersQuery;
using RentalPro.Application.Customers.GetCustomersQuery;
using RentalPro.Application.Customers.GetCustomersStatsQuery;
using RentalPro.Application.Customers.UpdateCustomerCommand;
using RentalPro.Contracts.Customers;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Presentation.Server.Controllers;

[ApiController]
[Route("api/customers")]
public sealed class CustomersController(
    IQueryHandler<PagedResult<CustomerDto>, GetCustomersQuery> getCustomersHandler,
    IQueryHandler<CustomerStatsDto, GetCustomerStatsQuery> getStatsHandler,
    ICommandHandler<Guid, CreateCustomerCommand> createCustomerHandler,
    ICommandHandler<UpdateCustomerCommand> updateCustomerHandler,
    ICommandHandler<DeleteCustomerCommand> deleteCustomerHandler,
    IQueryHandler<byte[], ExportCustomersQuery> exportCustomersHandler)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<CustomerDto>>> GetCustomers(
        [FromQuery] GetCustomersRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetCustomersQuery(
            request.Search,
            request.HasOrders,
            request.HasDebt,
            request.SortBy,
            request.Descending,
            request.Page,
            request.PageSize);

        var result = await getCustomersHandler.Handle(
            query,
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("stats")]
    public async Task<ActionResult<CustomerStatsDto>> GetStats(
        CancellationToken cancellationToken)
    {
        var result = await getStatsHandler.Handle(
            new GetCustomerStatsQuery(),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
    
    [HttpPost]
    public async Task<ActionResult<CreateCustomerResponse>> CreateCustomer(
        [FromBody] CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateCustomerCommand(
            request.LastName,
            request.FirstName,
            request.MiddleName,
            request.PhoneNumber,
            request.Email,
            request.PassportSeries,
            request.PassportNumber,
            request.PostalCode,
            request.Region,
            request.City,
            request.Street,
            request.House,
            request.Building,
            request.Apartment);

        var result = await createCustomerHandler.Handle(
            command,
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        var fullName = $"{request.LastName} {request.FirstName} {request.MiddleName}";

        return Ok(new CreateCustomerResponse(
            result.Value,
            fullName));
    }
    
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCustomer(
        Guid id,
        [FromBody] UpdateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCustomerCommand(
            id,
            request.LastName,
            request.FirstName,
            request.MiddleName,
            request.PhoneNumber,
            request.Email,
            request.PassportSeries,
            request.PassportNumber,
            request.PostalCode,
            request.Region,
            request.City,
            request.Street,
            request.House,
            request.Building,
            request.Apartment);

        var result = await updateCustomerHandler.Handle(
            command,
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCustomer(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await deleteCustomerHandler.Handle(
            new DeleteCustomerCommand(id),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }
    
    [HttpGet("export")]
    public async Task<IActionResult> ExportCustomers(
        [FromQuery] ExportCustomersRequest request,
        CancellationToken cancellationToken)
    {
        var query = new ExportCustomersQuery(
            request.Search,
            request.HasOrders,
            request.HasDebt,
            request.SortBy,
            request.Descending);

        var result = await exportCustomersHandler.Handle(
            query,
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return File(
            result.Value,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "customers.xlsx");
    }
}