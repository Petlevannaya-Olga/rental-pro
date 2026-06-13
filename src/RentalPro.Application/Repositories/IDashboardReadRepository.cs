using CSharpFunctionalExtensions;
using RentalPro.Contracts.Dashboard;
using RentalPro.Shared;

namespace RentalPro.Application.Repositories;

public interface IDashboardReadRepository
{
    Task<Result<DashboardDto, Errors>> GetAsync(
        CancellationToken cancellationToken = default);
}