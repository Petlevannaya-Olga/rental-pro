namespace RentalPro.Contracts.Users;

public sealed class GetUsersRequest
{
    public string? Search { get; init; }

    public Guid? RoleId { get; init; }

    public bool? IsActive { get; init; }

    public DateTime? CreatedFrom { get; init; }

    public DateTime? CreatedTo { get; init; }

    public string? SortBy { get; init; }

    public bool Descending { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;
}