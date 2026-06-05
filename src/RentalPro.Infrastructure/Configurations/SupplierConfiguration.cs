using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalPro.Domain.Suppliers;
using RentalPro.Domain.ValueObjects;

namespace RentalPro.Infrastructure.Configurations;

public sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("suppliers");

        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => SupplierId.Restore(value));

        builder
            .Property(x => x.Name)
            .HasColumnName("name")
            .HasConversion(
                name => name.Value,
                value => SupplierName.Create(value).Value)
            .HasMaxLength(SupplierName.MAX_LENGTH)
            .IsRequired();

        builder
            .Property(x => x.PhoneNumber)
            .HasColumnName("phone_number")
            .HasConversion(
                phoneNumber => phoneNumber == null ? null : phoneNumber.Value,
                value => string.IsNullOrWhiteSpace(value)
                    ? null
                    : PhoneNumber.Create(value).Value)
            .HasMaxLength(PhoneNumber.MAX_LENGTH);

        builder
            .Property(x => x.Email)
            .HasColumnName("email")
            .HasConversion(
                email => email == null ? null : email.Value,
                value => string.IsNullOrWhiteSpace(value)
                    ? null
                    : Email.Create(value).Value)
            .HasMaxLength(Email.MAX_LENGTH);

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

        builder.OwnsOne(
            x => x.ContactPerson,
            contactPerson =>
            {
                contactPerson
                    .Property(x => x.LastName)
                    .HasColumnName("contact_last_name")
                    .HasMaxLength(FullName.MAX_LENGTH);

                contactPerson
                    .Property(x => x.FirstName)
                    .HasColumnName("contact_first_name")
                    .HasMaxLength(FullName.MAX_LENGTH);

                contactPerson
                    .Property(x => x.MiddleName)
                    .HasColumnName("contact_middle_name")
                    .HasMaxLength(FullName.MAX_LENGTH);
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
            .HasIndex(x => x.Name)
            .IsUnique()
            .HasFilter("[deleted_at] IS NULL");;

        builder
            .HasIndex(x => x.PhoneNumber);

        builder
            .HasIndex(x => x.Email);

        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}