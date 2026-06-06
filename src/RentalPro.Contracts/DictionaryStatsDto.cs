namespace RentalPro.Contracts;

public sealed record DictionaryStatsDto(
    int PaymentMethods,
    int PaymentTypes,
    int OrderStatuses,
    int ToolCategories,
    int ToolStatuses,
    int Roles,
    int Manufacturers,
    int Suppliers);