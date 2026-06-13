namespace RentalPro.Contracts.Payments;

public sealed class ExportPaymentsRequest
{
    public string? Search { get; set; }

    public Guid? PaymentTypeId { get; set; }

    public Guid? PaymentMethodId { get; set; }

    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }

    public string? SortBy { get; set; }

    public bool Descending { get; set; }
}