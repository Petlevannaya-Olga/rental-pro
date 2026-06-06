using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using RentalPro.Domain.Manufacturers;
using RentalPro.Shared;

namespace RentalPro.Application.Repositories;

public interface IManufacturerRepository
{
    Task<Result<Manufacturer, Error>> AddAsync(
        Manufacturer manufacturer,
        CancellationToken cancellationToken);

    Task<Result<Manufacturer?, Error>> GetByAsync(
        Expression<Func<Manufacturer, bool>> expression,
        CancellationToken cancellationToken);

    Task<Result<List<Manufacturer>, Error>> GetAllAsync(
        CancellationToken cancellationToken);
}