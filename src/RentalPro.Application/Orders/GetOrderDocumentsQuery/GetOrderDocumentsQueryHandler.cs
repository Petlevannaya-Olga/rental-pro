using CSharpFunctionalExtensions;
using RentalPro.Application.Repositories;
using RentalPro.Contracts.Orders;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Orders.GetOrderDocumentsQuery;

public sealed class GetOrderDocumentsHandler(
    IOrdersReadRepository ordersReadRepository)
    : IQueryHandler<IReadOnlyList<OrderDocumentDto>, GetOrderDocumentsQuery>
{
    public async Task<Result<IReadOnlyList<OrderDocumentDto>, Errors>> Handle(
        GetOrderDocumentsQuery query,
        CancellationToken cancellationToken = default)
    {
        return await ordersReadRepository.GetDocumentsAsync(
            query.OrderId,
            cancellationToken);
    }
}