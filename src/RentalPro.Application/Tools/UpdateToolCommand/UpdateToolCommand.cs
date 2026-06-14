using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Tools.UpdateToolCommand;

public sealed record UpdateToolCommand(
    Guid Id,
    string ArticleNumber,
    string Name,
    string? Description,
    Guid CategoryId,
    Guid ManufacturerId,
    decimal RentalPricePerDay,
    decimal DepositAmount,
    string SerialNumber,
    string InventoryNumber,
    string? CurrentCondition) : IValidation;