namespace RentalPro.Contracts.Tools;

public sealed class ExportToolsRequest
{
    public string? Search { get; set; }

    public Guid? CategoryId { get; set; }

    public Guid? ManufacturerId { get; set; }

    public Guid? StatusId { get; set; }

    public string? SortBy { get; set; }

    public bool Descending { get; set; }
}