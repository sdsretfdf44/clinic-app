using Arrapa.ClinicOS.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arrapa.ClinicOS.Persistence.Configurations;

internal sealed class NotificationLogConfiguration : IEntityTypeConfiguration<NotificationLog>
{
    public void Configure(EntityTypeBuilder<NotificationLog> builder)
    {
        builder.ToTable("NotificationLogs");
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Kind).HasConversion<int>();
        builder.Property(n => n.Channel).HasConversion<int>();
        builder.Property(n => n.Status).HasConversion<int>();

        builder.Property(n => n.Recipient).IsRequired().HasMaxLength(200);
        builder.Property(n => n.Body).IsRequired().HasMaxLength(2000);
        builder.Property(n => n.ProviderMessageId).HasMaxLength(100);
        builder.Property(n => n.ErrorMessage).HasMaxLength(1000);

        builder.Property(n => n.RowVersion).IsRowVersion();
        builder.Ignore(n => n.DomainEvents);

        builder.HasOne(n => n.Appointment)
            .WithMany()
            .HasForeignKey(n => n.AppointmentId)
            .OnDelete(DeleteBehavior.SetNull);

        // Prevents the reminder scheduler from double-sending the same kind for the same appointment.
        builder.HasIndex(n => new { n.TenantId, n.AppointmentId, n.Kind }).IsUnique()
            .HasFilter("[AppointmentId] IS NOT NULL");

        builder.HasIndex(n => new { n.TenantId, n.CreatedOn });
    }
}
