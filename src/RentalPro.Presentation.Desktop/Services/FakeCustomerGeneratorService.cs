using Bogus;
using RentalPro.Presentation.Desktop.ViewModels;
using CustomerEditModel = RentalPro.Presentation.Desktop.Models.CustomerEditModel;

namespace RentalPro.Presentation.Desktop.Services;

public sealed class FakeCustomerGeneratorService
{
    private readonly Faker _faker = new("ru");

    private static readonly string[] MaleMiddleNames =
    [
        "Александрович", "Алексеевич", "Андреевич", "Антонович",
        "Артемович", "Борисович", "Валерьевич", "Васильевич",
        "Викторович", "Владимирович", "Дмитриевич", "Иванович",
        "Петрович", "Сергеевич", "Юрьевич"
    ];

    private static readonly string[] FemaleMiddleNames =
    [
        "Александровна", "Алексеевна", "Андреевна", "Антоновна",
        "Артемовна", "Борисовна", "Валерьевна", "Васильевна",
        "Викторовна", "Владимировна", "Дмитриевна", "Ивановна",
        "Петровна", "Сергеевна", "Юрьевна"
    ];

    private static readonly string[] Regions =
    [
        "Московская область",
        "Ленинградская область",
        "Краснодарский край",
        "Республика Татарстан",
        "Новосибирская область",
        "Самарская область",
        "Свердловская область",
        "Ростовская область"
    ];

    private static readonly string[] Cities =
    [
        "Москва",
        "Санкт-Петербург",
        "Краснодар",
        "Казань",
        "Новосибирск",
        "Самара",
        "Екатеринбург",
        "Ростов-на-Дону"
    ];

    private static readonly string[] Streets =
    [
        "Ленина",
        "Пушкина",
        "Гагарина",
        "Советская",
        "Молодежная",
        "Центральная",
        "Лесная",
        "Северная",
        "Невский проспект",
        "Баумана"
    ];

    public CustomerEditModel Generate()
    {
        var gender = _faker.PickRandom<Bogus.DataSets.Name.Gender>();

        var firstName = _faker.Name.FirstName(gender);
        var lastName = _faker.Name.LastName(gender);

        return new CustomerEditModel
        {
            LastName = lastName,
            FirstName = firstName,

            MiddleName = gender == Bogus.DataSets.Name.Gender.Male
                ? _faker.PickRandom(MaleMiddleNames)
                : _faker.PickRandom(FemaleMiddleNames),

            PhoneNumber = $"+79{_faker.Random.Number(100000000, 999999999)}",
            Email = _faker.Internet.Email(firstName, lastName).ToLowerInvariant(),

            PassportSeries = _faker.Random.Number(1000, 9999).ToString(),
            PassportNumber = _faker.Random.Number(100000, 999999).ToString(),

            PostalCode = _faker.Random.Number(100000, 999999).ToString(),
            Region = _faker.PickRandom(Regions),
            City = _faker.PickRandom(Cities),
            Street = _faker.PickRandom(Streets),
            House = _faker.Random.Number(1, 250).ToString(),

            Building = _faker.Random.Bool(0.35f)
                ? _faker.Random.Number(1, 10).ToString()
                : string.Empty,

            Apartment = _faker.Random.Bool(0.9f)
                ? _faker.Random.Number(1, 300).ToString()
                : string.Empty
        };
    }
}