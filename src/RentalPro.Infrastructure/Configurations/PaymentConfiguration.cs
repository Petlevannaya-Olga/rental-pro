using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalPro.Domain.Orders;
using RentalPro.Domain.Payments;
using RentalPro.Domain.ValueObjects;

namespace RentalPro.Infrastructure.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");

        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => PaymentId.Restore(value));

        builder
            .Property(x => x.OrderId)
            .HasColumnName("order_id")
            .HasConversion(
                id => id.Value,
                value => OrderId.Restore(value))
            .IsRequired();

        builder
            .Property(x => x.PaymentMethodId)
            .HasColumnName("payment_method_id")
            .HasConversion(
                id => id.Value,
                value => PaymentMethodId.Restore(value))
            .IsRequired();

        builder
            .Property(x => x.PaymentTypeId)
            .HasColumnName("payment_type_id")
            .HasConversion(
                id => id.Value,
                value => PaymentTypeId.Restore(value))
            .IsRequired();

        builder
            .Property(x => x.PaymentDate)
            .HasColumnName("payment_date")
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
            .Property(x => x.FiscalReceiptId)
            .HasColumnName("fiscal_receipt_id")
            .HasMaxLength(100);

        builder
            .Property(x => x.FiscalStatus)
            .HasColumnName("fiscal_status")
            .HasMaxLength(50);

        builder
            .Property(x => x.FiscalizedAt)
            .HasColumnName("fiscalized_at");

        builder
            .Property(x => x.FiscalErrorMessage)
            .HasColumnName("fiscal_error_message")
            .HasMaxLength(500);

        builder
            .HasOne<Order>()
            .WithMany()
            .HasForeignKey(x => x.OrderId);

        builder
            .HasOne<PaymentMethod>()
            .WithMany()
            .HasForeignKey(x => x.PaymentMethodId);

        builder
            .HasOne<PaymentType>()
            .WithMany()
            .HasForeignKey(x => x.PaymentTypeId);

        builder
            .HasIndex(x => x.OrderId);

        builder
            .HasIndex(x => x.PaymentMethodId);

        builder
            .HasIndex(x => x.PaymentTypeId);

        builder
            .HasIndex(x => x.PaymentDate);

        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}