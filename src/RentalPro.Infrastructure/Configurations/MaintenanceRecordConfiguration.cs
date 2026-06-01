using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalPro.Domain.MaintenanceRecords;
using RentalPro.Domain.Tools;
using RentalPro.Domain.Users;
using RentalPro.Domain.ValueObjects;

namespace RentalPro.Infrastructure.Configurations;

public sealed class MaintenanceRecordConfiguration
    : IEntityTypeConfiguration<MaintenanceRecord>
{
    public void Configure(EntityTypeBuilder<MaintenanceRecord> builder)
    {
        builder.ToTable("maintenance_records");

        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => MaintenanceRecordId.Restore(value));

        builder
            .Property(x => x.ToolId)
            .HasColumnName("tool_id")
            .HasConversion(
                id => id.Value,
                value => ToolId.Restore(value))
            .IsRequired();

        builder
            .Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasConversion(
                id => id.Value,
                value => UserId.Restore(value))
            .IsRequired();

        builder
            .Property(x => x.StatusId)
            .HasColumnName("status_id")
            .HasConversion(
                id => id.Value,
                value => MaintenanceStatusId.Restore(value))
            .IsRequired();

        builder
            .Property(x => x.StartDate)
            .HasColumnName("start_date")
            .IsRequired();

        builder
            .Property(x => x.EndDate)
            .HasColumnName("end_date");

        builder
            .Property(x => x.Description)
            .HasColumnName("description")
            .HasConversion(
                description => description.Value,
                value => Description.Create(value).Value)
            .HasMaxLength(Description.MAX_LENGTH)
            .IsRequired();

        builder
            .Property(x => x.Cost)
            .HasColumnName("cost")
            .HasConversion(
                v => v != null ? v.Value : (decimal?)null,
                v => v.HasValue
                    ? Money.Create(v.Value).Value
                    : null)
            .HasPrecision(18, 2);

        builder
            .Property(x => x.Result)
            .HasColumnName("result")
            .HasConversion(
                result => result == null ? null : result.Value,
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
            .HasOne<Tool>()
            .WithMany()
            .HasForeignKey(x => x.ToolId);

        builder
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId);

        builder
            .HasOne<MaintenanceStatus>()
            .WithMany()
            .HasForeignKey(x => x.StatusId);

        builder
            .HasIndex(x => x.ToolId);

        builder
            .HasIndex(x => x.UserId);

        builder
            .HasIndex(x => x.StatusId);

        builder
            .HasIndex(x => x.StartDate);

        builder
            .HasIndex(x => x.EndDate);

        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}