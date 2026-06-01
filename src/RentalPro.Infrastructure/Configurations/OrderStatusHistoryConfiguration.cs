using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalPro.Domain.Orders;
using RentalPro.Domain.Users;
using RentalPro.Domain.ValueObjects;

namespace RentalPro.Infrastructure.Configurations;

public sealed class OrderStatusHistoryConfiguration
    : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("order_status_history");

        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => OrderStatusHistoryId.Restore(value));

        builder
            .Property(x => x.OrderId)
            .HasColumnName("order_id")
            .HasConversion(
                id => id.Value,
                value => OrderId.Restore(value))
            .IsRequired();

        builder
            .Property(x => x.OldStatusId)
            .HasColumnName("old_status_id")
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue
                    ? OrderStatusId.Restore(value.Value)
                    : null);

        builder
            .Property(x => x.NewStatusId)
            .HasColumnName("new_status_id")
            .HasConversion(
                id => id.Value,
                value => OrderStatusId.Restore(value))
            .IsRequired();

        builder
            .Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasConversion(
                id => id.Value,
                value => UserId.Restore(value))
            .IsRequired();

        builder
            .Property(x => x.ChangeDate)
            .HasColumnName("change_date")
            .IsRequired();

        builder
            .Property(x => x.Comment)
            .HasColumnName("comment")
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

        builder.HasOne<Order>()
            .WithMany()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<OrderStatus>()
            .WithMany()
            .HasForeignKey(x => x.OldStatusId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<OrderStatus>()
            .WithMany()
            .HasForeignKey(x => x.NewStatusId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasIndex(x => x.OrderId);

        builder
            .HasIndex(x => x.NewStatusId);

        builder
            .HasIndex(x => x.UserId);

        builder
            .HasIndex(x => x.ChangeDate);
    }
}