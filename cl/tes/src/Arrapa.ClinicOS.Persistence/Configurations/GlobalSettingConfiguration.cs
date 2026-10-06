using Arrapa.ClinicOS.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arrapa.ClinicOS.Persistence.Configurations;

internal sealed class GlobalSettingConfiguration : IEntityTypeConfiguration<GlobalSetting>
{
    public void Configure(EntityTypeBuilder<GlobalSetting> builder)
    {
        builder.ToTable("GlobalSettings");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Key).IsRequired().HasMaxLength(128);
        builder.Property(s => s.Value).IsRequired().HasMaxLength(4000);
        builder.Property(s => s.Category).IsRequired().HasMaxLength(64);
        builder.Property(s => s.DataType).HasConversion<int>();
        builder.Property(s => s.Description).HasMaxLength(500);

        builder.HasIndex(s => s.Key).IsUnique();
        builder.HasIndex(s => s.Category);

        builder.Property(s => s.RowVersion).IsRowVersion();
        builder.HasQueryFilter(s => !s.IsDeleted);

        builder.Ignore(s => s.DomainEvents);
    }
}
