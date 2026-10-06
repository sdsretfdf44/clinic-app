namespace Arrapa.ClinicOS.Domain.Common;

/// <summary>
/// Entities implementing this interface are soft-deleted (never removed) by the persistence layer.
/// </summary>
public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedOn { get; set; }
    Guid? DeletedBy { get; set; }
}
