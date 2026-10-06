using Arrapa.ClinicOS.Domain.Subscriptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arrapa.ClinicOS.Persistence.Configurations;

internal sealed class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable("SubscriptionPlans");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Code).IsRequired().HasMaxLength(64);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).HasMaxLength(1000);
        builder.Property(p => p.Interval).HasConversion<int>();
        builder.Property(p => p.Price).HasColumnType("decimal(18,4)");
        builder.Property(p => p.Currency).IsRequired().HasMaxLength(10);
        builder.Property(p => p.FeaturesJson).IsRequired().HasDefaultValue("[]");

        builder.HasIndex(p => p.Code).IsUnique();
        builder.HasIndex(p => p.IsActive);
        builder.HasIndex(p => p.IsDefault);

        builder.Property(p => p.RowVersion).IsRowVersion();
        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.Ignore(p => p.DomainEvents);
    }
}
