using Arrapa.ClinicOS.Domain.Clinical;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arrapa.ClinicOS.Persistence.Configurations;

internal sealed class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.MRN).IsRequired().HasMaxLength(32);
        builder.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(p => p.MiddleName).HasMaxLength(100);
        builder.Property(p => p.LastName).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Gender).HasConversion<int>();
        builder.Property(p => p.BloodGroup).HasConversion<int>();
        builder.Property(p => p.MaritalStatus).HasConversion<int>();

        builder.Property(p => p.Phone).HasMaxLength(30);
        builder.Property(p => p.Email).HasMaxLength(200);
        builder.Property(p => p.PhotoUrl).HasMaxLength(500);

        builder.Property(p => p.AddressLine1).HasMaxLength(200);
        builder.Property(p => p.AddressLine2).HasMaxLength(200);
        builder.Property(p => p.City).HasMaxLength(100);
        builder.Property(p => p.State).HasMaxLength(100);
        builder.Property(p => p.Country).HasMaxLength(100);
        builder.Property(p => p.PostalCode).HasMaxLength(20);
        builder.Property(p => p.Occupation).HasMaxLength(200);

        builder.Property(p => p.EmergencyContactName).HasMaxLength(200);
        builder.Property(p => p.EmergencyContactPhone).HasMaxLength(30);
        builder.Property(p => p.EmergencyContactRelationship).HasMaxLength(100);

        builder.Property(p => p.GuardianName).HasMaxLength(200);
        builder.Property(p => p.GuardianPhone).HasMaxLength(30);
        builder.Property(p => p.GuardianRelationship).HasMaxLength(100);

        builder.Property(p => p.InsuranceProvider).HasMaxLength(200);
        builder.Property(p => p.InsurancePolicyNumber).HasMaxLength(100);
        builder.Property(p => p.InsuranceGroupNumber).HasMaxLength(100);

        builder.Property(p => p.Notes).HasMaxLength(4000);

        builder.HasIndex(p => new { p.TenantId, p.MRN }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(p => new { p.TenantId, p.LastName, p.FirstName });
        builder.HasIndex(p => new { p.TenantId, p.Phone });
        builder.HasIndex(p => new { p.TenantId, p.IsActive });
        builder.HasIndex(p => new { p.TenantId, p.UserId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0 AND [UserId] IS NOT NULL");

        builder.Property(p => p.RowVersion).IsRowVersion();

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Allergies)
            .WithOne(a => a.Patient)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.MedicalHistory)
            .WithOne(m => m.Patient)
            .HasForeignKey(m => m.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.CurrentMedications)
            .WithOne(m => m.Patient)
            .HasForeignKey(m => m.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(p => p.DomainEvents);
        builder.Ignore(p => p.FullName);
    }
}

internal sealed class PatientAllergyConfiguration : IEntityTypeConfiguration<PatientAllergy>
{
    public void Configure(EntityTypeBuilder<PatientAllergy> builder)
    {
        builder.ToTable("PatientAllergies");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Name).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Severity).HasConversion<int>();
        builder.Property(a => a.ReactionDescription).HasMaxLength(500);

        builder.HasIndex(a => new { a.TenantId, a.PatientId });

        builder.Property(a => a.RowVersion).IsRowVersion();
        builder.Ignore(a => a.DomainEvents);
    }
}

internal sealed class MedicalHistoryEntryConfiguration : IEntityTypeConfiguration<MedicalHistoryEntry>
{
    public void Configure(EntityTypeBuilder<MedicalHistoryEntry> builder)
    {
        builder.ToTable("MedicalHistoryEntries");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Condition).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Notes).HasMaxLength(1000);

        builder.HasIndex(m => new { m.TenantId, m.PatientId });

        builder.Property(m => m.RowVersion).IsRowVersion();
        builder.Ignore(m => m.DomainEvents);
    }
}

internal sealed class PatientMedicationConfiguration : IEntityTypeConfiguration<PatientMedication>
{
    public void Configure(EntityTypeBuilder<PatientMedication> builder)
    {
        builder.ToTable("PatientMedications");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Name).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Dosage).HasMaxLength(100);
        builder.Property(m => m.Frequency).HasMaxLength(100);
        builder.Property(m => m.Notes).HasMaxLength(500);

        builder.HasIndex(m => new { m.TenantId, m.PatientId, m.IsActive });

        builder.Property(m => m.RowVersion).IsRowVersion();
        builder.Ignore(m => m.DomainEvents);
    }
}
