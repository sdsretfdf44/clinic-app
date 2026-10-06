using System.ComponentModel.DataAnnotations;

namespace Arrapa.ClinicOS.Domain.Common;

/// <summary>
/// Root entity type. Every persisted entity in the platform inherits from this class and therefore participates in
/// audit, soft delete, optimistic concurrency, and multi-tenant isolation.
/// </summary>
public abstract class BaseEntity : IAuditable, ISoftDelete, IMustHaveTenant, IHasConcurrencyToken
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    public DateTimeOffset CreatedOn { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedOn { get; set; }
    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedOn { get; set; }
    public Guid? DeletedBy { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    private readonly List<DomainEvent> _domainEvents = [];
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void RaiseDomainEvent(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
