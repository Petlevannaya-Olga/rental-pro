using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalPro.Domain.Payments;

namespace RentalPro.Infrastructure.Configurations;

public sealed class PaymentMethodConfiguration
    : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.ToTable("payment_methods");

        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => PaymentMethodId.Restore(value));

        builder
            .Property(x => x.Name)
            .HasColumnName("name")
            .HasConversion(
                name => name.Value,
                value => PaymentMethodName.Create(value).Value)
            .HasMaxLength(PaymentMethodName.MAX_LENGTH)
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
            .HasIndex(x => x.Name)
            .IsUnique()
            .HasFilter("[deleted_at] IS NULL");;

        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}