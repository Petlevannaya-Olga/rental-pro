using CSharpFunctionalExtensions;
using RentalPro.Domain.Common;
using RentalPro.Domain.ValueObjects;
using RentalPro.Shared;

namespace RentalPro.Domain.Tools;

public sealed class ToolCategory : AuditableEntity<ToolCategoryId>
{
    private ToolCategory(
        ToolCategoryName name,
        Description? description)
        : base(ToolCategoryId.NewId())
    {
        Name = name;
        Description = description;
    }

    public ToolCategoryName Name { get; private set; }

    public Description? Description { get; private set; }

    public static Result<ToolCategory, Error> Create(
        string name,
        string? description)
    {
        var nameResult = ToolCategoryName.Create(name);

        if (nameResult.IsFailure)
            return nameResult.Error;

        var descriptionResult = CreateDescription(description);

        if (descriptionResult.IsFailure)
            return descriptionResult.Error;

        return new ToolCategory(
            nameResult.Value,
            descriptionResult.Value);
    }

    public UnitResult<Error> Update(
        string name,
        string? description)
    {
        var nameResult = ToolCategoryName.Create(name);

        if (nameResult.IsFailure)
            return nameResult.Error;

        var descriptionResult = CreateDescription(description);

        if (descriptionResult.IsFailure)
            return descriptionResult.Error;

        Name = nameResult.Value;
        Description = descriptionResult.Value;

        MarkUpdated();

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Delete()
    {
        return MarkDeleted(nameof(ToolCategory));
    }

    private static Result<Description?, Error> CreateDescription(
        string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return (Description?)null;

        var descriptionResult = Description.Create(description);

        if (descriptionResult.IsFailure)
            return descriptionResult.Error;

        return descriptionResult.Value;
    }
}