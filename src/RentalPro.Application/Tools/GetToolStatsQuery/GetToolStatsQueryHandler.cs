using CSharpFunctionalExtensions;
using RentalPro.Application.Repositories;
using RentalPro.Contracts.Tools;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Tools.GetToolStatsQuery;

public sealed class GetToolStatsQueryHandler(IToolsReadRepository readRepository)
    : IQueryHandler<ToolStatsDto, GetToolStatsQuery>
{
    public async Task<Result<ToolStatsDto, Errors>> Handle(
        GetToolStatsQuery query,
        CancellationToken cancellationToken)
    {
        return await readRepository.GetStatsAsync(cancellationToken);
    }
}