using Arrapa.ClinicOS.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arrapa.ClinicOS.Persistence.Configurations;

internal sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(32);
        builder.Property(i => i.Status).HasConversion<int>();
        builder.Property(i => i.CancellationReason).HasMaxLength(500);
        builder.Property(i => i.Notes).HasMaxLength(2000);
        builder.Property(i => i.Currency).IsRequired().HasMaxLength(10);

        builder.Property(i => i.Subtotal).HasColumnType("decimal(18,4)");
        builder.Property(i => i.DiscountAmount).HasColumnType("decimal(18,4)");
        builder.Property(i => i.TaxAmount).HasColumnType("decimal(18,4)");
        builder.Property(i => i.Total).HasColumnType("decimal(18,4)");
        builder.Property(i => i.AmountPaid).HasColumnType("decimal(18,4)");

        builder.Ignore(i => i.AmountDue);
        builder.Ignore(i => i.IsTerminal);
        builder.Ignore(i => i.CanEdit);
        builder.Ignore(i => i.CanPay);
        builder.Ignore(i => i.CanIssue);
        builder.Ignore(i => i.CanCancel);
        builder.Ignore(i => i.DomainEvents);

        builder.HasIndex(i => new { i.TenantId, i.InvoiceNumber }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(i => new { i.TenantId, i.PatientId, i.IssuedAt });
        builder.HasIndex(i => new { i.TenantId, i.Status, i.IssuedAt });
        builder.HasIndex(i => new { i.TenantId, i.AppointmentId });

        builder.Property(i => i.RowVersion).IsRowVersion();

        builder.HasOne(i => i.Patient)
            .WithMany()
            .HasForeignKey(i => i.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Appointment)
            .WithMany()
            .HasForeignKey(i => i.AppointmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(i => i.LineItems)
            .WithOne(l => l.Invoice)
            .HasForeignKey(l => l.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.Payments)
            .WithOne(p => p.Invoice)
            .HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
