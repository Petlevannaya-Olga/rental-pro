using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalPro.Domain.Suppliers;
using RentalPro.Domain.Tools;
using RentalPro.Domain.ValueObjects;

namespace RentalPro.Infrastructure.Configurations;

public sealed class ToolPurchaseConfiguration
    : IEntityTypeConfiguration<ToolPurchase>
{
    public void Configure(EntityTypeBuilder<ToolPurchase> builder)
    {
        builder.ToTable("tool_purchases");

        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => ToolPurchaseId.Restore(value));

        builder
            .Property(x => x.SupplierId)
            .HasColumnName("supplier_id")
            .HasConversion(
                id => id.Value,
                value => SupplierId.Restore(value))
            .IsRequired();

        builder
            .Property(x => x.ToolId)
            .HasColumnName("tool_id")
            .HasConversion(
                id => id.Value,
                value => ToolId.Restore(value))
            .IsRequired();

        builder
            .Property(x => x.PurchaseDate)
            .HasColumnName("purchase_date")
            .IsRequired();

        builder
            .Property(x => x.Price)
            .HasColumnName("price")
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
            .HasOne<Supplier>()
            .WithMany()
            .HasForeignKey(x => x.SupplierId);

        builder
            .HasOne<Tool>()
            .WithMany()
            .HasForeignKey(x => x.ToolId);

        builder
            .HasIndex(x => x.SupplierId);

        builder
            .HasIndex(x => x.ToolId);

        builder
            .HasIndex(x => x.PurchaseDate);

        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}