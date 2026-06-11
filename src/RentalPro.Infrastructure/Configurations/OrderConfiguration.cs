using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalPro.Domain.Customers;
using RentalPro.Domain.Orders;
using RentalPro.Domain.Users;
using RentalPro.Domain.ValueObjects;

namespace RentalPro.Infrastructure.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => OrderId.Restore(value));
        
        builder
            .Property(x => x.Number)
            .HasColumnName("number")
            .HasConversion(
                number => number.Value,
                value => OrderNumber.Create(value).Value)
            .HasMaxLength(OrderNumber.MAX_LENGTH)
            .IsRequired();

        builder
            .Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasConversion(
                id => id.Value,
                value => UserId.Restore(value))
            .IsRequired();

        builder
            .Property(x => x.CustomerId)
            .HasColumnName("customer_id")
            .HasConversion(
                id => id.Value,
                value => CustomerId.Restore(value))
            .IsRequired();

        builder
            .Property(x => x.StatusId)
            .HasColumnName("status_id")
            .HasConversion(
                id => id.Value,
                value => OrderStatusId.Restore(value))
            .IsRequired();

        builder
            .Property(x => x.OrderDate)
            .HasColumnName("order_date")
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

        builder
            .Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        builder
            .Property(x => x.DeletedAt)
            .HasColumnName("deleted_at");

        builder
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Status)
            .WithMany()
            .HasForeignKey(x => x.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(x => x.Items)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.UserId);

        builder.HasIndex(x => x.CustomerId);

        builder.HasIndex(x => x.StatusId);

        builder.HasIndex(x => x.OrderDate);

        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}