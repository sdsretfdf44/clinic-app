using Arrapa.ClinicOS.Domain.FeatureFlags;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arrapa.ClinicOS.Persistence.Configurations;

internal sealed class FeatureFlagConfiguration : IEntityTypeConfiguration<FeatureFlag>
{
    public void Configure(EntityTypeBuilder<FeatureFlag> builder)
    {
        builder.ToTable("FeatureFlags");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Key).IsRequired().HasMaxLength(128);
        builder.Property(f => f.Name).IsRequired().HasMaxLength(200);
        builder.Property(f => f.Description).HasMaxLength(1000);

        builder.HasIndex(f => f.Key).IsUnique();

        builder.HasMany(f => f.TenantOverrides)
            .WithOne(o => o.FeatureFlag)
            .HasForeignKey(o => o.FeatureFlagId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(f => f.RowVersion).IsRowVersion();
        builder.HasQueryFilter(f => !f.IsDeleted);

        builder.Ignore(f => f.DomainEvents);
    }
}

internal sealed class TenantFeatureFlagConfiguration : IEntityTypeConfiguration<TenantFeatureFlag>
{
    public void Configure(EntityTypeBuilder<TenantFeatureFlag> builder)
    {
        builder.ToTable("TenantFeatureFlags");

        builder.HasKey(t => t.Id);

        builder.HasIndex(t => new { t.TenantId, t.FeatureFlagId }).IsUnique();

        builder.Property(t => t.RowVersion).IsRowVersion();
        builder.HasQueryFilter(t => !t.IsDeleted);

        builder.Ignore(t => t.DomainEvents);
    }
}
