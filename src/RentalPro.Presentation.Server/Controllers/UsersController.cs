using Microsoft.AspNetCore.Mvc;
using RentalPro.Application;
using RentalPro.Application.Repositories;
using RentalPro.Application.Services;
using RentalPro.Application.Users.ChangeUserPasswordCommand;
using RentalPro.Application.Users.ChangeUserStatusCommand;
using RentalPro.Application.Users.CreateUserCommand;
using RentalPro.Application.Users.DeleteUserCommand;
using RentalPro.Application.Users.UpdateUserCommand;
using RentalPro.Contracts.Users;
using RentalPro.Domain.Roles;
using RentalPro.Domain.Users;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;
using GetUsersQuery = RentalPro.Application.Users.GetUsersQuery.GetUsersQuery;

namespace RentalPro.Presentation.Server.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController(
    IUsersReadRepository usersReadRepository,
    IExcelExportService<UserDto> usersExportService,
    IQueryHandler<PagedResult<UserDto>, GetUsersQuery> getUsersHandler,
    ICommandHandler<Guid, CreateUserCommand> createUserHandler,
    ICommandHandler<ChangeUserStatusCommand> changeUserStatusHandler,
    ICommandHandler<UpdateUserCommand> updateUserCommandHandler,
    ICommandHandler<ChangeUserPasswordCommand> changeUserPasswordHandler,
    ICommandHandler<DeleteUserCommand> deleteUserHandler)
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
    
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(
        CancellationToken cancellationToken)
    {
        var result = await usersReadRepository.GetStatsAsync(cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
    
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(
        Guid id,
        [FromBody] ChangeUserStatusRequest request,
        CancellationToken cancellationToken)
    {
        var userIdResult = UserId.Create(id);

        if (userIdResult.IsFailure)
            return BadRequest(userIdResult.Error);

        var command = new ChangeUserStatusCommand(
            userIdResult.Value,
            request.IsActive);

        var result = await changeUserStatusHandler.Handle(
            command,
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }
    
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUser(
        Guid id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateUserCommand(
            id,
            request.LastName,
            request.FirstName,
            request.MiddleName,
            request.Login,
            request.Email,
            request.PhoneNumber,
            request.RoleId);

        var result = await updateUserCommandHandler.Handle(
            command,
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }
    
    [HttpPatch("{id:guid}/password")]
    public async Task<IActionResult> ChangePassword(
        Guid id,
        [FromBody] ChangeUserPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangeUserPasswordCommand(
            id,
            request.OldPassword,
            request.NewPassword);

        var result = await changeUserPasswordHandler.Handle(
            command,
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUser(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteUserCommand(id);

        var result = await deleteUserHandler.Handle(
            command,
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }
    
    [HttpGet("export")]
    public async Task<IActionResult> ExportUsers(
        [FromQuery] ExportUsersRequest request,
        CancellationToken cancellationToken)
    {
        RoleId? roleId = null;

        if (request.RoleId.HasValue)
        {
            var roleIdResult = RoleId.Create(request.RoleId.Value);

            if (roleIdResult.IsFailure)
                return BadRequest(roleIdResult.Error);

            roleId = roleIdResult.Value;
        }

        var usersResult = await usersReadRepository.GetForExportAsync(
            request.Search,
            roleId,
            request.IsActive,
            request.CreatedFrom,
            request.CreatedTo,
            request.SortBy,
            request.Descending,
            cancellationToken);

        if (usersResult.IsFailure)
            return BadRequest(usersResult.Error);

        var fileBytes = usersExportService.Export(usersResult.Value);

        var fileName = $"users_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";

        return File(
            fileBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }
}