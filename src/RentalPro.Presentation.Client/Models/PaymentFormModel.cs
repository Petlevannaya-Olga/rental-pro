namespace RentalPro.Presentation.Client.Models;

public sealed class PaymentFormModel
{
    public Guid? PaymentTypeId { get; set; }
    public Guid? PaymentMethodId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.Now;
    public string? Comment { get; set; }
}