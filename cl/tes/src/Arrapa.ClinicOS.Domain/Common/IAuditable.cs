namespace Arrapa.ClinicOS.Domain.Common;

/// <summary>
/// Adds a full audit trail (created / updated) to an entity.
/// </summary>
public interface IAuditable
{
    DateTimeOffset CreatedOn { get; set; }
    Guid? CreatedBy { get; set; }
    DateTimeOffset? UpdatedOn { get; set; }
    Guid? UpdatedBy { get; set; }
}
