using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using RentalPro.Domain.Tools;
using RentalPro.Shared;

namespace RentalPro.Application.Repositories;

public interface IToolCategoriesRepository
{
    Task<Result<ToolCategory, Error>> AddAsync(
        ToolCategory toolCategory,
        CancellationToken cancellationToken);

    Task<Result<ToolCategory?, Error>> GetByAsync(
        Expression<Func<ToolCategory, bool>> expression,
        CancellationToken cancellationToken);

    Task<Result<List<ToolCategory>, Error>> GetAllAsync(
        CancellationToken cancellationToken);
}