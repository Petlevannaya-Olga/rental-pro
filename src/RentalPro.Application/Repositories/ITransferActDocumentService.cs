using CSharpFunctionalExtensions;
using RentalPro.Contracts.Orders;
using RentalPro.Shared;

namespace RentalPro.Application.Repositories;

public interface ITransferActDocumentService
{
    Task<Result<byte[], Errors>> GenerateTransferActAsync(
        TransferActDto act,
        CancellationToken cancellationToken = default);
}