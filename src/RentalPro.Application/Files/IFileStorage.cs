using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Application.Files;

public interface IFileStorage
{
    Task<Result<string, Error>> SaveToolImageAsync(
        Stream stream,
        string originalFileName,
        CancellationToken cancellationToken);
    
    Task<UnitResult<Error>> DeleteToolImageAsync(
        string photoPath,
        CancellationToken cancellationToken);
}