using Arrapa.ClinicOS.Domain.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arrapa.ClinicOS.Persistence.Configurations;

internal sealed class AuditLogEntryConfiguration : IEntityTypeConfiguration<AuditLogEntry>
{
    public void Configure(EntityTypeBuilder<AuditLogEntry> builder)
    {
        builder.ToTable("AuditLogEntries");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.EntityType).IsRequired().HasMaxLength(200);
        builder.Property(a => a.EntityId).IsRequired().HasMaxLength(64);
        builder.Property(a => a.Operation).HasConversion<int>();
        builder.Property(a => a.PerformedByEmail).HasMaxLength(200);
        builder.Property(a => a.IpAddress).HasMaxLength(45);
        builder.Property(a => a.UserAgent).HasMaxLength(500);
        builder.Property(a => a.CorrelationId).HasMaxLength(64);

        builder.HasIndex(a => a.CreatedOn);
        builder.HasIndex(a => new { a.TenantId, a.CreatedOn });
        builder.HasIndex(a => new { a.EntityType, a.EntityId });

        builder.Property(a => a.RowVersion).IsRowVersion();
        // Audit log entries are never soft-deleted, but keep the filter consistent with other entities.
        builder.HasQueryFilter(a => !a.IsDeleted);

        builder.Ignore(a => a.DomainEvents);
    }
}
