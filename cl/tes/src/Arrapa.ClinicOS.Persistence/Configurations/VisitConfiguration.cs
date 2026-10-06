using Arrapa.ClinicOS.Domain.Clinical;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arrapa.ClinicOS.Persistence.Configurations;

internal sealed class VisitConfiguration : IEntityTypeConfiguration<Visit>
{
    public void Configure(EntityTypeBuilder<Visit> builder)
    {
        builder.ToTable("Visits");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Symptoms).HasMaxLength(2000);
        builder.Property(v => v.Diagnosis).HasMaxLength(2000);
        builder.Property(v => v.DoctorNotes).HasMaxLength(4000);

        builder.HasIndex(v => new { v.TenantId, v.PatientId, v.VisitDate });
        builder.HasIndex(v => new { v.TenantId, v.DoctorId, v.VisitDate });
        builder.HasIndex(v => v.AppointmentId).IsUnique().HasFilter("[AppointmentId] IS NOT NULL AND [IsDeleted] = 0");

        builder.Property(v => v.RowVersion).IsRowVersion();

        builder.HasOne(v => v.Patient)
            .WithMany()
            .HasForeignKey(v => v.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Doctor)
            .WithMany()
            .HasForeignKey(v => v.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Appointment)
            .WithMany()
            .HasForeignKey(v => v.AppointmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(v => v.Vitals)
            .WithOne(x => x.Visit!)
            .HasForeignKey<VisitVitals>(x => x.VisitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(v => v.Prescriptions)
            .WithOne(p => p.Visit)
            .HasForeignKey(p => p.VisitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(v => v.DomainEvents);
    }
}

internal sealed class VisitVitalsConfiguration : IEntityTypeConfiguration<VisitVitals>
{
    public void Configure(EntityTypeBuilder<VisitVitals> builder)
    {
        builder.ToTable("VisitVitals");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.TemperatureCelsius).HasColumnType("decimal(5,2)");
        builder.Property(v => v.HeightCm).HasColumnType("decimal(6,2)");
        builder.Property(v => v.WeightKg).HasColumnType("decimal(6,2)");
        builder.Property(v => v.Bmi).HasColumnType("decimal(5,2)");
        builder.Property(v => v.Notes).HasMaxLength(1000);

        builder.HasIndex(v => new { v.TenantId, v.VisitId }).IsUnique().HasFilter("[IsDeleted] = 0");

        builder.Property(v => v.RowVersion).IsRowVersion();
        builder.Ignore(v => v.DomainEvents);
    }
}

internal sealed class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
{
    public void Configure(EntityTypeBuilder<Prescription> builder)
    {
        builder.ToTable("Prescriptions");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.GeneralInstructions).HasMaxLength(2000);

        builder.HasIndex(p => new { p.TenantId, p.VisitId, p.IssuedOn });

        builder.Property(p => p.RowVersion).IsRowVersion();

        builder.HasMany(p => p.Items)
            .WithOne(i => i.Prescription)
            .HasForeignKey(i => i.PrescriptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(p => p.DomainEvents);
    }
}

internal sealed class PrescriptionItemConfiguration : IEntityTypeConfiguration<PrescriptionItem>
{
    public void Configure(EntityTypeBuilder<PrescriptionItem> builder)
    {
        builder.ToTable("PrescriptionItems");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Medicine).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Strength).HasMaxLength(100);
        builder.Property(p => p.Dosage).HasMaxLength(100);
        builder.Property(p => p.Frequency).HasMaxLength(100);
        builder.Property(p => p.Duration).HasMaxLength(100);
        builder.Property(p => p.Instructions).HasMaxLength(500);

        builder.HasIndex(p => new { p.TenantId, p.PrescriptionId, p.SortOrder });

        builder.Property(p => p.RowVersion).IsRowVersion();
        builder.Ignore(p => p.DomainEvents);
    }
}
