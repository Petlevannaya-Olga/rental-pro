using RentalPro.Domain.Payments;

namespace RentalPro.Application;

using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using Shared;

public interface IPaymentMethodsRepository
{
    Task<Result<PaymentMethod, Error>> AddAsync(
        PaymentMethod paymentMethod,
        CancellationToken cancellationToken);

    Task<Result<List<PaymentMethod>, Error>> GetAllAsync(
        CancellationToken cancellationToken);

    Task<Result<PaymentMethod?, Error>> GetByAsync(
        Expression<Func<PaymentMethod, bool>> expression,
        CancellationToken cancellationToken);
}