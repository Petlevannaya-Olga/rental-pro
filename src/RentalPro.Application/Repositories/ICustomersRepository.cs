using RentalPro.Domain.Customers;

namespace RentalPro.Application.Repositories;

public interface ICustomersRepository
{
    Task<Customer?> GetByIdAsync(
        CustomerId id,
        CancellationToken cancellationToken);

    Task AddAsync(
        Customer customer,
        CancellationToken cancellationToken);

    void Delete(Customer customer);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);
}