namespace RentalPro.Contracts.Orders;

public sealed record ReturnActDto(
    Guid OrderId,
    string ActNumber,
    DateOnly ActDate,
    string ContractNumber,
    DateOnly ContractDate,
    string UserFullName,
    CustomerContractDto Customer,
    IReadOnlyList<ReturnActItemDto> Items);