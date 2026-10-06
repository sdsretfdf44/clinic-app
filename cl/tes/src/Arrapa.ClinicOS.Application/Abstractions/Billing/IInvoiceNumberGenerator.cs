namespace Arrapa.ClinicOS.Application.Abstractions.Billing;

/// <summary>
/// Produces a unique invoice number for a new invoice inside the given tenant.
/// </summary>
public interface IInvoiceNumberGenerator
{
    Task<string> GenerateAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
