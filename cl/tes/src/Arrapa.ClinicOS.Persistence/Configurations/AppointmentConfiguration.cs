using Arrapa.ClinicOS.Domain.Clinical;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arrapa.ClinicOS.Persistence.Configurations;

internal sealed class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.AppointmentNumber).IsRequired().HasMaxLength(32);
        builder.Property(a => a.Status).HasConversion<int>();
        builder.Property(a => a.Reason).HasMaxLength(500);
        builder.Property(a => a.Notes).HasMaxLength(2000);
        builder.Property(a => a.CancellationReason).HasMaxLength(500);
        builder.Property(a => a.ConsultationFee).HasColumnType("decimal(18,4)");
        builder.Property(a => a.Currency).HasMaxLength(10);

        builder.HasIndex(a => new { a.TenantId, a.AppointmentNumber }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(a => new { a.TenantId, a.DoctorId, a.ScheduledStart });
        builder.HasIndex(a => new { a.TenantId, a.PatientId, a.ScheduledStart });
        builder.HasIndex(a => new { a.TenantId, a.Status, a.ScheduledStart });

        builder.Property(a => a.RowVersion).IsRowVersion();

        builder.HasOne(a => a.Patient)
            .WithMany()
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Doctor)
            .WithMany()
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Department)
            .WithMany()
            .HasForeignKey(a => a.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Ignore(a => a.DomainEvents);
        builder.Ignore(a => a.IsTerminal);
    }
}
