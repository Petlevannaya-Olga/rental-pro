using CSharpFunctionalExtensions;
using RentalPro.Application.Repositories;
using RentalPro.Contracts.ToolStatuses;
using RentalPro.Shared;
using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.ToolStatuses.GetToolStatusesQuery;

public sealed class GetToolStatusesQueryHandler(
    IToolStatusesRepository repository)
    : IQueryHandler<IReadOnlyList<ToolStatusDto>, GetToolStatusesQuery>
{
    public async Task<Result<IReadOnlyList<ToolStatusDto>, Errors>> Handle(
        GetToolStatusesQuery query,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetAllAsync(
            cancellationToken);

        if (result.IsFailure)
            return result.Error.ToErrors();

        return result.Value
            .Select(x => new ToolStatusDto(
                x.Id.Value,
                x.Name.Value,
                x.CreatedAt,
                x.UpdatedAt))
            .ToList();
    }
}