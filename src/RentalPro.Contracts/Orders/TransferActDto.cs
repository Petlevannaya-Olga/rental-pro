namespace RentalPro.Contracts.Orders;

public sealed record TransferActDto(
    Guid OrderId,
    string ActNumber,
    DateOnly ActDate,
    string ContractNumber,
    DateOnly ContractDate,
    string UserFullName,
    CustomerContractDto Customer,
    decimal TotalRentalPrice,
    decimal TotalDeposit,
    IReadOnlyList<TransferActItemDto> Items);