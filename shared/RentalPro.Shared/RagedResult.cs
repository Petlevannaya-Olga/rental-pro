namespace RentalPro.Shared;

public sealed class PagedResult<T>(
    IReadOnlyList<T> items,
    int page,
    int pageSize,
    int totalCount)
{
    public IReadOnlyList<T> Items { get; } = items;

    public int Page { get; } = page;

    public int PageSize { get; } = pageSize;

    public int TotalCount { get; } = totalCount;

    public int TotalPages =>
        PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
}