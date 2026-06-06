
namespace RentalPro.Contracts.Manufacturers;

public sealed record ManufacturerDto(
    Guid Id,
    string Name,
    string Country,
    DateTime CreatedAt,
    DateTime? UpdatedAt);