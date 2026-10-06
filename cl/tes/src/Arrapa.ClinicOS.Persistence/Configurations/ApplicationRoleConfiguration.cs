using Arrapa.ClinicOS.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arrapa.ClinicOS.Persistence.Configurations;

internal sealed class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.ToTable("Roles");

        builder.Property(r => r.Description).HasMaxLength(500);

        builder.HasIndex(r => new { r.TenantId, r.NormalizedName }).IsUnique();
    }
}
