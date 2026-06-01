using CSharpFunctionalExtensions;
using RentalPro.Domain.Common;
using RentalPro.Shared;

namespace RentalPro.Domain.Roles;

public sealed class Role : AuditableEntity<RoleId>
{
    private Role(RoleName name)
        : base(RoleId.NewId())
    {
        Name = name;
    }

    public RoleName Name { get; private set; }

    public static Result<Role, Error> Create(string name)
    {
        var roleNameResult = RoleName.Create(name);

        if (roleNameResult.IsFailure)
            return roleNameResult.Error;

        return new Role(roleNameResult.Value);
    }

    public UnitResult<Error> Update(string name)
    {
        var roleNameResult = RoleName.Create(name);

        if (roleNameResult.IsFailure)
            return roleNameResult.Error;

        Name = roleNameResult.Value;

        MarkUpdated();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Delete()
    {
        return MarkDeleted(nameof(Role));
    }
}