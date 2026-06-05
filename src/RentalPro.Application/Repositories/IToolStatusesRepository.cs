using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using RentalPro.Domain.Tools;
using RentalPro.Shared;

namespace RentalPro.Application.Repositories;

public interface IToolStatusesRepository
{
    Task<Result<ToolStatus, Error>> AddAsync(
        ToolStatus toolStatus,
        CancellationToken cancellationToken);

    Task<Result<ToolStatus?, Error>> GetByAsync(
        Expression<Func<ToolStatus, bool>> expression,
        CancellationToken cancellationToken);

    Task<Result<List<ToolStatus>, Error>> GetAllAsync(
        CancellationToken cancellationToken);
}