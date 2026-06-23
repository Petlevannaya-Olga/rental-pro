using Bogus;
using RentalPro.Contracts.Customers;
using RentalPro.Contracts.Tools;
using RentalPro.Presentation.Desktop.Models;

namespace RentalPro.Presentation.Desktop.Services;

public sealed class FakeOrderGeneratorService
{
    private readonly Faker _faker = new("ru");

    private static readonly string[] Comments =
    [
        "Срочный заказ.",
        "Постоянный клиент.",
        "Проверить комплектность перед выдачей.",
        "Клиент заберет самостоятельно.",
        "Подготовить документы заранее.",
        "Провести дополнительную проверку инструмента."
    ];

    public void Fill(
        OrderEditModel order,
        IReadOnlyList<CustomerDto> customers,
        IReadOnlyList<ToolDto> tools)
    {
        order.CustomerId = null;
        order.CustomerName = string.Empty;
        order.Tools.Clear();

        order.OrderDate = DateTime.Today;
        order.Comment = _faker.PickRandom(Comments);

        FillCustomer(order, customers);
        FillTools(order, tools);
    }

    private void FillCustomer(
        OrderEditModel order,
        IReadOnlyList<CustomerDto> customers)
    {
        if (customers.Count == 0)
            return;

        var index = _faker.Random.Int(0, customers.Count - 1);
        var customer = customers[index];

        order.CustomerId = customer.Id;
        order.CustomerName = customer.FullName;
    }

    private void FillTools(
        OrderEditModel order,
        IReadOnlyList<ToolDto> tools)
    {
        if (tools.Count == 0)
            return;

        var count = _faker.Random.Int(
            1,
            Math.Min(10, tools.Count));

        var selectedTools = tools
            .OrderBy(_ => _faker.Random.Int())
            .Take(count);

        foreach (var tool in selectedTools)
        {
            var startDate = DateTime.Today.AddDays(
                _faker.Random.Int(0, 7));

            var rentalDays = _faker.Random.Int(1, 14);

            order.Tools.Add(new OrderToolEditModel
            {
                ToolId = tool.Id,
                ToolName = tool.Name,
                RentalPricePerDay = tool.RentalPricePerDay,
                DepositAmount = tool.DepositAmount,
                StartDate = startDate,
                EndDate = startDate.AddDays(rentalDays)
            });
        }
    }
}