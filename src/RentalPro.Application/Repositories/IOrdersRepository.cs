using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using RentalPro.Domain.Orders;
using RentalPro.Shared;

namespace RentalPro.Application.Repositories;

public interface IOrdersRepository
{
    Task<Result<Order, Error>> AddAsync(
        Order order,
        CancellationToken cancellationToken);

    Task<Result<Order?, Error>> GetByAsync(
        Expression<Func<Order, bool>> expression,
        CancellationToken cancellationToken);

    Task<Result<OrderItem, Error>> AddItemAsync(
        OrderItem orderItem,
        CancellationToken cancellationToken);

    Task<Result<OrderItem?, Error>> GetItemByAsync(
        Expression<Func<OrderItem, bool>> expression,
        CancellationToken cancellationToken);
    
    Task<Result<IReadOnlyList<OrderItem>, Error>> GetItemsAsync(
        OrderId orderId,
        CancellationToken cancellationToken);
}