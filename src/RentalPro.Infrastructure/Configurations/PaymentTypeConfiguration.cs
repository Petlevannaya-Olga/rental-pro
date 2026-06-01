using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalPro.Domain.Payments;

namespace RentalPro.Infrastructure.Configurations;

public sealed class PaymentTypeConfiguration
    : IEntityTypeConfiguration<PaymentType>
{
    public void Configure(EntityTypeBuilder<PaymentType> builder)
    {
        builder.ToTable("payment_types");

        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => PaymentTypeId.Restore(value));

        builder
            .Property(x => x.Name)
            .HasColumnName("name")
            .HasConversion(
                name => name.Value,
                value => PaymentTypeName.Create(value).Value)
            .HasMaxLength(PaymentTypeName.MAX_LENGTH)
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
            .IsUnique();

        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}