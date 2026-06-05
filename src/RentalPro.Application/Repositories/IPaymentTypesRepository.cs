using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using RentalPro.Domain.Payments;
using RentalPro.Shared;

namespace RentalPro.Application.Repositories;

public interface IPaymentTypesRepository
{
    Task<Result<PaymentType, Error>> AddAsync(
        PaymentType paymentType,
        CancellationToken cancellationToken);

    Task<Result<List<PaymentType>, Error>> GetAllAsync(CancellationToken cancellationToken);

    Task<Result<PaymentType?, Error>> GetByAsync(
        Expression<Func<PaymentType, bool>> expression,
        CancellationToken cancellationToken);
}