namespace RentalPro.Contracts.ToolStatuses;

public sealed record ToolStatusDto(
    Guid Id,
    string Name,
    DateTime CreatedAt,
    DateTime? UpdatedAt);