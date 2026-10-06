namespace Arrapa.ClinicOS.Domain.Common;

/// <summary>
/// Adds an EF Core-managed row version used for optimistic concurrency control.
/// </summary>
public interface IHasConcurrencyToken
{
    byte[] RowVersion { get; set; }
}
