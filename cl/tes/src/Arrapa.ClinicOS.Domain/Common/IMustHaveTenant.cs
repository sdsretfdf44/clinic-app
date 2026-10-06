namespace Arrapa.ClinicOS.Domain.Common;

/// <summary>
/// Entities implementing this interface are tenant-scoped. A global query filter guarantees isolation.
/// </summary>
public interface IMustHaveTenant
{
    Guid TenantId { get; set; }
}
