using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalPro.Domain.Tools;

namespace RentalPro.Infrastructure.Configurations;

public sealed class ToolStatusConfiguration : IEntityTypeConfiguration<ToolStatus>
{
    public void Configure(EntityTypeBuilder<ToolStatus> builder)
    {
        builder.ToTable("tool_statuses");

        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => ToolStatusId.Restore(value));

        builder
            .Property(x => x.Name)
            .HasColumnName("name")
            .HasConversion(
                name => name.Value,
                value => ToolStatusName.Create(value).Value)
            .HasMaxLength(ToolStatusName.MAX_LENGTH)
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
            .IsUnique()
            .HasFilter("[deleted_at] IS NULL");;

        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}