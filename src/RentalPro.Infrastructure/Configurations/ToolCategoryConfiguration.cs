using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalPro.Domain.Tools;
using RentalPro.Domain.ValueObjects;

namespace RentalPro.Infrastructure.Configurations;

public sealed class ToolCategoryConfiguration : IEntityTypeConfiguration<ToolCategory>
{
    public void Configure(EntityTypeBuilder<ToolCategory> builder)
    {
        builder.ToTable("tool_categories");

        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => ToolCategoryId.Restore(value));

        builder
            .Property(x => x.Name)
            .HasColumnName("name")
            .HasConversion(
                name => name.Value,
                value => ToolCategoryName.Create(value).Value)
            .HasMaxLength(ToolCategoryName.MAX_LENGTH)
            .IsRequired();

        builder
            .Property(x => x.Description)
            .HasColumnName("description")
            .HasConversion(
                description => description == null ? null : description.Value,
                value => string.IsNullOrWhiteSpace(value)
                    ? null
                    : Description.Create(value).Value)
            .HasMaxLength(Description.MAX_LENGTH);

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