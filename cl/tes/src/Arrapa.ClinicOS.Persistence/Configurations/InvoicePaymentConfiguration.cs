using Arrapa.ClinicOS.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arrapa.ClinicOS.Persistence.Configurations;

internal sealed class InvoicePaymentConfiguration : IEntityTypeConfiguration<InvoicePayment>
{
    public void Configure(EntityTypeBuilder<InvoicePayment> builder)
    {
        builder.ToTable("InvoicePayments");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Amount).HasColumnType("decimal(18,4)");
        builder.Property(p => p.Method).HasConversion<int>();
        builder.Property(p => p.Reference).HasMaxLength(100);
        builder.Property(p => p.Notes).HasMaxLength(500);

        builder.Property(p => p.RowVersion).IsRowVersion();
        builder.Ignore(p => p.DomainEvents);

        builder.HasIndex(p => new { p.TenantId, p.InvoiceId, p.PaidAt });
    }
}
