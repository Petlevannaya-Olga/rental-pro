using CSharpFunctionalExtensions;
using RentalPro.Application.Repositories;
using RentalPro.Contracts.Dashboard;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Dashboard.GetDashboardQuery;

public sealed class GetDashboardQueryHandler(
    IDashboardReadRepository repository)
    : IQueryHandler<DashboardDto, GetDashboardQuery>
{
    public async Task<Result<DashboardDto, Errors>> Handle(
        GetDashboardQuery query,
        CancellationToken cancellationToken)
    {
        return await repository.GetAsync(cancellationToken);
    }
}