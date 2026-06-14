using CSharpFunctionalExtensions;
using RentalPro.Application.Repositories;
using RentalPro.Contracts.Tools;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Tools.GetToolRentalHistoryQuery;

public sealed class GetToolRentalHistoryQueryHandler(
    IToolsReadRepository repository)
    : IQueryHandler<List<ToolRentalHistoryItemDto>,
        GetToolRentalHistoryQuery>
{
    public Task<Result<List<ToolRentalHistoryItemDto>, Errors>> Handle(
        GetToolRentalHistoryQuery query,
        CancellationToken cancellationToken)
    {
        return repository.GetRentalHistoryAsync(
            query.ToolId,
            cancellationToken);
    }
}