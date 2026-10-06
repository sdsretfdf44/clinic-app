using Arrapa.ClinicOS.Domain.Clinical;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arrapa.ClinicOS.Persistence.Configurations;

internal sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Name).IsRequired().HasMaxLength(200);
        builder.Property(d => d.Description).HasMaxLength(500);
        builder.Property(d => d.DefaultConsultationFee).HasColumnType("decimal(18,4)");

        builder.HasIndex(d => new { d.TenantId, d.Name }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(d => new { d.TenantId, d.IsActive });

        builder.Property(d => d.RowVersion).IsRowVersion();

        builder.HasMany(d => d.Doctors)
            .WithOne(doc => doc.Department)
            .HasForeignKey(doc => doc.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Ignore(d => d.DomainEvents);
    }
}
