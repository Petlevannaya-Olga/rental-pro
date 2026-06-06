using CSharpFunctionalExtensions;
using RentalPro.Contracts;
using RentalPro.Shared;

namespace RentalPro.Application.Repositories;

public interface IDictionaryStatsRepository
{
    Task<Result<DictionaryStatsDto, Error>> GetStatsAsync(
        CancellationToken cancellationToken);
}