using CSharpFunctionalExtensions;
using RentalPro.Application.Repositories;
using RentalPro.Contracts.Manufacturers;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Manufactures.GetManufacturesQuery;

public sealed class GetManufacturersQueryHandler(
    IManufacturerRepository repository)
    : IQueryHandler<IReadOnlyList<ManufacturerDto>, GetManufacturersQuery>
{
    public async Task<Result<IReadOnlyList<ManufacturerDto>, Errors>> Handle(
        GetManufacturersQuery query,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetAllAsync(
            cancellationToken);

        if (result.IsFailure)
            return result.Error.ToErrors();

        return result.Value
            .Select(x => new ManufacturerDto(
                x.Id.Value,
                x.Name.Value,
                x.Country.Value,
                x.CreatedAt,
                x.UpdatedAt))
            .ToList();
    }
}