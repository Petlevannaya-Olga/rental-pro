using RentalPro.Application.Repositories;
using RentalPro.Domain.Payments;
using Microsoft.Extensions.Logging;
using RentalPro.Shared;

using CSharpFunctionalExtensions;

namespace RentalPro.Infrastructure.Repositories;

public sealed class PaymentsRepository(
    ApplicationDbContext dbContext,
    ILogger<PaymentsRepository> logger)
    : IPaymentsRepository
{
    public async Task<UnitResult<Errors>> AddAsync(
        Payment payment,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "Adding payment {PaymentId}",
                payment.Id.Value);

            await dbContext.Payments.AddAsync(
                payment,
                cancellationToken);

            return UnitResult.Success<Errors>();
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning(
                "Adding payment was cancelled. PaymentId: {PaymentId}",
                payment.Id.Value);

            return CommonErrors
                .OperationCancelled("add.payment.was.cancelled")
                .ToErrors();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to add payment {PaymentId}",
                payment.Id.Value);

            return CommonErrors.Db(
                    "add.payment.to.db.exception",
                    "Не удалось добавить оплату")
                .ToErrors();
        }
    }
}