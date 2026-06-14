using RentalPro.Shared.Abstractions;

namespace RentalPro.Application.Tools.GetToolRentalHistoryQuery;

public sealed record GetToolRentalHistoryQuery(Guid ToolId) : IQuery;