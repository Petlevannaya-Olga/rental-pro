using CSharpFunctionalExtensions;
using RentalPro.Domain.Common;
using RentalPro.Shared;

namespace RentalPro.Domain.Tools;

public sealed class ToolStatus : AuditableEntity<ToolStatusId>
{
    private ToolStatus(ToolStatusName name)
        : base(ToolStatusId.NewId())
    {
        Name = name;
    }

    public ToolStatusName Name { get; private set; }

    public static Result<ToolStatus, Error> Create(string name)
    {
        var nameResult = ToolStatusName.Create(name);

        if (nameResult.IsFailure)
            return nameResult.Error;

        return new ToolStatus(nameResult.Value);
    }

    public UnitResult<Error> Update(string name)
    {
        var nameResult = ToolStatusName.Create(name);

        if (nameResult.IsFailure)
            return nameResult.Error;

        Name = nameResult.Value;

        MarkUpdated();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Delete()
    {
        return MarkDeleted(nameof(ToolStatus));
    }
}