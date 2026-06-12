using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using RentalPro.Application.Services;
using RentalPro.Contracts.Payments;
using RentalPro.Shared;

namespace RentalPro.Infrastructure.Services;

public sealed class TestOrangeDataFiscalReceiptService(
    ILogger<TestOrangeDataFiscalReceiptService> logger)
    : IFiscalReceiptService
{
    public Task<Result<FiscalReceiptResult, Errors>> CreateReceiptAsync(
        PaymentFiscalizationDto payment,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Test Orange Data receipt created for payment {PaymentId}",
            payment.PaymentId);

        var result = new FiscalReceiptResult(
            ReceiptId: $"TEST-OD-{DateTime.UtcNow:yyyyMMddHHmmss}-{payment.PaymentId.ToString()[..6]}",
            Status: "Fiscalized",
            FiscalizedAt: DateTime.UtcNow,
            ErrorMessage: null);

        return Task.FromResult<Result<FiscalReceiptResult, Errors>>(result);
    }
}