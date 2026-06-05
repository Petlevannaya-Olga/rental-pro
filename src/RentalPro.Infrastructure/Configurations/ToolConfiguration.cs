using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalPro.Domain.Manufacturers;
using RentalPro.Domain.Tools;
using RentalPro.Domain.ValueObjects;

namespace RentalPro.Infrastructure.Configurations;

public sealed class ToolConfiguration : IEntityTypeConfiguration<Tool>
{
    public void Configure(EntityTypeBuilder<Tool> builder)
    {
        builder.ToTable("tools");

        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => ToolId.Restore(value));

        builder
            .Property(x => x.ArticleNumber)
            .HasColumnName("article_number")
            .HasConversion(
                value => value.Value,
                value => ArticleNumber.Create(value).Value)
            .HasMaxLength(ArticleNumber.MAX_LENGTH)
            .IsRequired();

        builder
            .Property(x => x.Name)
            .HasColumnName("name")
            .HasConversion(
                value => value.Value,
                value => ToolName.Create(value).Value)
            .HasMaxLength(ToolName.MAX_LENGTH)
            .IsRequired();

        builder
            .Property(x => x.Description)
            .HasColumnName("description")
            .HasConversion(
                value => value == null ? null : value.Value,
                value => string.IsNullOrWhiteSpace(value)
                    ? null
                    : Description.Create(value).Value)
            .HasMaxLength(Description.MAX_LENGTH);

        builder
            .Property(x => x.CategoryId)
            .HasColumnName("category_id")
            .HasConversion(
                id => id.Value,
                value => ToolCategoryId.Restore(value))
            .IsRequired();

        builder
            .Property(x => x.ManufacturerId)
            .HasColumnName("manufacturer_id")
            .HasConversion(
                id => id.Value,
                value => ManufacturerId.Restore(value))
            .IsRequired();

        builder
            .Property(x => x.StatusId)
            .HasColumnName("status_id")
            .HasConversion(
                id => id.Value,
                value => ToolStatusId.Restore(value))
            .IsRequired();

        builder
            .Property(x => x.RentalPricePerDay)
            .HasColumnName("rental_price_per_day")
            .HasConversion(
                money => money.Value,
                value => Money.Create(value).Value)
            .HasPrecision(18, 2)
            .IsRequired();

        builder
            .Property(x => x.DepositAmount)
            .HasColumnName("deposit_amount")
            .HasConversion(
                money => money.Value,
                value => Money.Create(value).Value)
            .HasPrecision(18, 2)
            .IsRequired();

        builder
            .Property(x => x.SerialNumber)
            .HasColumnName("serial_number")
            .HasConversion(
                value => value.Value,
                value => SerialNumber.Create(value).Value)
            .HasMaxLength(SerialNumber.MAX_LENGTH)
            .IsRequired();

        builder
            .Property(x => x.InventoryNumber)
            .HasColumnName("inventory_number")
            .HasConversion(
                value => value.Value,
                value => InventoryNumber.Create(value).Value)
            .HasMaxLength(InventoryNumber.MAX_LENGTH)
            .IsRequired();

        builder
            .Property(x => x.CurrentCondition)
            .HasColumnName("current_condition")
            .HasConversion(
                value => value == null ? null : value.Value,
                value => string.IsNullOrWhiteSpace(value)
                    ? null
                    : ReturnCondition.Create(value).Value)
            .HasMaxLength(ReturnCondition.MAX_LENGTH);

        builder
            .Property(x => x.PhotoPath)
            .HasColumnName("photo_path")
            .HasConversion(
                value => value == null ? null : value.Value,
                value => string.IsNullOrWhiteSpace(value)
                    ? null
                    : PhotoPath.Create(value).Value)
            .HasMaxLength(PhotoPath.MAX_LENGTH);

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
            .HasOne<ToolCategory>()
            .WithMany()
            .HasForeignKey(x => x.CategoryId);

        builder
            .HasOne<Manufacturer>()
            .WithMany()
            .HasForeignKey(x => x.ManufacturerId);

        builder
            .HasOne<ToolStatus>()
            .WithMany()
            .HasForeignKey(x => x.StatusId);

        builder
            .HasIndex(x => x.ArticleNumber)
            .IsUnique()
            .HasFilter("[deleted_at] IS NULL");;

        builder
            .HasIndex(x => x.SerialNumber)
            .IsUnique()
            .HasFilter("[deleted_at] IS NULL");;

        builder
            .HasIndex(x => x.InventoryNumber)
            .IsUnique()
            .HasFilter("[deleted_at] IS NULL");;

        builder
            .HasIndex(x => x.CategoryId);

        builder
            .HasIndex(x => x.ManufacturerId);

        builder
            .HasIndex(x => x.StatusId);

        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}