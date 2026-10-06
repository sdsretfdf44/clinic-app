using Arrapa.ClinicOS.Domain.Clinical;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arrapa.ClinicOS.Persistence.Configurations;

internal sealed class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.ToTable("Doctors");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Specialty).HasMaxLength(200);
        builder.Property(d => d.LicenseNumber).HasMaxLength(64);
        builder.Property(d => d.RegistrationNumber).HasMaxLength(64);
        builder.Property(d => d.Bio).HasMaxLength(1000);
        builder.Property(d => d.ConsultationFee).HasColumnType("decimal(18,4)");

        builder.HasIndex(d => new { d.TenantId, d.UserId }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(d => new { d.TenantId, d.DepartmentId });
        builder.HasIndex(d => new { d.TenantId, d.IsActive });

        builder.Property(d => d.RowVersion).IsRowVersion();

        builder.HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(d => d.WorkingHours)
            .WithOne(wh => wh.Doctor)
            .HasForeignKey(wh => wh.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(d => d.DomainEvents);
    }
}
