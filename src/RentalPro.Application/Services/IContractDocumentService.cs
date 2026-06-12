using CSharpFunctionalExtensions;
using RentalPro.Contracts.Orders;
using RentalPro.Shared;

namespace RentalPro.Application.Services;

public interface IContractDocumentService
{
    Task<Result<byte[], Errors>> GenerateRentalContractAsync(
        RentalContractDto contract,
        CancellationToken cancellationToken = default);
}