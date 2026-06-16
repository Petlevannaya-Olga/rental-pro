using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalPro.Application.Dashboard.GetDashboardQuery;
using RentalPro.Contracts.Dashboard;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Presentation.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController(
    IQueryHandler<DashboardDto, GetDashboardQuery> handler)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get(
        CancellationToken cancellationToken)
    {
        var query = new GetDashboardQuery();

        var result = await handler.Handle(
            query,
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
}