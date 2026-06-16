using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalPro.Application.Dictionaries.GetDictionaryStatsQuery;
using RentalPro.Contracts;

namespace RentalPro.Presentation.Server.Controllers;

[Authorize(Roles = "Администратор")]
[ApiController]
[Route("api/dictionaries")]
public sealed class DictionariesController(
    GetDictionaryStatsQueryHandler getDictionaryStatsQueryHandler)
    : ControllerBase
{
    [HttpGet("stats")]
    public async Task<ActionResult<DictionaryStatsDto>> GetStats(
        CancellationToken cancellationToken)
    {
        var result = await getDictionaryStatsQueryHandler.Handle(
            new GetDictionaryStatsQuery(),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
}