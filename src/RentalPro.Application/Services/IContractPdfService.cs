using CSharpFunctionalExtensions;
using RentalPro.Contracts.Orders;
using RentalPro.Shared;

namespace RentalPro.Application.Services;

public interface IContractPdfService
{
    Task<Result<byte[], Errors>> GenerateRentalContractPdfAsync(
        RentalContractDto contract,
        CancellationToken cancellationToken = default);
}