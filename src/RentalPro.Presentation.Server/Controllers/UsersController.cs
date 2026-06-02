using Microsoft.AspNetCore.Mvc;
using RentalPro.Application.Users.CreateUserCommand;
using RentalPro.Contracts.Users;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;
using GetUsersQuery = RentalPro.Application.Users.GetUsersQuery.GetUsersQuery;

namespace RentalPro.Presentation.Server.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController(
    IQueryHandler<PagedResult<UserDto>, GetUsersQuery> getUsersHandler,
    ICommandHandler<Guid, CreateUserCommand> createUserHandler)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUsers(
        [FromQuery] GetUsersRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetUsersQuery(
            request.Search,
            request.RoleId,
            request.IsActive,
            request.CreatedFrom,
            request.CreatedTo,
            request.SortBy,
            request.Descending,
            request.Page,
            request.PageSize);

        var result = await getUsersHandler.Handle(query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateUser(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand(
            request.Login,
            request.Password,
            request.LastName,
            request.FirstName,
            request.MiddleName,
            request.PhoneNumber,
            request.Email,
            request.RoleId);

        var result = await createUserHandler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Created(
            $"/api/users/{result.Value}",
            new CreateUserResponse(result.Value));
    }
}