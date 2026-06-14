using Bogus;
using RentalPro.Presentation.Client.Models;

namespace RentalPro.Presentation.Client.Services;

public sealed class FakeToolGeneratorService
{
    private readonly Faker _faker = new("ru");

    private static readonly string[] Conditions =
    [
        "Исправен",
        "Исправен, следы эксплуатации",
        "Хорошее состояние",
        "Отличное состояние",
        "После технического обслуживания",
        "Незначительные следы эксплуатации"
    ];

    private static readonly string[] ToolNames =
    [
        "Перфоратор",
        "Дрель",
        "Шуруповерт",
        "Угловая шлифмашина",
        "Лобзик",
        "Циркулярная пила",
        "Отбойный молоток",
        "Компрессор",
        "Сварочный аппарат",
        "Строительный миксер"
    ];

    public ToolFormModel Generate()
    {
        var toolName = _faker.PickRandom(ToolNames);
        var modelNumber = _faker.Random.Number(100, 999);

        return new ToolFormModel
        {
            Name = $"{toolName} {modelNumber}",
            ArticleNumber = $"ART-{_faker.Random.Number(1000, 9999)}",
            SerialNumber = $"SN-{_faker.Random.Number(100000, 999999)}",
            InventoryNumber = $"INV-{_faker.Random.Number(1000, 9999)}",
            RentalPricePerDay = _faker.Random.Number(500, 5000),
            DepositAmount = _faker.Random.Number(1000, 20000),
            CurrentCondition = _faker.PickRandom(Conditions),
            Description = $"{toolName} в исправном состоянии. Подходит для строительных и ремонтных работ.",
            PhotoPath = null
        };
    }
}