using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.Roles;

public readonly record struct RoleId
{
    public Guid Value { get; }

    private RoleId(Guid value)
    {
        Value = value;
    }

    public static RoleId NewId()
    {
        return new RoleId(Guid.NewGuid());
    }

    public static Result<RoleId, Error> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return CommonErrors.Validation(
                nameof(value),
                "Role id cannot be empty");
        }

        return new RoleId(value);
    }
    
    public static RoleId Restore(Guid value)
    {
        return new RoleId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}