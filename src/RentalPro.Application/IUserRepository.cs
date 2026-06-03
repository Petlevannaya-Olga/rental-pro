using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using RentalPro.Domain.Roles;
using RentalPro.Domain.Users;
using RentalPro.Shared;

namespace RentalPro.Application;

public interface IUserRepository
{
    Task<Result<User?, Error>> GetByAsync(
        Expression<Func<User, bool>> expression,
        CancellationToken cancellationToken);

    Task<Result<User, Error>> AddAsync(
        User user,
        CancellationToken cancellationToken);
}