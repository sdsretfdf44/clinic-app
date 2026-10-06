using Arrapa.ClinicOS.Domain.Clinical;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arrapa.ClinicOS.Persistence.Configurations;

internal sealed class ReceptionistConfiguration : IEntityTypeConfiguration<Receptionist>
{
    public void Configure(EntityTypeBuilder<Receptionist> builder)
    {
        builder.ToTable("Receptionists");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Title).HasMaxLength(200);

        builder.HasIndex(r => new { r.TenantId, r.UserId }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(r => new { r.TenantId, r.IsActive });

        builder.Property(r => r.RowVersion).IsRowVersion();

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(r => r.DomainEvents);
    }
}
