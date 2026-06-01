using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.MaintenanceRecords;

public readonly record struct MaintenanceRecordId
{
    public Guid Value { get; }

    private MaintenanceRecordId(Guid value)
    {
        Value = value;
    }

    public static MaintenanceRecordId NewId()
    {
        return new MaintenanceRecordId(Guid.NewGuid());
    }

    public static Result<MaintenanceRecordId, Error> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return CommonErrors.Validation(
                nameof(value),
                "Maintenance record id cannot be empty");
        }

        return new MaintenanceRecordId(value);
    }
    
    public static MaintenanceRecordId Restore(Guid value)
    {
        return new MaintenanceRecordId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}