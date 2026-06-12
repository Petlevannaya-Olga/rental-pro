using CSharpFunctionalExtensions;
using RentalPro.Contracts.Orders;
using RentalPro.Shared;

namespace RentalPro.Application.Services;

public interface IReturnActDocumentService
{
    Task<Result<byte[], Errors>> GenerateReturnActAsync(
        ReturnActDto act,
        CancellationToken cancellationToken = default);
}