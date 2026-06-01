using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.MaintenanceRecords;

public readonly record struct MaintenanceStatusId
{
    public Guid Value { get; }

    private MaintenanceStatusId(Guid value)
    {
        Value = value;
    }

    public static MaintenanceStatusId NewId()
    {
        return new MaintenanceStatusId(Guid.NewGuid());
    }

    public static Result<MaintenanceStatusId, Error> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return CommonErrors.Validation(
                nameof(value),
                "Maintenance status id cannot be empty");
        }

        return new MaintenanceStatusId(value);
    }
    
    public static MaintenanceStatusId Restore(Guid value)
    {
        return new MaintenanceStatusId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}