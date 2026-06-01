using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalPro.Domain.Orders;
using RentalPro.Domain.Tools;
using RentalPro.Domain.ValueObjects;

namespace RentalPro.Infrastructure.Configurations;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");

        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => OrderItemId.Restore(value));

        builder
            .Property(x => x.OrderId)
            .HasColumnName("order_id")
            .HasConversion(
                id => id.Value,
                value => OrderId.Restore(value))
            .IsRequired();

        builder
            .Property(x => x.ToolId)
            .HasColumnName("tool_id")
            .HasConversion(
                id => id.Value,
                value => ToolId.Restore(value))
            .IsRequired();

        builder
            .Property(x => x.RentalPricePerDay)
            .HasColumnName("rental_price_per_day")
            .HasConversion(
                money => money.Value,
                value => Money.Create(value).Value)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.OwnsOne(
            x => x.RentalPeriod,
            rentalPeriod =>
            {
                rentalPeriod
                    .Property(x => x.StartDate)
                    .HasColumnName("start_date")
                    .IsRequired();

                rentalPeriod
                    .Property(x => x.PlannedReturnDate)
                    .HasColumnName("planned_return_date")
                    .IsRequired();
            });

        builder
            .Property(x => x.ActualReturnedDate)
            .HasColumnName("actual_returned_date");

        builder
            .Property(x => x.ItemTotalCost)
            .HasColumnName("item_total_cost")
            .HasConversion(
                money => money.Value,
                value => Money.Create(value).Value)
            .HasPrecision(18, 2)
            .IsRequired();

        builder
            .Property(x => x.ReturnCondition)
            .HasColumnName("return_condition")
            .HasConversion(
                condition => condition == null ? null : condition.Value,
                value => string.IsNullOrWhiteSpace(value)
                    ? null
                    : ReturnCondition.Create(value).Value)
            .HasMaxLength(ReturnCondition.MAX_LENGTH);

        builder
            .Property(x => x.DamageComment)
            .HasColumnName("damage_comment")
            .HasConversion(
                comment => comment == null ? null : comment.Value,
                value => string.IsNullOrWhiteSpace(value)
                    ? null
                    : Comment.Create(value).Value)
            .HasMaxLength(Comment.MAX_LENGTH);

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
            .HasOne<Order>()
            .WithMany()
            .HasForeignKey(x => x.OrderId);

        builder
            .HasOne<Tool>()
            .WithMany()
            .HasForeignKey(x => x.ToolId);

        builder
            .HasIndex(x => x.OrderId);

        builder
            .HasIndex(x => x.ToolId);

        builder
            .HasIndex(x => x.ActualReturnedDate);

        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}