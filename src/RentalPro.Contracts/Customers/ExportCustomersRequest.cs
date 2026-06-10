namespace RentalPro.Contracts.Customers;

public sealed class ExportCustomersRequest
{
    public string? Search { get; set; }

    public bool? HasOrders { get; set; }

    public bool? HasDebt { get; set; }

    public string? SortBy { get; set; }

    public bool Descending { get; set; }
}