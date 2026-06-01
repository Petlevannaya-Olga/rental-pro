using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.Users;

public readonly record struct UserId
{
    public Guid Value { get; }

    private UserId(Guid value)
    {
        Value = value;
    }

    public static UserId NewId()
    {
        return new UserId(Guid.NewGuid());
    }

    public static Result<UserId, Error> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return CommonErrors.Validation(
                nameof(value),
                "User id cannot be empty");
        }

        return new UserId(value);
    }
    
    public static UserId Restore(Guid value)
    {
        return new UserId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}