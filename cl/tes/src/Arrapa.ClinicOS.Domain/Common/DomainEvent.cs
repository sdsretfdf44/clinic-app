using MediatR;

namespace Arrapa.ClinicOS.Domain.Common;

/// <summary>
/// Base type for events raised inside the domain layer and dispatched by the persistence pipeline.
/// </summary>
public abstract record DomainEvent : INotification
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
