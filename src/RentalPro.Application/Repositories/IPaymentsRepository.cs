using RentalPro.Domain.Payments;

namespace RentalPro.Application.Repositories;

public interface IPaymentsRepository
{
    Task AddAsync(
        Payment payment,
        CancellationToken cancellationToken = default);
}