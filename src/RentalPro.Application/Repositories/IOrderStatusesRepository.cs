using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using RentalPro.Domain.Orders;
using RentalPro.Shared;

namespace RentalPro.Application.Repositories;

public interface IOrderStatusesRepository
{
    Task<Result<OrderStatus, Error>> AddAsync(
        OrderStatus orderStatus,
        CancellationToken cancellationToken);

    Task<Result<List<OrderStatus>, Error>> GetAllAsync(CancellationToken cancellationToken);

    Task<Result<OrderStatus?, Error>> GetByAsync(
        Expression<Func<OrderStatus, bool>> expression,
        CancellationToken cancellationToken);
}