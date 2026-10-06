using Arrapa.ClinicOS.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arrapa.ClinicOS.Persistence.Configurations;

internal sealed class InvoiceLineItemConfiguration : IEntityTypeConfiguration<InvoiceLineItem>
{
    public void Configure(EntityTypeBuilder<InvoiceLineItem> builder)
    {
        builder.ToTable("InvoiceLineItems");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Description).IsRequired().HasMaxLength(300);
        builder.Property(l => l.ServiceCode).HasMaxLength(50);

        builder.Property(l => l.Quantity).HasColumnType("decimal(18,4)");
        builder.Property(l => l.UnitPrice).HasColumnType("decimal(18,4)");
        builder.Property(l => l.LineTotal).HasColumnType("decimal(18,4)");

        builder.Property(l => l.RowVersion).IsRowVersion();
        builder.Ignore(l => l.DomainEvents);

        builder.HasIndex(l => new { l.TenantId, l.InvoiceId });
    }
}
