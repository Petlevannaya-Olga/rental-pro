using CSharpFunctionalExtensions;

namespace RentalPro.Shared.Abstractions;

public interface IQueryHandler<TResponse, in TQuery>
    where TQuery : IQuery
{
    Task<Result<TResponse, Errors>> Handle(TQuery query, CancellationToken cancellationToken);
}

public interface IQuery;