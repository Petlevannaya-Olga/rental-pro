using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalPro.Domain.Roles;
using RentalPro.Domain.Users;
using RentalPro.Domain.ValueObjects;

namespace RentalPro.Infrastructure.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => UserId.Restore(value));

        builder
            .Property(x => x.Login)
            .HasColumnName("login")
            .HasConversion(
                login => login.Value,
                value => Login.Create(value).Value)
            .HasMaxLength(Login.MAX_LENGTH)
            .IsRequired();

        builder
            .Property(x => x.PasswordHash)
            .HasColumnName("password_hash")
            .HasConversion(
                passwordHash => passwordHash.Value,
                value => PasswordHash.Create(value).Value)
            .HasMaxLength(PasswordHash.MAX_LENGTH)
            .IsRequired();

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

        builder
            .Property(x => x.RoleId)
            .HasColumnName("role_id")
            .HasConversion(
                roleId => roleId.Value,
                value => RoleId.Restore(value))
            .IsRequired();

        builder
            .Property(x => x.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

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
            .HasOne(x => x.Role)
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .IsRequired();

        builder
            .HasIndex(x => x.Login)
            .IsUnique()
            .HasFilter("[deleted_at] IS NULL");;

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