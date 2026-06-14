using CSharpFunctionalExtensions;
using RentalPro.Contracts.Orders;
using RentalPro.Contracts.Payments;
using RentalPro.Domain.Orders;
using RentalPro.Domain.Payments;
using RentalPro.Shared;

namespace RentalPro.Application.Repositories;

public interface IOrdersReadRepository
{
    Task<Result<PagedResult<OrderDto>, Errors>> GetPagedAsync(
        string? search,
        OrderStatusId? statusId,
        DateOnly? startFrom,
        DateOnly? startTo,
        DateOnly? endFrom,
        DateOnly? endTo,
        string? sortBy,
        bool descending,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<Result<OrderStatsDto, Errors>> GetStatsAsync(
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<OrderDto>, Errors>> GetForExportAsync(
        string? search,
        OrderStatusId? statusId,
        DateOnly? startFrom,
        DateOnly? startTo,
        DateOnly? endFrom,
        DateOnly? endTo,
        string? sortBy,
        bool descending,
        CancellationToken cancellationToken);
    
    Task<Result<OrderDetailsDto, Errors>> GetByIdAsync(
        OrderId orderId,
        CancellationToken cancellationToken);
    
    Task<Result<RentalContractDto, Errors>> GetContractDataAsync(
        OrderId orderId,
        CancellationToken cancellationToken = default);
    
    Task<Result<IReadOnlyList<OrderDocumentDto>, Errors>> GetDocumentsAsync(
        OrderId orderId,
        CancellationToken cancellationToken = default);
    
    Task<Result<TransferActDto, Errors>> GetTransferActDataAsync(
        OrderId orderId,
        DateOnly actDate,
        CancellationToken cancellationToken = default);
    
    Task<Result<ReturnActDto, Errors>> GetReturnActDataAsync(
        OrderId orderId,
        DateOnly actDate,
        CancellationToken cancellationToken = default);
    
    Task<Result<PaymentFiscalizationDto, Errors>> GetPaymentFiscalizationDataAsync(
        PaymentId paymentId,
        CancellationToken cancellationToken = default);
    
    Task<Result<bool, Errors>> CustomerHasOrdersAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);
}