using CSharpFunctionalExtensions;
using RentalPro.Domain.Payments;
using RentalPro.Shared;

namespace RentalPro.Application.Repositories;

public interface IPaymentsRepository
{
    Task<UnitResult<Errors>> AddAsync(
        Payment payment,
        CancellationToken cancellationToken = default);
}