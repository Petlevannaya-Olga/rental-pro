namespace RentalPro.Contracts.Orders;

public sealed record RentalContractDto(
    Guid OrderId,
    string ContractNumber,
    DateOnly ContractDate,
    string UserFullName,
    CustomerContractDto Customer,
    decimal TotalRentalPrice,
    decimal TotalDeposit,
    decimal TotalAmount);