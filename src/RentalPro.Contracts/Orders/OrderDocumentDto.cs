namespace RentalPro.Contracts.Orders;

public sealed record OrderDocumentDto(
    OrderDocumentType Type,
    DateOnly? Date,
    string Title);

public enum OrderDocumentType
{
    Contract = 1,
    TransferAct = 2,
    ReturnAct = 3
}