using Arrapa.ClinicOS.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arrapa.ClinicOS.Persistence.Configurations;

internal sealed class TenantSettingConfiguration : IEntityTypeConfiguration<TenantSetting>
{
    public void Configure(EntityTypeBuilder<TenantSetting> builder)
    {
        builder.ToTable("TenantSettings");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Key).IsRequired().HasMaxLength(128);
        builder.Property(s => s.Value).IsRequired().HasMaxLength(4000);
        builder.Property(s => s.Category).IsRequired().HasMaxLength(64);
        builder.Property(s => s.DataType).HasConversion<int>();
        builder.Property(s => s.Description).HasMaxLength(500);

        builder.HasIndex(s => new { s.TenantId, s.Key }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(s => new { s.TenantId, s.Category });

        builder.Property(s => s.RowVersion).IsRowVersion();

        builder.Ignore(s => s.DomainEvents);
    }
}
