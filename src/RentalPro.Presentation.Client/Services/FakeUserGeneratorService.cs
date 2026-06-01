using Bogus;
using RentalPro.Presentation.Client.Models.Users;

namespace RentalPro.Presentation.Client.Services;

public sealed class FakeUserGeneratorService
{
    private readonly Faker _faker = new("ru");

    private static readonly string[] MaleMiddleNames =
    [
        "Александрович",
        "Алексеевич",
        "Андреевич",
        "Антонович",
        "Артёмович",
        "Борисович",
        "Валерьевич",
        "Васильевич",
        "Викторович",
        "Владимирович",
        "Вячеславович",
        "Геннадьевич",
        "Григорьевич",
        "Денисович",
        "Дмитриевич",
        "Евгеньевич",
        "Иванович",
        "Игоревич",
        "Константинович",
        "Максимович",
        "Михайлович",
        "Николаевич",
        "Олегович",
        "Павлович",
        "Петрович",
        "Романович",
        "Сергеевич",
        "Станиславович",
        "Фёдорович",
        "Юрьевич"
    ];

    private static readonly string[] FemaleMiddleNames =
    [
        "Александровна",
        "Алексеевна",
        "Андреевна",
        "Антоновна",
        "Артёмовна",
        "Борисовна",
        "Валерьевна",
        "Васильевна",
        "Викторовна",
        "Владимировна",
        "Вячеславовна",
        "Геннадьевна",
        "Григорьевна",
        "Денисовна",
        "Дмитриевна",
        "Евгеньевна",
        "Ивановна",
        "Игоревна",
        "Константиновна",
        "Максимовна",
        "Михайловна",
        "Николаевна",
        "Олеговна",
        "Павловна",
        "Петровна",
        "Романовна",
        "Сергеевна",
        "Станиславовна",
        "Фёдоровна",
        "Юрьевна"
    ];

    public CreateUserFormModel Generate()
    {
        var gender = _faker.PickRandom<Bogus.DataSets.Name.Gender>();
        var firstName = _faker.Name.FirstName(gender);
        var lastName = _faker.Name.LastName(gender);
        
        return new CreateUserFormModel
        {
            LastName = lastName,
            FirstName = firstName,
            MiddleName = gender == Bogus.DataSets.Name.Gender.Male
                ? _faker.PickRandom(MaleMiddleNames)
                : _faker.PickRandom(FemaleMiddleNames),

            Login = _faker.Internet.UserName(firstName, lastName).ToLowerInvariant(),
            Email = _faker.Internet.Email(firstName, lastName, "rentalpro.ru").ToLowerInvariant(),
            PhoneNumber = $"+79{_faker.Random.Number(100000000, 999999999)}",
            Password = "Password123!"
        };
    }
}