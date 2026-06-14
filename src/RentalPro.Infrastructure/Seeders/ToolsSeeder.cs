using Microsoft.EntityFrameworkCore;
using RentalPro.Domain.Manufacturers;
using RentalPro.Domain.Tools;

namespace RentalPro.Infrastructure.Seeders;

public static class ToolsSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Tools.AnyAsync())
            return;

        var categories = await context.ToolCategories.ToListAsync();
        var manufacturers = await context.Manufacturers.ToListAsync();
        var statuses = await context.ToolStatuses.ToListAsync();

        var status = GetStatusId(statuses, "Доступен");

        var tools = new[]
        {
            new ToolSeed("BOSCH-001", "Перфоратор Bosch GBH 2-26", "Профессиональный перфоратор для бурения бетона", "Перфораторы", "Bosch", status, 700, 5000, "SN-BOSCH-001", "INV-001", "Исправен, следы эксплуатации"),
            new ToolSeed("BOSCH-002", "Дрель Bosch GSB 13 RE", "Ударная дрель для бытовых и строительных работ", "Дрели", "Bosch", status, 450, 3000, "SN-BOSCH-002", "INV-002", "Исправна"),
            new ToolSeed("BOSCH-003", "Болгарка Bosch GWS 750", "Угловая шлифовальная машина", "Болгарки", "Bosch", status, 500, 3500, "SN-BOSCH-003", "INV-003", "Исправна"),

            new ToolSeed("MAKITA-001", "Шуруповерт Makita DF333", "Аккумуляторный шуруповерт", "Шуруповерты", "Makita", status, 450, 3500, "SN-MAKITA-001", "INV-004", "Исправен"),
            new ToolSeed("MAKITA-002", "Перфоратор Makita HR2470", "Перфоратор SDS-plus", "Перфораторы", "Makita", status, 750, 5500, "SN-MAKITA-002", "INV-005", "Исправен"),
            new ToolSeed("MAKITA-003", "Лобзик Makita 4329", "Электрический лобзик", "Лобзики", "Makita", status, 400, 2500, "SN-MAKITA-003", "INV-006", "Требуется проверка кабеля"),

            new ToolSeed("DEWALT-001", "Шуруповерт DeWalt DCD771", "Аккумуляторный шуруповерт", "Шуруповерты", "DeWalt", status, 500, 4000, "SN-DEWALT-001", "INV-007", "Исправен"),
            new ToolSeed("DEWALT-002", "Болгарка DeWalt DWE4157", "Угловая шлифовальная машина", "Болгарки", "DeWalt", status, 550, 4000, "SN-DEWALT-002", "INV-008", "Исправна"),
            new ToolSeed("DEWALT-003", "Пила DeWalt DWE560", "Дисковая пила", "Пилы", "DeWalt", status, 650, 4500, "SN-DEWALT-003", "INV-009", "Исправна"),

            new ToolSeed("METABO-001", "Шлифмашина Metabo SXE 3150", "Эксцентриковая шлифовальная машина", "Шлифовальные машины", "Metabo", status, 400, 3000, "SN-METABO-001", "INV-010", "Исправна"),
            new ToolSeed("METABO-002", "Дрель Metabo SBE 650", "Ударная дрель", "Дрели", "Metabo", status, 420, 3000, "SN-METABO-002", "INV-011", "Исправна"),

            new ToolSeed("HILTI-001", "Перфоратор Hilti TE 30", "Мощный профессиональный перфоратор", "Перфораторы", "Hilti", status, 1200, 9000, "SN-HILTI-001", "INV-012", "Исправен"),
            new ToolSeed("HILTI-002", "Сварочный аппарат Hilti X-BT", "Оборудование для монтажных работ", "Сварочное оборудование", "Hilti", status, 1500, 12000, "SN-HILTI-002", "INV-013", "На диагностике"),

            new ToolSeed("MILWAUKEE-001", "Шуруповерт Milwaukee M18", "Аккумуляторный шуруповерт", "Шуруповерты", "Milwaukee", status, 700, 6000, "SN-MIL-001", "INV-014", "Исправен"),
            new ToolSeed("MILWAUKEE-002", "Пила Milwaukee M18 CCS55", "Аккумуляторная циркулярная пила", "Пилы", "Milwaukee", status, 850, 7000, "SN-MIL-002", "INV-015", "Исправна"),

            new ToolSeed("RYOBI-001", "Садовый триммер Ryobi RLT", "Триммер для ухода за участком", "Садовая техника", "Ryobi", status, 500, 3500, "SN-RYOBI-001", "INV-016", "Исправен"),
            new ToolSeed("RYOBI-002", "Лобзик Ryobi RJS750", "Электролобзик", "Лобзики", "Ryobi", status, 350, 2500, "SN-RYOBI-002", "INV-017", "Исправен"),

            new ToolSeed("ZUBR-001", "Дрель Зубр ДУ-710", "Ударная дрель", "Дрели", "Зубр", status, 300, 2000, "SN-ZUBR-001", "INV-018", "Исправна"),
            new ToolSeed("ZUBR-002", "Болгарка Зубр УШМ-125", "Угловая шлифовальная машина", "Болгарки", "Зубр", status, 350, 2500, "SN-ZUBR-002", "INV-019", "Исправна"),

            new ToolSeed("PATRIOT-001", "Газонокосилка Patriot PT", "Садовая техника для ухода за газоном", "Садовая техника", "Patriot", status, 900, 7000, "SN-PATRIOT-001", "INV-020", "Исправна")
        };

        foreach (var toolSeed in tools)
        {
            var categoryId = GetCategoryId(categories, toolSeed.CategoryName);
            var manufacturerId = GetManufacturerId(manufacturers, toolSeed.ManufacturerName);

            var result = Tool.Create(
                articleNumber: toolSeed.ArticleNumber,
                name: toolSeed.Name,
                description: toolSeed.Description,
                categoryId: categoryId,
                manufacturerId: manufacturerId,
                statusId: toolSeed.StatusId,
                rentalPricePerDay: toolSeed.RentalPricePerDay,
                depositAmount: toolSeed.DepositAmount,
                serialNumber: toolSeed.SerialNumber,
                inventoryNumber: toolSeed.InventoryNumber,
                currentCondition: toolSeed.CurrentCondition,
                photoPath: null);

            if (result.IsFailure)
                throw new InvalidOperationException(result.Error.Message);

            await context.Tools.AddAsync(result.Value);
        }

        await context.SaveChangesAsync();
    }

    private static Guid GetCategoryId(
        List<ToolCategory> categories,
        string name)
    {
        return categories
            .First(x => x.Name.Value == name)
            .Id
            .Value;
    }

    private static Guid GetManufacturerId(
        List<Manufacturer> manufacturers,
        string name)
    {
        return manufacturers
            .First(x => x.Name.Value == name)
            .Id
            .Value;
    }

    private static Guid GetStatusId(
        List<ToolStatus> statuses,
        string name)
    {
        return statuses
            .First(x => x.Name.Value == name)
            .Id
            .Value;
    }

    private sealed record ToolSeed(
        string ArticleNumber,
        string Name,
        string? Description,
        string CategoryName,
        string ManufacturerName,
        Guid StatusId,
        decimal RentalPricePerDay,
        decimal DepositAmount,
        string SerialNumber,
        string InventoryNumber,
        string? CurrentCondition);
}