using Microsoft.EntityFrameworkCore;
using RentalPro.Application.Repositories;
using RentalPro.Domain.Customers;

namespace RentalPro.Infrastructure.Repositories;

public sealed class CustomersRepository(
    ApplicationDbContext dbContext)
    : ICustomersRepository
{
    public async Task<Customer?> GetByIdAsync(
        CustomerId id,
        CancellationToken cancellationToken)
    {
        return await dbContext.Customers
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task AddAsync(
        Customer customer,
        CancellationToken cancellationToken)
    {
        await dbContext.Customers.AddAsync(
            customer,
            cancellationToken);
    }

    public void Delete(Customer customer)
    {
        customer.Delete();
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}