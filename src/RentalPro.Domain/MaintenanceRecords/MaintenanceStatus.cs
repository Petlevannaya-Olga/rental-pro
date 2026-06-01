using CSharpFunctionalExtensions;
using RentalPro.Domain.Common;
using RentalPro.Shared;

namespace RentalPro.Domain.MaintenanceRecords;

public sealed class MaintenanceStatus : AuditableEntity<MaintenanceStatusId>
{
    private MaintenanceStatus(MaintenanceStatusName name)
        : base(MaintenanceStatusId.NewId())
    {
        Name = name;
    }

    public MaintenanceStatusName Name { get; private set; }

    public static Result<MaintenanceStatus, Error> Create(string name)
    {
        var nameResult = MaintenanceStatusName.Create(name);

        if (nameResult.IsFailure)
            return nameResult.Error;

        return new MaintenanceStatus(nameResult.Value);
    }

    public UnitResult<Error> Update(string name)
    {
        var nameResult = MaintenanceStatusName.Create(name);

        if (nameResult.IsFailure)
            return nameResult.Error;

        Name = nameResult.Value;

        MarkUpdated();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Delete()
    {
        return MarkDeleted(nameof(MaintenanceStatus));
    }
}