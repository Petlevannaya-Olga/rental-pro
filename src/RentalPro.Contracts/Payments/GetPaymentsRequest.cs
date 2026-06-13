namespace RentalPro.Contracts.Payments;

public sealed class GetPaymentsRequest
{
    public string? Search { get; set; }

    public Guid? PaymentTypeId { get; set; }

    public Guid? PaymentMethodId { get; set; }

    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }

    public string? SortBy { get; set; }

    public bool Descending { get; set; } = true;

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}