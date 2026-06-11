using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using RentalPro.Shared;

namespace RentalPro.Domain.Orders;

public sealed class OrderNumber : ValueObject
{
    public const int MAX_LENGTH = 20;

    private static readonly Regex Regex =
        new(@"^ORD-\d{4}-\d{6}$", RegexOptions.Compiled);

    public string Value { get; }

    private OrderNumber(string value)
    {
        Value = value;
    }

    public static Result<OrderNumber, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return CommonErrors.IsRequired(nameof(value));

        value = value.Trim().ToUpperInvariant();

        if (value.Length > MAX_LENGTH)
        {
            return CommonErrors.LengthIsWrong(
                nameof(value),
                1,
                MAX_LENGTH);
        }

        if (!Regex.IsMatch(value))
        {
            return CommonErrors.Failure(
                nameof(value),
                "Неверный формат номера заказа. Ожидается: ORD-YYYY-XXXXXX");
        }

        return new OrderNumber(value);
    }

    public static OrderNumber Generate(
        OrderId orderId,
        DateTime orderDate)
    {
        var shortId = orderId.Value
            .ToString("N")[..6]
            .ToUpperInvariant();

        var value = $"ORD-{orderDate:yyyy}-{shortId}";

        return new OrderNumber(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value;
    }
}