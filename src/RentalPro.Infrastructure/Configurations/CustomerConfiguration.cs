using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalPro.Domain.Customers;
using RentalPro.Domain.ValueObjects;

namespace RentalPro.Infrastructure.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => CustomerId.Restore(value));

        builder.OwnsOne(
            x => x.FullName,
            fullName =>
            {
                fullName
                    .Property(x => x.LastName)
                    .HasColumnName("last_name")
                    .HasMaxLength(FullName.MAX_LENGTH)
                    .IsRequired();

                fullName
                    .Property(x => x.FirstName)
                    .HasColumnName("first_name")
                    .HasMaxLength(FullName.MAX_LENGTH)
                    .IsRequired();

                fullName
                    .Property(x => x.MiddleName)
                    .HasColumnName("middle_name")
                    .HasMaxLength(FullName.MAX_LENGTH)
                    .IsRequired();
            });

        builder
            .Property(x => x.PhoneNumber)
            .HasColumnName("phone_number")
            .HasConversion(
                phoneNumber => phoneNumber.Value,
                value => PhoneNumber.Create(value).Value)
            .HasMaxLength(PhoneNumber.MAX_LENGTH)
            .IsRequired();

        builder
            .Property(x => x.Email)
            .HasColumnName("email")
            .HasConversion(
                email => email.Value,
                value => Email.Create(value).Value)
            .HasMaxLength(Email.MAX_LENGTH)
            .IsRequired();

        builder.OwnsOne(
            x => x.PassportData,
            passport =>
            {
                passport
                    .Property(x => x.Series)
                    .HasColumnName("passport_series")
                    .HasMaxLength(PassportData.SERIES_LENGTH)
                    .IsRequired();

                passport
                    .Property(x => x.Number)
                    .HasColumnName("passport_number")
                    .HasMaxLength(PassportData.NUMBER_LENGTH)
                    .IsRequired();

                passport
                    .HasIndex(x => new { x.Series, x.Number })
                    .IsUnique()
                    .HasFilter("[deleted_at] IS NULL");;
            });

        builder.OwnsOne(
            x => x.Address,
            address =>
            {
                address
                    .Property(x => x.PostalCode)
                    .HasColumnName("postal_code")
                    .HasMaxLength(Address.POSTAL_CODE_LENGTH);

                address
                    .Property(x => x.Region)
                    .HasColumnName("region")
                    .HasMaxLength(Address.MAX_REGION_LENGTH);

                address
                    .Property(x => x.City)
                    .HasColumnName("city")
                    .HasMaxLength(Address.MAX_CITY_LENGTH);

                address
                    .Property(x => x.Street)
                    .HasColumnName("street")
                    .HasMaxLength(Address.MAX_STREET_LENGTH);

                address
                    .Property(x => x.House)
                    .HasColumnName("house")
                    .HasMaxLength(Address.MAX_HOUSE_LENGTH);

                address
                    .Property(x => x.Building)
                    .HasColumnName("building")
                    .HasMaxLength(Address.MAX_BUILDING_LENGTH);

                address
                    .Property(x => x.Apartment)
                    .HasColumnName("apartment")
                    .HasMaxLength(Address.MAX_APARTMENT_LENGTH);
            });

        builder
            .Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder
            .Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        builder
            .Property(x => x.DeletedAt)
            .HasColumnName("deleted_at");

        builder
            .HasIndex(x => x.PhoneNumber)
            .IsUnique()
            .HasFilter("[deleted_at] IS NULL");;

        builder
            .HasIndex(x => x.Email)
            .IsUnique()
            .HasFilter("[deleted_at] IS NULL");;

        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}