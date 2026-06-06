using System.Xml;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Extensions;
using RentalPro.Application.Repositories;
using RentalPro.Contracts.Roles;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Roles.GetRolesQuery;

public sealed class GetRolesQueryHandler(
    IRoleRepository roleRepository,
    ILogger<GetRolesQueryHandler> logger)
    : IQueryHandler<List<RoleDto>, GetRolesQuery>
{
    public async Task<Result<List<RoleDto>, Errors>> Handle(
        GetRolesQuery query,
        CancellationToken cancellationToken)
    {
        var rolesResult = await roleRepository.GetAllAsync(
            cancellationToken);

        if (rolesResult.IsFailure)
            return rolesResult.Error.ToErrors();

        return rolesResult.Value
            .Select(x => x.ToDto())
            .ToList();
    }
}