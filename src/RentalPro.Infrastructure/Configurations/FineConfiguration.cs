using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalPro.Domain.Fines;
using RentalPro.Domain.Orders;
using RentalPro.Domain.ValueObjects;

namespace RentalPro.Infrastructure.Configurations;

public sealed class FineConfiguration : IEntityTypeConfiguration<Fine>
{
    public void Configure(EntityTypeBuilder<Fine> builder)
    {
        builder.ToTable("fines");

        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => FineId.Restore(value));

        builder
            .Property(x => x.OrderItemId)
            .HasColumnName("order_item_id")
            .HasConversion(
                id => id.Value,
                value => OrderItemId.Restore(value))
            .IsRequired();

        builder
            .Property(x => x.FineDate)
            .HasColumnName("fine_date")
            .IsRequired();

        builder
            .Property(x => x.Amount)
            .HasColumnName("amount")
            .HasConversion(
                money => money.Value,
                value => Money.Create(value).Value)
            .HasPrecision(18, 2)
            .IsRequired();

        builder
            .Property(x => x.Reason)
            .HasColumnName("reason")
            .HasConversion(
                reason => reason.Value,
                value => FineReason.Create(value).Value)
            .HasMaxLength(FineReason.MAX_LENGTH)
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
            .HasOne<OrderItem>()
            .WithMany()
            .HasForeignKey(x => x.OrderItemId);

        builder
            .HasIndex(x => x.OrderItemId);

        builder
            .HasIndex(x => x.FineDate);

        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}