using CSharpFunctionalExtensions;
using RentalPro.Application.Repositories;
using RentalPro.Contracts;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Dictionaries.GetDictionaryStatsQuery;

public sealed class GetDictionaryStatsQueryHandler(
    IDictionaryStatsRepository repository)
    : IQueryHandler<DictionaryStatsDto, GetDictionaryStatsQuery>
{
    public async Task<Result<DictionaryStatsDto, Errors>> Handle(
        GetDictionaryStatsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetStatsAsync(cancellationToken);

        if (result.IsFailure)
            return result.Error.ToErrors();

        return result.Value;
    }
}