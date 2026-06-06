using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using RentalPro.Domain.Tools;
using RentalPro.Shared;

namespace RentalPro.Application.Repositories;

public interface IToolsRepository
{
    Task<Result<Tool?, Error>> GetByAsync(
        Expression<Func<Tool, bool>> expression,
        CancellationToken cancellationToken);

    Task<Result<Tool, Error>> AddAsync(
        Tool tool,
        CancellationToken cancellationToken);
}