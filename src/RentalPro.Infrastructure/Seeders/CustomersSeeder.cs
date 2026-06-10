using Microsoft.EntityFrameworkCore;
using RentalPro.Domain.Customers;
using RentalPro.Domain.ValueObjects;

namespace RentalPro.Infrastructure.Seeders;

public static class CustomersSeeder
{
    public static async Task SeedAsync(ApplicationDbContext dbContext)
    {
        if (await dbContext.Customers.AnyAsync())
            return;

        var customers = new List<Customer>();

        AddCustomer(
            customers,
            "Иванов",
            "Иван",
            "Иванович",
            "+79990000001",
            "ivanov@rentalpro.ru",
            "1234",
            "567890",
            "101000",
            "Московская область",
            "Москва",
            "Ленина",
            "1",
            null,
            "10");

        AddCustomer(
            customers,
            "Петров",
            "Пётр",
            "Петрович",
            "+79990000002",
            "petrov@rentalpro.ru",
            "1234",
            "567891",
            "101001",
            "Московская область",
            "Москва",
            "Пушкина",
            "2",
            null,
            "11");

        AddCustomer(
            customers,
            "Сидоров",
            "Алексей",
            "Игоревич",
            "+79990000003",
            "sidorov@rentalpro.ru",
            "1234",
            "567892",
            "101002",
            "Московская область",
            "Москва",
            "Гагарина",
            "3",
            null,
            "12");

        AddCustomer(
            customers,
            "Кузнецов",
            "Дмитрий",
            "Сергеевич",
            "+79990000004",
            "kuznetsov@rentalpro.ru",
            "1234",
            "567893",
            "101003",
            "Санкт-Петербург",
            "Санкт-Петербург",
            "Невский проспект",
            "15",
            "2",
            "45");

        AddCustomer(
            customers,
            "Смирнова",
            "Елена",
            "Викторовна",
            "+79990000005",
            "smirnova@rentalpro.ru",
            "1234",
            "567894",
            "101004",
            "Краснодарский край",
            "Краснодар",
            "Северная",
            "20",
            null,
            "8");

        AddCustomer(
            customers,
            "Волков",
            "Артём",
            "Олегович",
            "+79990000006",
            "volkov@rentalpro.ru",
            "1234",
            "567895",
            "101005",
            "Республика Татарстан",
            "Казань",
            "Баумана",
            "7",
            null,
            "14");

        AddCustomer(
            customers,
            "Фёдоров",
            "Николай",
            "Викторович",
            "+79990000007",
            "fedorov@rentalpro.ru",
            "1234",
            "567896",
            "101006",
            "Новосибирская область",
            "Новосибирск",
            "Советская",
            "25",
            null,
            "22");

        AddCustomer(
            customers,
            "Морозов",
            "Егор",
            "Владимирович",
            "+79990000008",
            "morozov@rentalpro.ru",
            "1234",
            "567897",
            "101007",
            "Самарская область",
            "Самара",
            "Молодёжная",
            "17",
            null,
            "30");

        AddCustomer(
            customers,
            "Новикова",
            "Мария",
            "Алексеевна",
            "+79990000009",
            "novikova@rentalpro.ru",
            "1234",
            "567898",
            "101008",
            "Ростовская область",
            "Ростов-на-Дону",
            "Центральная",
            "9",
            null,
            "15");

        AddCustomer(
            customers,
            "Орлов",
            "Павел",
            "Николаевич",
            "+79990000010",
            "orlov@rentalpro.ru",
            "1234",
            "567899",
            "101009",
            "Свердловская область",
            "Екатеринбург",
            "Лесная",
            "5",
            null,
            "2");

        await dbContext.Customers.AddRangeAsync(customers);

        await dbContext.SaveChangesAsync();
    }

    private static void AddCustomer(
        ICollection<Customer> customers,
        string lastName,
        string firstName,
        string middleName,
        string phoneNumber,
        string email,
        string passportSeries,
        string passportNumber,
        string postalCode,
        string region,
        string city,
        string street,
        string house,
        string? building,
        string? apartment)
    {
        var addressResult = Address.Create(
            postalCode,
            region,
            city,
            street,
            house,
            building,
            apartment);

        if (addressResult.IsFailure)
            throw new InvalidOperationException(addressResult.Error.Message);

        var customerResult = Customer.Create(
            lastName,
            firstName,
            middleName,
            phoneNumber,
            email,
            passportSeries,
            passportNumber,
            addressResult.Value);

        if (customerResult.IsFailure)
            throw new InvalidOperationException(customerResult.Error.Message);

        customers.Add(customerResult.Value);
    }
}