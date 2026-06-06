using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using RentalPro.Domain.Roles;
using RentalPro.Shared;

namespace RentalPro.Application.Repositories;

public interface IRoleRepository
{
    Task<Result<Role?, Error>> GetByAsync(
        Expression<Func<Role, bool>> expression,
        CancellationToken cancellationToken);

    Task<Result<List<Role>, Error>> GetAllAsync(CancellationToken cancellationToken);
    
    Task<Result<Role, Error>> AddAsync(
        Role role,
        CancellationToken cancellationToken);
}