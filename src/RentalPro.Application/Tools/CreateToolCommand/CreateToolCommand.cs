using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Tools.CreateToolCommand;

public sealed record CreateToolCommand(
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