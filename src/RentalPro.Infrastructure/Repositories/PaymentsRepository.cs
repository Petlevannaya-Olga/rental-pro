using RentalPro.Application.Repositories;
using RentalPro.Domain.Payments;
using Microsoft.Extensions.Logging;

namespace RentalPro.Infrastructure.Repositories;

public sealed class PaymentsRepository(
    ApplicationDbContext dbContext,
    ILogger<PaymentsRepository> logger)
    : IPaymentsRepository
{
    public async Task AddAsync(
        Payment payment,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Adding payment {PaymentId}",
            payment.Id.Value);

        await dbContext.Payments.AddAsync(
            payment,
            cancellationToken);
    }
}