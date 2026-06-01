using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.Common;

public abstract class AuditableEntity<TId>(TId id)
    where TId : notnull
{
    public TId Id { get; protected set; } = id;

    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; protected set; }

    public DateTime? DeletedAt { get; protected set; }

    public bool IsDeleted => DeletedAt.HasValue;

    protected void MarkUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    protected UnitResult<Error> MarkDeleted(string entityName)
    {
        if (DeletedAt.HasValue)
        {
            return CommonErrors.Validation(
                nameof(DeletedAt),
                $"{entityName} is already deleted");
        }

        DeletedAt = DateTime.UtcNow;

        return UnitResult.Success<Error>();
    }
}