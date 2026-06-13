namespace RentalPro.Contracts.Orders;

public sealed record OrderDetailsDto(
    Guid Id,
    string Number,

    Guid CustomerId,
    string CustomerFullName,

    Guid UserId,
    string UserFullName,

    Guid StatusId,
    string StatusName,

    DateTime OrderDate,
    DateTime CreatedAt,
    DateTime? UpdatedAt,

    string? Comment,
    
    decimal TotalCost,
    decimal DepositTotal,
    
    decimal PaidRentalAmount,
    decimal PaidDepositAmount,
    decimal TotalPaidAmount,
    decimal RemainingRentalAmount,
    decimal RemainingDepositAmount,
    decimal TotalRemainingAmount,
    
    decimal RefundedDepositAmount,
    decimal RemainingDepositRefundAmount,
    bool AllItemsReturned,
    List<OrderDetailsItemDto> Items);