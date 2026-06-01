using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalPro.Domain.Orders;

namespace RentalPro.Infrastructure.Configurations;

public sealed class OrderStatusConfiguration : IEntityTypeConfiguration<OrderStatus>
{
    public void Configure(EntityTypeBuilder<OrderStatus> builder)
    {
        builder.ToTable("order_statuses");

        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => OrderStatusId.Restore(value));

        builder
            .Property(x => x.Name)
            .HasColumnName("name")
            .HasConversion(
                name => name.Value,
                value => OrderStatusName.Create(value).Value)
            .HasMaxLength(OrderStatusName.MAX_LENGTH)
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