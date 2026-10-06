using Arrapa.ClinicOS.Domain.Clinical;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arrapa.ClinicOS.Persistence.Configurations;

internal sealed class WorkingHoursConfiguration : IEntityTypeConfiguration<WorkingHours>
{
    public void Configure(EntityTypeBuilder<WorkingHours> builder)
    {
        builder.ToTable("WorkingHours");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.DayOfWeek).HasConversion<int>();
        builder.Property(w => w.StartTime);
        builder.Property(w => w.EndTime);

        builder.HasIndex(w => new { w.TenantId, w.DoctorId, w.DayOfWeek });

        builder.Property(w => w.RowVersion).IsRowVersion();

        builder.Ignore(w => w.DomainEvents);
    }
}
