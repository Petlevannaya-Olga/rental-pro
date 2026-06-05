using CSharpFunctionalExtensions;
using RentalPro.Domain.Common;
using RentalPro.Shared;

namespace RentalPro.Domain.Tools;

public sealed class ToolCategory : AuditableEntity<ToolCategoryId>
{
    private ToolCategory(ToolCategoryName name)
        : base(ToolCategoryId.NewId())
    {
        Name = name;
    }

    public ToolCategoryName Name { get; private set; }
    
    public static Result<ToolCategory, Error> Create(string name)
    {
        var nameResult = ToolCategoryName.Create(name);

        if (nameResult.IsFailure)
            return nameResult.Error;

        return new ToolCategory(nameResult.Value);
    }

    public UnitResult<Error> Update(string name)
    {
        var nameResult = ToolCategoryName.Create(name);

        if (nameResult.IsFailure)
            return nameResult.Error;

        Name = nameResult.Value;

        MarkUpdated();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Delete()
    {
        return MarkDeleted(nameof(ToolCategory));
    }
}