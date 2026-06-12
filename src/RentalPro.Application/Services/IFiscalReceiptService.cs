using CSharpFunctionalExtensions;
using RentalPro.Contracts.Payments;
using RentalPro.Shared;

namespace RentalPro.Application.Services;

public interface IFiscalReceiptService
{
    Task<Result<FiscalReceiptResult, Errors>> CreateReceiptAsync(
        PaymentFiscalizationDto payment,
        CancellationToken cancellationToken = default);
}