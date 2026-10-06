using Arrapa.ClinicOS.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arrapa.ClinicOS.Persistence.Configurations;

internal sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Slug).IsRequired().HasMaxLength(64);
        builder.Property(t => t.Status).HasConversion<int>();
        builder.Property(t => t.ContactEmail).HasMaxLength(200);
        builder.Property(t => t.ContactPhone).HasMaxLength(30);
        builder.Property(t => t.LogoUrl).HasMaxLength(500);
        builder.Property(t => t.TimeZone).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Locale).IsRequired().HasMaxLength(10);
        builder.Property(t => t.Currency).IsRequired().HasMaxLength(10);
        builder.Property(t => t.SuspensionReason).HasMaxLength(500);
        builder.Property(t => t.DefaultConsultationFee).HasColumnType("decimal(18,4)");

        builder.HasIndex(t => t.Slug).IsUnique();
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.SubscriptionPlanId);

        builder.Property(t => t.RowVersion).IsRowVersion();

        builder.HasQueryFilter(t => !t.IsDeleted);

        builder.Ignore(t => t.DomainEvents);
    }
}
