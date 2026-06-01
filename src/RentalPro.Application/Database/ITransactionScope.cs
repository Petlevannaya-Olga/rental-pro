using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Application.Database;

public interface ITransactionScope : IDisposable
{
    UnitResult<Error> Commit();

    UnitResult<Error> Rollback();
}